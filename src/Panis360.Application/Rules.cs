using Panis360.Contracts;
namespace Panis360.Application;
public sealed class BusinessException(string message) : Exception(message);
public static class Rules {
 public static void Require(bool condition, string message) { if (!condition) throw new BusinessException(message); }
 public static decimal Money(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
 public static List<(int Days, decimal Amount)> Split(decimal total, IEnumerable<TermPartDto> parts) {
  var list=parts.ToList();
  Require(total>0 && list.Count>0 && list.Count<=24,"Informe total positivo e de 1 a 24 parcelas.");
  Require(list.All(p=>p.Days>=0 && p.Days<=3650 && p.Percent>0) && list.Sum(p=>p.Percent)==100m,"Percentuais devem somar 100%; prazos devem ser válidos.");
  var result=new List<(int,decimal)>(); decimal used=0;
  for(var i=0;i<list.Count;i++) {
   var amount=i==list.Count-1 ? total-used : Money(total*list[i].Percent/100m);
   Require(amount>0,"Total insuficiente para a quantidade de parcelas.");
   result.Add((list[i].Days,amount)); used+=amount;
  }
  return result;
 }
 public static void Quantity(decimal q) => Require(q>0 && q<=1000000 && decimal.Round(q,3)==q,"Quantidade deve ser positiva, até 1.000.000, com até 3 casas decimais.");
 public static readonly string[] PaymentMethods=["Boleto","PIX","Dinheiro","Cartão"];
 public static readonly string[] Roles=["Administrador","Vendedor","Estoque","Financeiro"];
}
