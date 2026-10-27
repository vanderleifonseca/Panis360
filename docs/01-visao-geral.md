# 01 — Visão geral

## Contexto

O Panis360 atende uma distribuidora B2B de fermento, misturas e outros insumos perecíveis para padarias, MEIs e empresas de alimentação. A operação tem vendedores externos com carteira própria de clientes, processamento central dos pedidos, controle de estoque por lote e validade e venda a prazo com parcelas curtas.

Problemas que o sistema resolve:

- **Pedido sem internet:** o vendedor registra o pedido no aplicativo mesmo sem sinal e envia depois, sem risco de duplicar.
- **Demanda maior que o estoque:** a central ajusta o atendimento (corte) com motivo registrado, sem perder o que o cliente pediu originalmente.
- **Perda por validade:** a venda consome primeiro os lotes que vencem antes (FEFO) e nunca usa lote vencido.
- **Cobrança:** a venda gera automaticamente as parcelas conforme a condição de pagamento, e o financeiro registra recebimentos parciais.
- **Rastreabilidade:** cadastros, cortes, vendas, ajustes de estoque e recebimentos ficam na trilha de auditoria com o usuário responsável.

## Perfis de acesso

O sistema tem quatro perfis. A API aplica as permissões em todas as chamadas, independentemente do que a tela mostra.

| Perfil | Pode fazer | Não pode |
| --- | --- | --- |
| **Administrador** | Tudo: usuários, produtos, condições de pagamento, bloqueio de clientes, cancelamento de pedidos, indicadores e auditoria | — |
| **Vendedor** | Ver e cadastrar clientes **da própria carteira**, registrar visitas, criar pedidos, consultar produtos, condições, os próprios pedidos e os títulos da carteira | Cortar, vender, mexer no estoque, receber títulos, ver clientes de outro vendedor |
| **Estoque** | Consultar catálogo e pedidos, ajustar corte, confirmar venda, registrar e ajustar lotes, ver o histórico de movimentações | Criar pedidos, ver títulos, receber |
| **Financeiro** | Consultar pedidos e todos os títulos, registrar recebimentos | Criar pedidos, cortar, vender, mexer no estoque |

O **aplicativo** aceita apenas Vendedor e Administrador. Os demais perfis usam o **painel web**.

## Funcionalidades implementadas

### Painel web (Blazor)

| Tela | Rota | Perfis | O que faz |
| --- | --- | --- | --- |
| Início | `/` | Todos (indicadores: Administrador) | Clientes, pedidos, vendas acumuladas, saldo a receber e lotes vencendo em até 7 dias |
| Clientes e roteiro | `/clientes` | Administrador, Vendedor | Cadastro, sequência de visita, registro de visita, bloqueio (Administrador) |
| Produtos | `/produtos` | Todos (cadastro: Administrador) | Catálogo com estoque válido |
| Pedidos e cortes | `/pedidos` | Todos | Novo pedido (Administrador, Vendedor), corte e venda (Administrador, Estoque), cancelamento (Administrador) |
| Estoque por lote | `/estoque` | Administrador, Estoque | Entrada de lote, ajuste com motivo, histórico de movimentações |
| Contas a receber | `/financeiro` | Administrador, Financeiro, Vendedor | Parcelas e saldos; recebimento (Administrador, Financeiro) |
| Configurações | `/configuracoes` | Administrador | Cadastro de usuários e de condições de pagamento |

As telas buscam os dados ao clicar em **Carregar / atualizar**.

### Aplicativo de vendas (MAUI)

- Login com o endereço da API configurável.
- Carteira de clientes em ordem de visita, com saldo em aberto, vencimentos e itens da última compra.
- Catálogo e condições de pagamento em cache local (SQLite).
- Montagem do pedido e gravação na **fila local**; funciona sem conexão.
- Sincronização manual dos pedidos pendentes, com o motivo de cada recusa.
- Cadastro de cliente e registro de visita (exigem conexão).

### Formas e condições de pagamento

- **Formas:** Boleto, PIX, Dinheiro e Cartão. São apenas registros; não há integração bancária.
- **Condições criadas no primeiro uso:** À vista, 7/14 dias (50% + 50%) e 7/14/21 dias (33,33% + 33,33% + 33,34%). O administrador pode cadastrar outras, com até 24 parcelas.

## Fora do escopo atual

Emissão fiscal, boleto e PIX integrados, expedição e entrega, devoluções, estorno de venda, reserva de estoque, corte automático, mapa de rotas, relatórios exportáveis e transmissão agendada. A lista completa e as decisões pendentes estão em [08 — Roadmap e limitações](08-roadmap.md).
