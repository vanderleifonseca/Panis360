# 06 — Contratos iniciais da API

Proposta para implementação. Nenhum endpoint está disponível ainda. Prefixo `/api/v1`; JSON em UTF-8; UUID para IDs; instantes ISO 8601 em UTC; datas de vencimento sem conversão de fuso; valores monetários decimais. OpenAPI será gerado após E1.

| Operação | Endpoint proposto | Autorização |
| --- | --- | --- |
| Autenticar/renovar | Contrato de identidade a definir em E1 | Identidade |
| Consultar carteira | GET /clientes | Vendedor filtrado ou comercial |
| Consultar catálogo | GET /catalogo | Usuário autorizado |
| Consultar roteiro | GET /roteiros | Vendedor filtrado ou comercial |
| Registrar visita | POST /visitas | Vendedor da carteira |
| Receber pedido | POST /pedidos | Vendedor da carteira |
| Consultar pedido | GET /pedidos/{id} | Carteira ou central |
| Analisar janela | GET /janelas/{id}/demanda | Central |
| Simular corte | POST /janelas/{id}/cortes/simulacoes | Supervisor |
| Confirmar corte | POST /janelas/{id}/cortes | Supervisor |
| Conferir separação | POST /separacoes/{id}/conferencias | Estoque |
| Registrar documento externo | POST /vendas/{id}/documentos-fiscais | Faturamento autorizado |
| Confirmar saída | POST /cargas/{id}/expedicoes | Logística |
| Registrar entrega | POST /entregas | Logística |
| Consultar títulos | GET /titulos | Financeiro ou carteira autorizada |
| Registrar recebimento | POST /recebimentos | Financeiro |
| Registrar estorno | POST /recebimentos/{id}/estornos | Financeiro autorizado |
| Obter mudanças | GET /sincronizacao?cursor=... | Escopo do usuário |

## Exemplo ilustrativo de pedido

Os UUIDs abaixo são fictícios. Cliente/vendedor são revalidados pelo servidor; não confiar em identidade enviada no corpo.

```json
{
  "id": "11111111-1111-4111-8111-111111111111",
  "clienteId": "22222222-2222-4222-8222-222222222222",
  "coletadoEm": "2026-10-21T14:00:00Z",
  "catalogoVersao": "versao-exemplo",
  "condicaoPagamentoId": "33333333-3333-4333-8333-333333333333",
  "itens": [
    {
      "produtoId": "44444444-4444-4444-8444-444444444444",
      "embalagemId": "55555555-5555-4555-8555-555555555555",
      "quantidadeSolicitada": 10,
      "precoUnitarioColetado": 100.00
    }
  ]
}
```

Enviar `Idempotency-Key` estável por operação. Primeira criação: 201 e localização do recurso. Repetição equivalente: resposta persistida equivalente, sem nova gravação comercial. Conteúdo diferente sob a mesma chave: 409. Estoque insuficiente não rejeita a coleta; divergência de preço coloca o pedido em revisão.

## Concorrência e falhas

- ETag/If-Match ou versão equivalente para mutações sobre recursos existentes.
- Simulação de corte não reserva; confirmação revalida a versão do estoque e cria reservas atomicamente.
- 400: formato inválido; 401: não autenticado; 403: sem acesso; 404: recurso inexistente ou não visível; 409: conflito de versão/idempotência; 422: regra de negócio; 429: limite; 503: indisponibilidade transitória.
- Respostas de erro usam Problem Details, código estável e correlationId, sem stack trace ou segredos.
- Listas paginadas com limite máximo definido; cursor de sincronização é opaco. Resposta inclui `nextCursor`, `hasMore` e mudanças/inativações.
- Finalizar página local e cursor na mesma transação evita perder alterações após interrupção.
- Webhooks futuros exigirão verificação de autenticidade e idempotência; dados do provedor não são aceitos como autorização do usuário.
