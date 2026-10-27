using Microsoft.EntityFrameworkCore;
using Panis360.Application;
using Panis360.Contracts;
using Panis360.Domain;
using Panis360.Infrastructure;
if(args.Length==2 && args[0]=="--schema") {
 using var pg=new AppDb(new DbContextOptionsBuilder<AppDb>().UseNpgsql("Host=localhost;Database=panis360").Options);
 await File.WriteAllTextAsync(args[1],pg.Database.GenerateCreateScript());Console.WriteLine("Esquema PostgreSQL gerado.");return;
}
int passed=0;
void Check(bool value,string name){if(!value)throw new Exception("FALHOU: "+name);Console.WriteLine("OK: "+name);passed++;}
async Task Reject(Func<Task> action,string name){try{await action();}catch(BusinessException){Check(true,name);return;}throw new Exception("Era esperada rejeição: "+name);}
var split=Rules.Split(235m,[new(7,50),new(14,50)]);Check(split[0].Amount==117.50m&&split[1].Amount==117.50m,"Parcelamento 7/14 da imagem");
var thirds=Rules.Split(100m,[new(7,33.33m),new(14,33.33m),new(21,33.34m)]);Check(thirds.Sum(x=>x.Amount)==100m&&thirds[^1].Amount==33.34m,"Soma e ajuste de centavos");
await Reject(()=>{Rules.Split(10,[new(7,50)]);return Task.CompletedTask;},"Percentuais incompletos rejeitados");
var path=Path.Combine(Path.GetTempPath(),"panis-tests-"+Guid.NewGuid()+".db");
var options=new DbContextOptionsBuilder<AppDb>().UseSqlite("Data Source="+path).Options;
var seller=new User{Name="Vendedor",Email="seller@test.local"};var customer=new Customer{Name="Padaria teste",Document="12345678901",City="Foz",SellerId=seller.Id};
var product=new Product{Name="Fermento",Price=23.50m};var term=new PaymentTerm{Name="7/14",Parts=[new(){Days=7,Percent=50},new(){Days=14,Percent=50}]};
try {
 await using(var db=new AppDb(options)){await db.Database.EnsureCreatedAsync();db.Users.Add(seller);db.Customers.Add(customer);db.Products.Add(product);db.Terms.Add(term);db.Lots.AddRange(new StockLot{ProductId=product.Id,Code="Vencido",ExpiresOn=Operations.Today.AddDays(-1),Quantity=20},new StockLot{ProductId=product.Id,Code="Primeiro",ExpiresOn=Operations.Today.AddDays(1),Quantity=6},new StockLot{ProductId=product.Id,Code="Segundo",ExpiresOn=Operations.Today.AddDays(2),Quantity=4});await db.SaveChangesAsync();}
 var request=new OrderInput{CustomerId=customer.Id,PaymentTermId=term.Id,Items=[new(product.Id,12)]};Guid orderId;
 await using(var db=new AppDb(options)){orderId=await new Operations(db).CreateOrder(request,seller.Id,false);}
 await using(var db=new AppDb(options)){Check(await new Operations(db).CreateOrder(request,seller.Id,false)==orderId,"Reenvio não duplica pedido");}
 await using(var db=new AppDb(options)){await Reject(()=>new Operations(db).CreateOrder(request,Guid.NewGuid(),false),"Reenvio de outro vendedor rejeitado");}
 await using(var db=new AppDb(options)){await Reject(async()=>{await new Operations(db).ConfirmSale(orderId,seller.Id);},"Venda acima do estoque rejeitada");}
 await using(var db=new AppDb(options)){Check(await db.Sales.CountAsync()==0&&await db.Receivables.CountAsync()==0,"Falha não gera venda ou títulos");}
 await using(var db=new AppDb(options)){var order=await db.Orders.Include(x=>x.Items).SingleAsync();await new Operations(db).Cut(orderId,new CutInput("Falta de estoque",[new(order.Items[0].Id,10)]),seller.Id);Check(order.Items[0].Requested==12,"Corte preserva pedido original");}
 Guid saleId;
 await using(var db=new AppDb(options)){saleId=await new Operations(db).ConfirmSale(orderId,seller.Id);}
 await using(var db=new AppDb(options)){
  Check((await db.Sales.SingleAsync()).Total==235m,"Venda usa quantidade cortada");
  var lots=await db.Lots.ToListAsync();Check(lots.Single(x=>x.Code=="Vencido").Quantity==20&&lots.Where(x=>x.Code!="Vencido").Sum(x=>x.Quantity)==0,"FEFO e exclusão de lotes vencidos");
  var titles=await db.Receivables.OrderBy(x=>x.Number).ToListAsync();Check(titles.Count==2&&titles.All(x=>x.Amount==117.50m)&&titles[0].DueDate==Operations.Today.AddDays(7),"Venda gera parcelas e vencimentos");
  Check(await new Operations(db).ConfirmSale(orderId,seller.Id)==saleId&&await db.Sales.CountAsync()==1,"Confirmar novamente não duplica venda");
 }
 Guid titleId;await using(var db=new AppDb(options)){titleId=await db.Receivables.Select(x=>x.Id).FirstAsync();}
 var receipt=new ReceiptInput(Guid.NewGuid(),50m);
 await using(var db=new AppDb(options)){await new Operations(db).Receive(titleId,receipt,seller.Id);}
 await using(var db=new AppDb(options)){await new Operations(db).Receive(titleId,receipt,seller.Id);Check((await db.Receivables.FindAsync(titleId))!.Paid==50&&await db.Receipts.CountAsync()==1,"Recebimento parcial e reenvio idempotente");}
 await using(var db=new AppDb(options)){await Reject(()=>new Operations(db).Receive(titleId,new(Guid.NewGuid(),100),seller.Id),"Recebimento acima do saldo rejeitado");}
 await using(var db=new AppDb(options)){var c=await db.Customers.SingleAsync();c.Blocked=true;await db.SaveChangesAsync();request.ClientRequestId=Guid.NewGuid();await Reject(async()=>{await new Operations(db).CreateOrder(request,seller.Id,false);},"Cliente bloqueado não gera novo pedido");}
 Console.WriteLine($"Concluído: {passed} verificações aprovadas.");
} finally { Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();foreach(var suffix in new[]{"","-wal","-shm"})File.Delete(path+suffix); }
