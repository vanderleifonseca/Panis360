using Microsoft.EntityFrameworkCore;
namespace Panis360.Infrastructure;
// EnsureCreated não altera bancos existentes; aqui ficam as alterações incrementais, idempotentes, aplicadas na inicialização.
public static class SchemaUpgrade {
 public static async Task Apply(AppDb db) {
  var sqlite=db.Database.ProviderName?.Contains("Sqlite")==true;
  var exists=sqlite
   ?await db.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS \"Value\" FROM pragma_table_info('Movements') WHERE name='ActorId'").SingleAsync()
   :await db.Database.SqlQueryRaw<int>("SELECT COUNT(*)::int AS \"Value\" FROM information_schema.columns WHERE table_name='Movements' AND column_name='ActorId'").SingleAsync();
  if(exists>0) return;
  await using var tx=await db.Database.BeginTransactionAsync();
  await db.Database.ExecuteSqlRawAsync(sqlite?"ALTER TABLE \"Movements\" ADD COLUMN \"ActorId\" TEXT NULL":"ALTER TABLE \"Movements\" ADD COLUMN \"ActorId\" uuid NULL");
  // Movimentações anteriores à coluna: usuário recuperado da auditoria (o ajuste é auditado na mesma gravação, segundos de diferença).
  await db.Database.ExecuteSqlRawAsync("UPDATE \"Movements\" SET \"ActorId\"=(SELECT a.\"ActorId\" FROM \"Audits\" a WHERE a.\"Action\"='Lote recebido' AND a.\"TargetId\"=\"Movements\".\"StockLotId\" LIMIT 1) WHERE \"Reason\"='Entrada inicial'");
  await db.Database.ExecuteSqlRawAsync("UPDATE \"Movements\" SET \"ActorId\"=(SELECT a.\"ActorId\" FROM \"Audits\" a WHERE a.\"Action\"='Venda confirmada' AND a.\"TargetId\"=\"Movements\".\"SaleId\" LIMIT 1) WHERE \"SaleId\" IS NOT NULL");
  await db.Database.ExecuteSqlRawAsync(sqlite
   ?"UPDATE \"Movements\" SET \"ActorId\"=(SELECT a.\"ActorId\" FROM \"Audits\" a WHERE a.\"Action\"='Lote ajustado' AND a.\"TargetId\"=\"Movements\".\"StockLotId\" AND abs(julianday(a.\"Date\")-julianday(\"Movements\".\"Date\"))*86400<10 LIMIT 1) WHERE \"Reason\" LIKE 'Ajuste:%'"
   :"UPDATE \"Movements\" SET \"ActorId\"=(SELECT a.\"ActorId\" FROM \"Audits\" a WHERE a.\"Action\"='Lote ajustado' AND a.\"TargetId\"=\"Movements\".\"StockLotId\" AND abs(extract(epoch from a.\"Date\"-\"Movements\".\"Date\"))<10 LIMIT 1) WHERE \"Reason\" LIKE 'Ajuste:%'");
  await tx.CommitAsync();
 }
}
