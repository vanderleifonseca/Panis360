namespace Panis360.Domain;
public abstract class Entity { public Guid Id { get; set; } = Guid.NewGuid(); }
public class User : Entity {
 public string Name { get; set; } = "";
 public string Email { get; set; } = "";
 public string PasswordHash { get; set; } = "";
 public string Role { get; set; } = "Vendedor";
 public bool Active { get; set; } = true;
}
public class Session : Entity {
 public string TokenHash { get; set; } = "";
 public Guid UserId { get; set; }
 public DateTime ExpiresAt { get; set; }
}
public class Customer : Entity {
 public string Name { get; set; } = "";
 public string Document { get; set; } = "";
 public string Phone { get; set; } = "";
 public string Street { get; set; } = "";
 public string Number { get; set; } = "";
 public string Neighborhood { get; set; } = "";
 public string City { get; set; } = "";
 public string State { get; set; } = "PR";
 public string ZipCode { get; set; } = "";
 public Guid SellerId { get; set; }
 public int VisitOrder { get; set; }
 public bool Blocked { get; set; }
}
public class Product : Entity {
 public string Name { get; set; } = "";
 public string Group { get; set; } = "";
 public string Unit { get; set; } = "UN";
 public decimal Price { get; set; }
 public bool Active { get; set; } = true;
}
public class StockLot : Entity {
 public Guid ProductId { get; set; }
 public string Code { get; set; } = "";
 public DateOnly ExpiresOn { get; set; }
 public decimal Quantity { get; set; }
 public int Revision { get; set; }
}
public class PaymentTerm : Entity {
 public string Name { get; set; } = "";
 public List<PaymentTermPart> Parts { get; set; } = [];
}
public class PaymentTermPart : Entity {
 public Guid PaymentTermId { get; set; }
 public int Days { get; set; }
 public decimal Percent { get; set; }
}
public class Order : Entity {
 public Guid ClientRequestId { get; set; }
 public string RequestHash { get; set; } = "";
 public Guid CustomerId { get; set; }
 public Guid SellerId { get; set; }
 public Guid PaymentTermId { get; set; }
 public string PaymentMethod { get; set; } = "Boleto";
 public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
 public string Status { get; set; } = "Recebido";
 public int Revision { get; set; }
 public string CutReason { get; set; } = "";
 public List<OrderItem> Items { get; set; } = [];
}
public class OrderItem : Entity {
 public Guid OrderId { get; set; }
 public Guid ProductId { get; set; }
 public string ProductName { get; set; } = "";
 public decimal Requested { get; set; }
 public decimal Accepted { get; set; }
 public decimal UnitPrice { get; set; }
}
public class Sale : Entity {
 public Guid OrderId { get; set; }
 public Guid CustomerId { get; set; }
 public DateOnly Date { get; set; }
 public decimal Total { get; set; }
}
public class Receivable : Entity {
 public Guid SaleId { get; set; }
 public Guid CustomerId { get; set; }
 public int Number { get; set; }
 public DateOnly DueDate { get; set; }
 public decimal Amount { get; set; }
 public decimal Paid { get; set; }
 public int Revision { get; set; }
 public string PaymentMethod { get; set; } = "";
}
public class Receipt : Entity {
 public Guid ReceivableId { get; set; }
 public Guid RequestId { get; set; }
 public decimal Amount { get; set; }
 public DateTime Date { get; set; } = DateTime.UtcNow;
}
public class StockMovement : Entity {
 public Guid StockLotId { get; set; }
 public Guid? SaleId { get; set; }
 public Guid? ActorId { get; set; }
 public decimal Quantity { get; set; }
 public DateTime Date { get; set; } = DateTime.UtcNow;
 public string Reason { get; set; } = "";
}
public class Visit : Entity {
 public Guid CustomerId { get; set; }
 public Guid SellerId { get; set; }
 public DateTime Date { get; set; } = DateTime.UtcNow;
 public string Note { get; set; } = "";
}
public class Audit : Entity {
 public Guid ActorId { get; set; }
 public DateTime Date { get; set; } = DateTime.UtcNow;
 public string Action { get; set; } = "";
 public Guid TargetId { get; set; }
}
