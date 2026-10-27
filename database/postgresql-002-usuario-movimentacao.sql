-- Usuário responsável por cada movimentação de estoque.
-- Para bancos PostgreSQL criados com postgresql-inicial.sql antes desta coluna. Pode ser executado mais de uma vez.
ALTER TABLE "Movements" ADD COLUMN IF NOT EXISTS "ActorId" uuid;

-- Movimentações anteriores: usuário recuperado da auditoria.
UPDATE "Movements" SET "ActorId" = (SELECT a."ActorId" FROM "Audits" a WHERE a."Action" = 'Lote recebido' AND a."TargetId" = "Movements"."StockLotId" LIMIT 1)
WHERE "Reason" = 'Entrada inicial' AND "ActorId" IS NULL;
UPDATE "Movements" SET "ActorId" = (SELECT a."ActorId" FROM "Audits" a WHERE a."Action" = 'Venda confirmada' AND a."TargetId" = "Movements"."SaleId" LIMIT 1)
WHERE "SaleId" IS NOT NULL AND "ActorId" IS NULL;
UPDATE "Movements" SET "ActorId" = (SELECT a."ActorId" FROM "Audits" a WHERE a."Action" = 'Lote ajustado' AND a."TargetId" = "Movements"."StockLotId"
 AND abs(extract(epoch from a."Date" - "Movements"."Date")) < 10 LIMIT 1)
WHERE "Reason" LIKE 'Ajuste:%' AND "ActorId" IS NULL;
