# ADR 001 — Monólito modular com aplicativo offline

- **Data:** 21/10/2026 (proposta) · 26/10/2026 (revisão após implementação)
- **Status:** aceita e implementada

## Contexto

Os vendedores registram pedidos em campo, sem garantia de conexão nem de estoque. A central precisa cortar e confirmar vendas com consistência de estoque e financeiro. A equipe trabalha com C# e quer uma operação simples de manter no início.

## Decisão

- **Monólito modular** em .NET 10, separado em Domain, Contracts, Application, Infrastructure e Api.
- **API ASP.NET Core** como único ponto de acesso ao banco, concentrando regras e autorização.
- **Painel Blazor Interactive Server** para as operações centrais, consumindo a API.
- **App .NET MAUI** (Android e Windows) com SQLite local e fila de pedidos para coleta offline.
- **Idempotência por identificador gerado no cliente** (`ClientRequestId` e `RequestId`) para permitir reenvio seguro.
- **SQLite** para demonstração e **PostgreSQL** como destino de produção, trocando por configuração.
- Operações críticas (venda e recebimento) em **transação Serializable** com tokens de concorrência.

## Alternativas consideradas

| Alternativa | Por que não |
| --- | --- |
| Microsserviços | Custo de operação e consistência distribuída sem necessidade demonstrada |
| App apenas online | Não atende a coleta com conectividade limitada |
| PWA offline | Possível; exigiria validar persistência e recursos do aparelho antes de substituir o MAUI |
| Painel acessando o banco direto | Duplicaria regras e contornaria a autorização da API |

## Consequências

- Poucos componentes para operar e transações locais para estoque e financeiro.
- Os DTOs em Contracts são compartilhados pela API, pelo painel e pelo app, e qualquer mudança de contrato afeta os três.
- A sessão do painel vive no circuito do servidor; recarregar a página exige novo login.
- O app MAUI fica fora da solução principal para não exigir workloads no CI.
- Falta adotar migrations antes de uso com dados reais (ver [08 — Roadmap](../08-roadmap.md)).

## Revisão

Reavaliar se surgirem iOS, operação offline no navegador, múltiplas empresas ou integrações com volume alto. Uma mudança relevante cria um novo ADR que referencia este.
