# Panis360 — arquitetura implementada

Aplicação B2B para distribuidora de insumos de panificação, baseada nas três imagens de requisitos. MAUI e Blazor consomem API ASP.NET Core; somente o servidor acessa o banco central.

## Projetos
- Domain: entidades persistidas.
- Contracts: modelos de entrada e de resposta, sem dependência do Entity Framework.
- Application: regras de quantidades, dinheiro e parcelamento.
- Infrastructure: DbContext e implementação transacional das operações comerciais.
- Api: autenticação, autorização, endpoints e composição dos serviços.
- Web: Blazor Interactive Server, com HttpClient para a API e token por circuito.
- App: MAUI para Android e Windows, catálogo em SQLite, fila de pedidos e sessão em SecureStorage.
- Tests: verificações executáveis de regras e operações com SQLite temporário.

## Fluxo comercial
Pedido Recebido → corte opcional → confirmação de venda → baixa FEFO e títulos na mesma transação. Pedido Recebido também pode ser Cancelado pelo administrador. Venda confirmada é imutável nesta versão; estorno e devolução ainda não implementados.

Quantidade solicitada é preservada. Quantidade aceita é ajustável antes da venda; não existe reserva antecipada. Disponibilidade só é garantida na confirmação. Saldo de lotes vencidos não pode ser vendido. A venda atende integralmente a quantidade aceita: se não houver saldo, a confirmação falha e o operador precisa rever o corte.

Moeda arredondada por linha para 2 casas, AwayFromZero. Quantidades até 3 casas; UN exige inteiro. Parcelas geradas sobre o total confirmado; última parcela recebe ajuste de centavos. Prazos em dias corridos a partir da data da venda no fuso de São Paulo, sem ajuste automático de feriados. Juros, multa e desconto das imagens não foram presumidos nem cobrados.

Cada pedido tem ClientRequestId único e hash do conteúdo; reenvio idêntico devolve o mesmo pedido. Cada recebimento tem RequestId único. Estoque e títulos têm revisão de concorrência. Venda e recebimento usam isolamento Serializable. Conflitos exigem atualização e nova tentativa; operações nunca devem ser confirmadas pela interface antes do retorno da API.

## Perfis
- Administrador: todos os módulos.
- Vendedor: clientes próprios, inclusão de clientes, produtos, condições, pedidos próprios, visitas e títulos da carteira.
- Estoque: catálogo, pedidos, cortes, lotes e confirmação de venda.
- Financeiro: consulta de pedidos e títulos; recebimentos.

O aplicativo aceita vendedor e administrador. Outros perfis utilizam o painel. A API aplica as permissões independentemente dos botões da interface. Documentos de clientes são normalizados e precisam ter 11 ou 14 dígitos; validação de dígitos verificadores e consulta cadastral são evoluções.

## Identidade
Senhas com PasswordHasher do ASP.NET Core; tokens aleatórios de 256 bits com apenas hash no banco, validade de 12 horas e revogação ao sair online. Limitação por IP no login. Sem senha fixa no código; administrador criado apenas no banco vazio a partir de user-secrets ou variáveis de ambiente.

No painel, a sessão fica na memória do circuito. Recarregar completamente a página pode exigir novo login. No app, a sessão fica em SecureStorage; a base local contém dados comerciais sem criptografia adicional. Pendências são particionadas por usuário. Uma troca de usuário limpa o estado em memória, e a carteira armazenada é recuperada apenas para o mesmo usuário. Dispositivos precisam de proteção de acesso antes de uso real.

## Persistência
SQLite é a configuração inicial de demonstração para abrir o projeto sem instalar servidor. PostgreSQL é o destino proposto para operação central e já possui provedor configurável. Nenhuma mudança automática de provedor copia registros.

EnsureCreated é permitido somente em Development e apenas cria um banco vazio: não atualiza esquemas existentes. Antes de evoluir o modelo com dados reais, estabelecer migrations do EF Core e backup/restauração. Esquema PostgreSQL inicial disponível em database/postgresql-inicial.sql. Não executar esse arquivo em banco com tabelas já criadas pelo EnsureCreated.

## Limites desta entrega
Não inclui emissão fiscal, emissão bancária, conciliação automática, produção/receitas, entrega fracionada, reserva, estorno de venda, devoluções, edição completa dos cadastros, manutenção visual de usuários, relatórios exportáveis ou agendador 12h/19h. Os dados de histórico são os pedidos vendidos e suas quantidades/preços preservados. Roteiro é uma sequência por cliente e registro de visitas, sem mapa ou otimização geográfica.

No app, pedido é montado em memória e fica persistido quando o usuário toca Salvar pedido na fila local. Fechar o app antes desse comando perde o carrinho. Sincronização é manual; um pedido aceito pelo servidor após timeout pode ser reenviado com o mesmo identificador. Pedidos rejeitados permanecem na fila com motivo; edição e descarte visual da fila serão evolução. Novos clientes e visitas exigem conexão.

## Evolução para uso real
Antes de produção: validar PostgreSQL com concorrência, backups e restauração; publicar API e painel com HTTPS e gestão de segredos; validar MAUI em dispositivos; revisar dados pessoais e retenção; acrescentar recuperação de senha e revogação administrativa; implementar migrations, observabilidade e fluxos de estorno/devolução. Não expor a configuração HTTP de desenvolvimento à internet.
