# 08 — Roadmap e limitações

## Limitações da versão atual

| Área | Limitação |
| --- | --- |
| Fiscal | Não emite NF-e nem registra documento fiscal externo |
| Cobrança | Boleto e PIX são apenas rótulos; não há emissão nem conciliação bancária |
| Pedido | Sem reserva de estoque; sem edição de itens depois de enviado; sem rascunho no servidor |
| Corte | Manual, pedido a pedido; sem corte proporcional automático entre pedidos |
| Venda | Imutável: sem estorno nem devolução; atende sempre a quantidade atendida inteira |
| Logística | Sem separação, carga, expedição ou registro de entrega |
| Financeiro | Sem juros, multa, desconto, renegociação ou estorno de recebimento |
| Vencimentos | Dias corridos, sem ajuste de fim de semana ou feriado |
| Cadastros | Sem edição completa de clientes e produtos nem gestão visual de usuários (desativar, trocar senha) |
| Clientes | CPF/CNPJ sem validação dos dígitos verificadores |
| Roteiro | Apenas a sequência de visita; sem mapa ou otimização de rota |
| App | Sincronização manual; o carrinho se perde se o app fechar antes de salvar na fila; a fila não permite editar nem descartar pedidos; cadastro de cliente e visita exigem conexão |
| Relatórios | Apenas os indicadores do início; sem exportação |
| Janelas | Sem transmissão agendada (12h e 19h) nem fechamento de janela |
| Segurança | Sem recuperação de senha nem revogação administrativa de sessões; base local do app sem criptografia adicional |
| Banco | Sem migrations do EF Core; `EnsureCreated` mais `SchemaUpgrade` apenas em Development |

## Próximas etapas sugeridas

A ordem segue as dependências entre as etapas, não um calendário.

1. **Base para produção:** migrations do EF Core, PostgreSQL validado com concorrência, backup e restauração, HTTPS, gestão de segredos, logs e observabilidade.
2. **Gestão de usuários:** desativar usuário, trocar e recuperar senha, revogar sessões.
3. **Cadastros completos:** edição de clientes e produtos, validação de CPF/CNPJ, inativação no lugar de exclusão.
4. **App em campo:** validação em aparelhos, fila editável, carrinho persistido, sincronização automática com novas tentativas.
5. **Estoque e corte:** reserva na aprovação, corte proporcional entre pedidos, bloqueio de lote, validade mínima por cliente.
6. **Expedição e entrega:** separação, carga, entrega parcial, recusa, devolução e quarentena.
7. **Financeiro:** estorno de recebimento, ajustes vinculados à entrega, encargos aprovados.
8. **Relatórios:** vendas, cortes, validade, inadimplência e exportação.
9. **Integrações:** emissor fiscal, banco ou provedor de cobrança e PIX, com idempotência e reconciliação.

## Decisões de negócio pendentes

Antes das etapas correspondentes, estas perguntas precisam de resposta, com responsável e data:

| ID | Pergunta | Afeta |
| --- | --- | --- |
| D01 | Quantas empresas, filiais e depósitos existem? | Modelo de estoque e banco |
| D02 | Qual emissor fiscal é usado e como registra devolução e recusa? | Expedição e integração fiscal |
| D03 | Qual banco ou provedor de cobrança e quais modalidades? | Integração financeira |
| D04 | Praça é município, bairro ou região? Há exclusividade e substituição temporária de vendedor? | Carteiras |
| D05 | Quais critérios de corte, bloqueio, crédito e exceção? | Corte e bloqueio |
| D06 | O vencimento conta da venda, da saída ou da entrega? Ajusta fim de semana e feriado? | Parcelamento |
| D07 | Quais percentuais de juros, multa e desconto são válidos? | Financeiro |
| D08 | Há venda fracionada, embalagens diferentes e conversões? | Produtos e quantidades |
| D09 | Há validade mínima por cliente ou produto? | Seleção de lotes |
| D10 | Aceita cliente PF ou só PJ? | Cadastro de clientes |
| D11 | Quanto tempo o app pode operar offline? Como tratar aparelho perdido? | Segurança do app |
| D12 | Quantos vendedores, clientes, produtos e pedidos por dia? Que indisponibilidade é tolerável? | Infraestrutura |
| D13 | Existem frete, desconto por item, impostos no preço e renegociação? | Totais e financeiro |

Registre cada resposta numa issue e atualize os documentos afetados. Uma mudança estrutural gera um novo ADR em `docs/adr/`.
