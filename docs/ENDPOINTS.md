# Endpoints

URL local: http://localhost:5207. Exceto health e login, envie `Authorization: Bearer TOKEN`. IDs são GUIDs. Erros de negócio retornam 400 com campo error; falta de login, 401; perfil sem acesso, 403; conflitos de persistência, 409.

| Método | Rota | Acesso |
|---|---|---|
| GET | /health | Público |
| POST | /auth/login | Público com limitação de tentativas |
| POST | /api/logout | Autenticado |
| GET | /api/sellers | Administrador ou vendedor |
| POST | /api/users | Administrador |
| GET | /api/customers | Autenticado; vendedor vê a carteira própria |
| POST | /api/customers | Administrador ou vendedor |
| PUT | /api/customers/{id}/block?blocked=true | Administrador |
| GET | /api/products | Autenticado |
| POST | /api/products | Administrador |
| GET / POST | /api/lots | Administrador ou estoque |
| GET | /api/terms | Autenticado |
| POST | /api/terms | Administrador |
| GET | /api/orders | Autenticado; vendedor vê pedidos próprios |
| POST | /api/orders | Administrador ou vendedor |
| PUT | /api/orders/{id}/cut | Administrador ou estoque |
| POST | /api/orders/{id}/sell | Administrador ou estoque |
| POST | /api/orders/{id}/cancel | Administrador |
| GET | /api/receivables | Administrador, financeiro ou vendedor da carteira |
| POST | /api/receivables/{id}/receive | Administrador ou financeiro |
| GET / POST | /api/visits | Administrador ou vendedor |
| GET | /api/dashboard | Administrador |
| GET | /api/audit | Administrador |

Modelos completos estão em src/Panis360.Contracts/Models.cs. O servidor define preço, vendedor e total; não aceita preço enviado pelo cliente. Vencimentos usam a data da venda.
