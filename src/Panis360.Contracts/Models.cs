using System.ComponentModel.DataAnnotations;
namespace Panis360.Contracts;
public record LoginRequest(string Email, string Password);
public record LoginResult(string Token, Guid UserId, string Name, string Role, DateTime ExpiresAt);
public record SellerDto(Guid Id, string Name);
public class UserInput {
 [Required] public string Name { get; set; } = "";
 [Required,EmailAddress] public string Email { get; set; } = "";
 [Required,MinLength(12)] public string Password { get; set; } = "";
 public string Role { get; set; } = "Vendedor";
}
public class CustomerInput {
 [Required,StringLength(120)] public string Name { get; set; } = "";
 [Required] public string Document { get; set; } = "";
 public string Phone { get; set; } = "";
 public string Street { get; set; } = "";
 public string Number { get; set; } = "";
 public string Neighborhood { get; set; } = "";
 [Required] public string City { get; set; } = "Foz do Iguaçu";
 [Required,StringLength(2,MinimumLength=2)] public string State { get; set; } = "PR";
 public string ZipCode { get; set; } = "";
 public Guid SellerId { get; set; }
 public int VisitOrder { get; set; }
 public bool Blocked { get; set; }
}
public record CustomerDto(Guid Id, string Name, string Document, string Phone, string City, string State, Guid SellerId, int VisitOrder, bool Blocked);
public class ProductInput {
 [Required] public string Name { get; set; } = "";
 public string Group { get; set; } = "Insumos";
 public string Unit { get; set; } = "UN";
 [Range(typeof(decimal),"0.01","1000000",ParseLimitsInInvariantCulture=true)] public decimal Price { get; set; }
 public bool Active { get; set; } = true;
}
public record ProductDto(Guid Id, string Name, string Group, string Unit, decimal Price, decimal Available);
public record LotInput(Guid ProductId, string Code, DateOnly ExpiresOn, decimal Quantity);
public record LotDto(Guid Id, Guid ProductId, string ProductName, string Code, DateOnly ExpiresOn, decimal Quantity, string Unit, int Revision);
public record MovementDto(DateTime Date, decimal Quantity, decimal Balance, string Reason, Guid? SaleId, string User);
public record LotEditInput(string Code, DateOnly ExpiresOn, decimal Quantity, string Reason, int Revision);
public record TermPartDto(int Days, decimal Percent);
public record TermInput(string Name, List<TermPartDto> Parts);
public record TermDto(Guid Id, string Name, List<TermPartDto> Parts);
public record OrderLineInput(Guid ProductId, decimal Quantity);
public class OrderInput {
 public Guid ClientRequestId { get; set; } = Guid.NewGuid();
 public Guid CustomerId { get; set; }
 public Guid PaymentTermId { get; set; }
 public string PaymentMethod { get; set; } = "Boleto";
 public List<OrderLineInput> Items { get; set; } = [];
}
public record OrderLineDto(Guid Id, Guid ProductId, string ProductName, decimal Requested, decimal Accepted, decimal UnitPrice);
public record OrderDto(Guid Id, Guid CustomerId, string CustomerName, DateTime CreatedAt, string Status, string CutReason, decimal Total, List<OrderLineDto> Items);
public record CutLine(Guid ItemId, decimal Accepted);
public record CutInput(string Reason, List<CutLine> Items);
public record ReceivableDto(Guid Id, Guid CustomerId, string CustomerName, int Number, DateOnly DueDate, decimal Amount, decimal Paid, string PaymentMethod) { public decimal Balance => Amount-Paid; }
public record ReceiptInput(Guid RequestId, decimal Amount);
public record VisitInput(Guid CustomerId, string Note);
public record DashboardDto(int Customers, int Orders, decimal Sales, decimal OpenBalance, int ExpiringLots);
