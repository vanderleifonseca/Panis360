# 05 — Modelo conceitual de dados

Proposta de entidades e vínculos; ainda não é um esquema SQL nem uma migration aprovada. IDs de entidades de domínio: UUID. Número comercial sequencial será separado do identificador técnico. Todas as entidades mutáveis relevantes terão controle de versão.

| Entidade | Campos principais e vínculos |
| --- | --- |
| Usuario | Id, identidade de autenticação, ativo; funções e permissões |
| Vendedor | Id, UsuarioId, nome, ativo |
| Territorio | Id, nome, ativo; municípios/regiões conforme D04 |
| AtribuicaoTerritorial | Id, TerritorioId, VendedorId, início, fim; sem sobreposição de exclusividade |
| Cliente | Id, tipo, documento, razão/nome, ativo, bloqueio e motivo |
| EnderecoCliente | Id, ClienteId, finalidade, logradouro, número, complemento, bairro, cidade, UF, CEP |
| CarteiraCliente | Id, ClienteId, VendedorId, TerritorioId, vigência |
| RoteiroVisita | Id, VendedorId, dia da semana, ativo |
| ItemRoteiro | Id, RoteiroId, ClienteId, posição |
| Visita | Id, ClienteId, VendedorId, instante, resultado, observação, PedidoId opcional |
| Produto | Id, código, descrição, categoria, unidade-base, ativo |
| Embalagem | Id, ProdutoId, unidade comercial, fator de conversão, múltiplo mínimo |
| TabelaPreco/ItemPreco | Id, vigência, ProdutoId/EmbalagemId, valor |
| Promocao | Id, produto/lote elegível, preço/regra, início, fim, aprovação |
| Deposito | Id, nome, ativo |
| Lote | Id, ProdutoId, código, validade, bloqueio, origem |
| MovimentoEstoque | Id, DepositoId, LoteId, tipo, quantidade-base, origem, instante, estornoDeId |
| ReservaEstoque | Id, ItemPedidoId, DepositoId, LoteId, quantidade-base, estado, RodadaCorteId |
| JanelaProcessamento | Id, data, horário, estado, fechamento |
| Pedido | Id, número, ClienteId, VendedorId, JanelaId, estado, coleta, recebimento, versão comercial |
| ItemPedido | Id, PedidoId, produto/embalagem, preço copiado, quantidades solicitada e aprovada |
| RodadaCorte/ItemCorte | Id, JanelaId, regra, operador, motivo, solicitado, aprovado e déficit |
| Separacao/ItemSeparacao | Id, PedidoId, lote, quantidade conferida, estado |
| Carga/ItemCarga | Id, veículo/motorista ou referências, itens separados, saída |
| Venda/ItemVenda | Id, PedidoId, itens faturados, valores e condições copiadas |
| DocumentoFiscal | Id, VendaId, referência externa, chave quando aplicável, estado e ajustes vinculados |
| Entrega/ItemEntrega | Id, CargaId, VendaId, instante, quantidade aceita/recusada e motivo |
| Devolucao | Id, origem, lote, quantidade, inspeção, destinação |
| CondicaoPagamento/RegraParcela | Id, descrição, prazo, percentual, regras versionadas |
| FormaPagamento | Id, descrição, tipo/instrumento, ativo |
| TituloReceber | Id, VendaId, sequência, vencimento, valor original, forma, estado |
| AjusteFinanceiro | Id, TituloId, valor, motivo, ocorrência/documento relacionado |
| Recebimento/AlocacaoRecebimento | Id, referência, valor, instante; vínculo e valor destinado a cada título |
| EventoAuditoria | Id, ator, ação, entidade, instante, motivo, correlação e diferenças permitidas |
| RegistroIdempotencia | Escopo, chave, hash do conteúdo, resultado, PedidoId |
| EventoOutbox | Id, tipo, conteúdo mínimo, tentativas, próxima execução, estado |

## Cardinalidades centrais

- Cliente 1:N endereços, pedidos e vínculos históricos de carteira.
- Pedido 1:N itens; item 1:N reservas e registros de separação.
- Produto 1:N embalagens e lotes; lote pode existir em vários depósitos por movimentos.
- Venda 1:N itens, documentos, entregas e títulos. MVP propõe uma venda original por pedido; documentos de ajuste não constituem nova venda duplicada.
- Título N:M recebimentos através de alocações, permitindo pagamentos parciais e um recebimento para vários títulos.

## Integridade

1. Restrição única de pedido por UUID e idempotência por escopo/chave.
2. Exclusividade territorial exige restrição transacional sobre períodos sobrepostos, não apenas índice sobre ativo.
3. Quantidades não negativas; aprovadas não excedem solicitadas. Demais etapas não excedem sua origem autorizada.
4. Conversões positivas e versionadas; guardar fator usado na operação.
5. Valores em decimal, nunca float/double. Precisão e escala finais dependem de D08.
6. Documento do cliente normalizado e único no contexto da empresa; validação final de PF/PJ depende de D10.
7. Sequência de parcela única por venda; alocação de recebimento não excede valor disponível nem saldo autorizado.
8. Restrições de saldo e reserva são garantidas na transação; somas entre linhas exigem estratégia além de CHECK simples.
9. Saldo de estoque derivado do livro de movimentos; eventual projeção de saldo deve ser reconciliável.
10. Saldo financeiro = original + débitos − créditos − recebimentos líquidos de estornos. Encargos dependem de regra aprovada.

## Índices a avaliar

Pedidos por janela/estado; títulos por cliente/vencimento/estado; lotes por produto/validade; movimentos por depósito/lote; carteira por vendedor/vigência; outbox por estado/próxima execução. Validar com consultas e volumes reais antes de otimizar.
