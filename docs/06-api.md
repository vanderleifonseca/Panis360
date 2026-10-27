# 06 — API

URL local: `http://localhost:5207`. JSON em UTF-8, identificadores GUID. Exceto `/health` e `/auth/login`, todas as rotas exigem `Authorization: Bearer TOKEN`. Os modelos de requisição e resposta estão em `src/Panis360.Contracts/Models.cs`.

## Códigos de resposta

| Código | Quando |
| --- | --- |
| 200 / 204 | Sucesso (204 sem corpo) |
| 400 | Regra de negócio ou validação. Corpo: `{"error": "mensagem"}` |
| 401 | Sem token, token expirado ou usuário inativo |
| 403 | Perfil sem permissão |
| 409 | Conflito de concorrência ou registro duplicado |
| 429 | Limite de tentativas de login |
| 500 | Falha interna (detalhe apenas no log da API) |

## Endpoints

### Público

| Método | Rota | Descrição |
| --- | --- | --- |
| GET | `/health` | `{"status":"ok","system":"Panis360"}` |
| POST | `/auth/login` | `{email, password}` → `{token, userId, name, role, expiresAt}`. Até 10 por minuto por IP |

### Sessão e usuários

| Método | Rota | Perfis | Descrição |
| --- | --- | --- | --- |
| POST | `/api/logout` | Autenticado | Encerra a sessão atual |
| GET | `/api/sellers` | Administrador, Vendedor | Usuários ativos que podem ter carteira |
| POST | `/api/users` | Administrador | Cria usuário: `{name, email, password (12+), role}` |

### Clientes e visitas

| Método | Rota | Perfis | Descrição |
| --- | --- | --- | --- |
| GET | `/api/customers` | Autenticado | Lista por ordem de visita; o vendedor vê só a carteira dele |
| POST | `/api/customers` | Administrador, Vendedor | Cria cliente; o vendedor sempre cria na própria carteira |
| PUT | `/api/customers/{id}/block?blocked=true\|false` | Administrador | Bloqueia ou desbloqueia |
| GET | `/api/visits` | Administrador, Vendedor | Visitas (o vendedor vê as próprias) |
| POST | `/api/visits` | Administrador, Vendedor | `{customerId, note}`; o cliente precisa ser da carteira |

### Catálogo e estoque

| Método | Rota | Perfis | Descrição |
| --- | --- | --- | --- |
| GET | `/api/products` | Autenticado | Produtos ativos com `available` (estoque válido) |
| POST | `/api/products` | Administrador | `{name, group, unit: UN\|KG\|L, price, active}` |
| GET | `/api/lots` | Administrador, Estoque | Lotes por validade, com `revision` |
| POST | `/api/lots` | Administrador, Estoque | Entrada: `{productId, code, expiresOn, quantity}` |
| PUT | `/api/lots/{id}` | Administrador, Estoque | Ajuste: `{code, expiresOn, quantity, reason, revision}` |
| GET | `/api/lots/{id}/movements` | Administrador, Estoque | Histórico com saldo após cada movimentação e usuário |

### Condições de pagamento

| Método | Rota | Perfis | Descrição |
| --- | --- | --- | --- |
| GET | `/api/terms` | Autenticado | Condições com as parcelas `{days, percent}` |
| POST | `/api/terms` | Administrador | `{name, parts:[{days, percent}]}`; os percentuais somam 100 |

### Pedidos e venda

| Método | Rota | Perfis | Descrição |
| --- | --- | --- | --- |
| GET | `/api/orders` | Autenticado | Mais recentes primeiro; o vendedor vê só os próprios |
| POST | `/api/orders` | Administrador, Vendedor | Cria o pedido (idempotente por `clientRequestId`); devolve o id |
| PUT | `/api/orders/{id}/cut` | Administrador, Estoque | `{reason, items:[{itemId, accepted}]}` |
| POST | `/api/orders/{id}/sell` | Administrador, Estoque | Confirma a venda; devolve o id da venda (idempotente) |
| POST | `/api/orders/{id}/cancel` | Administrador | Cancela um pedido Recebido |

### Financeiro

| Método | Rota | Perfis | Descrição |
| --- | --- | --- | --- |
| GET | `/api/receivables` | Administrador, Financeiro, Vendedor | Parcelas por vencimento; o vendedor vê as da carteira |
| POST | `/api/receivables/{id}/receive` | Administrador, Financeiro | `{requestId, amount}` (idempotente por `requestId`) |

### Administração

| Método | Rota | Perfis | Descrição |
| --- | --- | --- | --- |
| GET | `/api/dashboard` | Administrador | `{customers, orders, sales, openBalance, expiringLots}` |
| GET | `/api/audit` | Administrador | Últimos 200 eventos de auditoria |

## Exemplo: login e pedido

```http
POST /auth/login
Content-Type: application/json

{ "email": "vendedor@exemplo.local", "password": "senha-com-12-caracteres" }
```

```http
POST /api/orders
Authorization: Bearer 3F9A...
Content-Type: application/json

{
  "clientRequestId": "11111111-1111-4111-8111-111111111111",
  "customerId": "22222222-2222-4222-8222-222222222222",
  "paymentTermId": "33333333-3333-4333-8333-333333333333",
  "paymentMethod": "Boleto",
  "items": [
    { "productId": "44444444-4444-4444-8444-444444444444", "quantity": 12 }
  ]
}
```

Os identificadores acima são fictícios. Não envie preço: o servidor usa o preço atual do produto. Repetir a mesma requisição devolve o mesmo id de pedido.

## Observações

- O servidor define preço, vendedor e total; dados de identidade enviados no corpo não são usados para autorização.
- Os vencimentos partem da data da confirmação da venda, não da data do pedido.
- Não há versionamento de rota (`/api/v1`), paginação nem OpenAPI nesta versão.
