using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Panis360.Application;
using Panis360.Contracts;
using Panis360.Domain;
namespace Panis360.Infrastructure;
public class Operations(AppDb db) {
 public void Audit(Guid actor,string action,Guid target)=>db.Audits.Add(new Audit{ActorId=actor,Action=action,TargetId=target});
 public static DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow,"America/Sao_Paulo"));
 public async Task<Guid> CreateOrder(OrderInput input,Guid actor,bool admin) {
  var hash=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(input))));
  var existing=await db.Orders.SingleOrDefaultAsync(x=>x.ClientRequestId==input.ClientRequestId);
  if(existing!=null) { Rules.Require((admin||existing.SellerId==actor)&&existing.RequestHash==hash,"Identificador já utilizado por outro pedido."); return existing.Id; }
  Rules.Require(input.ClientRequestId!=Guid.Empty,"Identificador do pedido obrigatório.");
  var customer=await db.Customers.FindAsync(input.CustomerId) ?? throw new BusinessException("Cliente inexistente.");
  Rules.Require(admin||customer.SellerId==actor,"Cliente fora da carteira do vendedor.");
  Rules.Require(!customer.Blocked,"Cliente bloqueado para novas compras.");
  Rules.Require(await db.Terms.AnyAsync(x=>x.Id==input.PaymentTermId),"Condição de pagamento inválida.");
  Rules.Require(Rules.PaymentMethods.Contains(input.PaymentMethod),"Forma de pagamento inválida.");
  Rules.Require(input.Items.Count>0 && input.Items.Count<=200,"Pedido deve ter de 1 a 200 itens.");
  Rules.Require(input.Items.Select(x=>x.ProductId).Distinct().Count()==input.Items.Count,"Produto repetido no pedido.");
  var order=new Order{ClientRequestId=input.ClientRequestId,RequestHash=hash,CustomerId=customer.Id,SellerId=customer.SellerId,PaymentTermId=input.PaymentTermId,PaymentMethod=input.PaymentMethod};
  foreach(var line in input.Items) {
   Rules.Quantity(line.Quantity);
   var p=await db.Products.FindAsync(line.ProductId) ?? throw new BusinessException("Produto inexistente.");
   Rules.Require(p.Active,"Produto inativo.");
   if(p.Unit=="UN") Rules.Require(decimal.Truncate(line.Quantity)==line.Quantity,"Produto em UN exige quantidade inteira.");
   order.Items.Add(new OrderItem{ProductId=p.Id,ProductName=p.Name,Requested=line.Quantity,Accepted=line.Quantity,UnitPrice=p.Price});
  }
  db.Orders.Add(order); Audit(actor,"Pedido recebido",order.Id); await db.SaveChangesAsync(); return order.Id;
 }
 public async Task Cut(Guid id,CutInput input,Guid actor) {
  var order=await db.Orders.Include(x=>x.Items).SingleOrDefaultAsync(x=>x.Id==id) ?? throw new BusinessException("Pedido inexistente.");
  Rules.Require(order.Status=="Recebido","Somente pedido recebido pode ser ajustado.");
  Rules.Require(!string.IsNullOrWhiteSpace(input.Reason),"Informe o motivo do ajuste.");
  Rules.Require(input.Items.Count==order.Items.Count && input.Items.Select(x=>x.ItemId).Distinct().Count()==order.Items.Count,"Informe todos os itens, sem repetição.");
  foreach(var line in input.Items) {
   var item=order.Items.SingleOrDefault(x=>x.Id==line.ItemId) ?? throw new BusinessException("Item inválido.");
   Rules.Require(line.Accepted>=0 && line.Accepted<=item.Requested && decimal.Round(line.Accepted,3)==line.Accepted,"Quantidade atendida inválida.");
   var product=await db.Products.FindAsync(item.ProductId);
   Rules.Require(product!.Unit!="UN" || decimal.Truncate(line.Accepted)==line.Accepted,"Produto em UN exige quantidade inteira.");
   item.Accepted=line.Accepted;
  }
  order.CutReason=input.Reason; order.Revision++; Audit(actor,"Corte ajustado",id); await db.SaveChangesAsync();
 }
 public async Task<Guid> ConfirmSale(Guid id,Guid actor) {
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
  var order=await db.Orders.Include(x=>x.Items).SingleOrDefaultAsync(x=>x.Id==id) ?? throw new BusinessException("Pedido inexistente.");
  if(order.Status=="Vendido") { var prior=await db.Sales.SingleAsync(x=>x.OrderId==id); return prior.Id; }
  Rules.Require(order.Status=="Recebido","Pedido não pode ser vendido.");
  Rules.Require(!await db.Customers.Where(x=>x.Id==order.CustomerId).Select(x=>x.Blocked).SingleAsync(),"Cliente bloqueado.");
  var total=order.Items.Sum(x=>Rules.Money(x.Accepted*x.UnitPrice));
  Rules.Require(total>0,"Pedido sem quantidade atendida.");
  var term=await db.Terms.Include(x=>x.Parts).SingleAsync(x=>x.Id==order.PaymentTermId);
  var split=Rules.Split(total,term.Parts.OrderBy(x=>x.Days).Select(x=>new TermPartDto(x.Days,x.Percent)));
  var sale=new Sale{OrderId=id,CustomerId=order.CustomerId,Date=Today,Total=total};
  foreach(var item in order.Items.Where(x=>x.Accepted>0)) {
   var lots=await db.Lots.Where(x=>x.ProductId==item.ProductId && x.ExpiresOn>=Today).OrderBy(x=>x.ExpiresOn).ThenBy(x=>x.Id).ToListAsync();
   Rules.Require(lots.Sum(x=>x.Quantity)>=item.Accepted,$"Estoque insuficiente para {item.ProductName}. Ajuste o corte.");
   var remaining=item.Accepted;
   foreach(var lot in lots.Where(x=>x.Quantity>0)) {
    var used=Math.Min(lot.Quantity,remaining); lot.Quantity-=used;lot.Revision++; remaining-=used;
    db.Movements.Add(new StockMovement{StockLotId=lot.Id,SaleId=sale.Id,ActorId=actor,Quantity=-used,Reason="Venda"});
    if(remaining==0) break;
   }
  }
  db.Sales.Add(sale); int number=0;
  foreach(var part in split) db.Receivables.Add(new Receivable{SaleId=sale.Id,CustomerId=order.CustomerId,Number=++number,DueDate=Today.AddDays(part.Days),Amount=part.Amount,PaymentMethod=order.PaymentMethod});
  order.Status="Vendido";order.Revision++; Audit(actor,"Venda confirmada",sale.Id);
  await db.SaveChangesAsync();await tx.CommitAsync();return sale.Id;
 }
 public async Task Receive(Guid id,ReceiptInput input,Guid actor) {
  Rules.Require(input.RequestId!=Guid.Empty,"Identificador obrigatório.");
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
  var previous=await db.Receipts.SingleOrDefaultAsync(x=>x.RequestId==input.RequestId);
  if(previous!=null) { Rules.Require(previous.ReceivableId==id && previous.Amount==input.Amount,"Identificador já usado em outro recebimento.");return; }
  var r=await db.Receivables.FindAsync(id) ?? throw new BusinessException("Título inexistente.");
  Rules.Require(input.Amount>0 && Rules.Money(input.Amount)==input.Amount && input.Amount<=r.Amount-r.Paid,"Valor inválido ou acima do saldo.");
  r.Paid+=input.Amount;r.Revision++;
  db.Receipts.Add(new Receipt{ReceivableId=id,RequestId=input.RequestId,Amount=input.Amount});
  Audit(actor,"Recebimento registrado",id); await db.SaveChangesAsync();await tx.CommitAsync();
 }
}
