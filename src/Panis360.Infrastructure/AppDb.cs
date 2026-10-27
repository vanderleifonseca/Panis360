using Microsoft.EntityFrameworkCore;
using Panis360.Domain;
namespace Panis360.Infrastructure;
public class AppDb(DbContextOptions<AppDb> options) : DbContext(options) {
 public DbSet<User> Users=>Set<User>(); public DbSet<Session> Sessions=>Set<Session>();
 public DbSet<Customer> Customers=>Set<Customer>(); public DbSet<Product> Products=>Set<Product>();
 public DbSet<StockLot> Lots=>Set<StockLot>(); public DbSet<PaymentTerm> Terms=>Set<PaymentTerm>();
 public DbSet<Order> Orders=>Set<Order>(); public DbSet<Sale> Sales=>Set<Sale>();
 public DbSet<Receivable> Receivables=>Set<Receivable>(); public DbSet<Receipt> Receipts=>Set<Receipt>();
 public DbSet<StockMovement> Movements=>Set<StockMovement>(); public DbSet<Visit> Visits=>Set<Visit>(); public DbSet<Audit> Audits=>Set<Audit>();
 protected override void OnModelCreating(ModelBuilder b) {
  b.Entity<User>().HasIndex(x=>x.Email).IsUnique();
  b.Entity<Session>().HasIndex(x=>x.TokenHash).IsUnique();
  b.Entity<Session>().HasOne<User>().WithMany().HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Customer>().HasIndex(x=>x.Document).IsUnique();
  b.Entity<Customer>().HasOne<User>().WithMany().HasForeignKey(x=>x.SellerId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<StockLot>().HasIndex(x=>new{x.ProductId,x.Code}).IsUnique();
  b.Entity<StockLot>().HasOne<Product>().WithMany().HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<StockLot>().Property(x=>x.Revision).IsConcurrencyToken();
  b.Entity<PaymentTerm>().HasMany(x=>x.Parts).WithOne().HasForeignKey(x=>x.PaymentTermId);
  b.Entity<Order>().HasIndex(x=>x.ClientRequestId).IsUnique();
  b.Entity<Order>().Property(x=>x.Revision).IsConcurrencyToken();
  b.Entity<Order>().HasMany(x=>x.Items).WithOne().HasForeignKey(x=>x.OrderId);
  b.Entity<Order>().HasOne<Customer>().WithMany().HasForeignKey(x=>x.CustomerId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Order>().HasOne<User>().WithMany().HasForeignKey(x=>x.SellerId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Order>().HasOne<PaymentTerm>().WithMany().HasForeignKey(x=>x.PaymentTermId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<OrderItem>().HasOne<Product>().WithMany().HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Sale>().HasIndex(x=>x.OrderId).IsUnique();
  b.Entity<Sale>().HasOne<Order>().WithMany().HasForeignKey(x=>x.OrderId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Receivable>().Property(x=>x.Revision).IsConcurrencyToken();
  b.Entity<Receivable>().HasOne<Sale>().WithMany().HasForeignKey(x=>x.SaleId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Receipt>().HasIndex(x=>x.RequestId).IsUnique();
  b.Entity<Receipt>().HasOne<Receivable>().WithMany().HasForeignKey(x=>x.ReceivableId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<StockMovement>().HasOne<StockLot>().WithMany().HasForeignKey(x=>x.StockLotId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Visit>().HasOne<Customer>().WithMany().HasForeignKey(x=>x.CustomerId).OnDelete(DeleteBehavior.Restrict);
  foreach(var entity in b.Model.GetEntityTypes())
   foreach(var p in entity.GetProperties().Where(x=>x.ClrType==typeof(decimal))) { p.SetPrecision(18); p.SetScale(3); }
 }
}
