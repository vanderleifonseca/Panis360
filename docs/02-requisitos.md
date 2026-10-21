# 02 — Requisitos e critérios de aceite

Origem: **I** = informado pelo usuário; **P** = proposta técnica a validar. Prioridade P0 = MVP; P1 = evolução.

| ID | Origem | Prioridade | Requisito e aceite |
| --- | --- | --- | --- |
| RF01 | P | P0 | Autenticar usuários e autorizar por função e carteira. Vendedor A não consegue consultar cliente de B alterando o identificador na API. |
| RF02 | I | P0 | Garantir exclusividade territorial. Duas atribuições ativas sobre a mesma praça e período são recusadas, inclusive em gravações simultâneas. |
| RF03 | I | P0 | Manter roteiro por dia e posição. Inserir posição 25 entre 20 e 30 conserva a ordem. |
| RF04 | I | P0 | Manter catálogo com preços e promoções. Promoção fora da vigência deixa de ser aplicada; versão usada no pedido é preservada. |
| RF05 | I | P0 | Coletar pedidos sem limitar pela disponibilidade central. Solicitar 100 com saldo 70 é permitido, exibindo sujeito à confirmação. |
| RF06 | P | P0 | Gravar rascunho e pedido localmente sem internet. Reiniciar o aparelho conserva os dados gravados. |
| RF07 | I/P | P0 | Transmitir por janelas de 12h e 19h, com envio manual e novas tentativas. Reenviar o mesmo identificador não cria novo pedido. |
| RF08 | I | P0 | Consolidar demanda e realizar corte. Solicitado 100, disponível 70: soma aprovada não ultrapassa 70; motivo e operador são registrados. |
| RF09 | P | P0 | Reservar estoque atomicamente. Duas aprovações concorrentes não geram saldo disponível negativo. |
| RF10 | I/P | P0 | Controlar perecíveis por lote, validade e bloqueio. Lote vencido não pode ser reservado ou expedido. |
| RF11 | I/P | P0 | Separar, conferir e montar carga. Quantidades separadas não excedem as aprovadas; divergências voltam para revisão. |
| RF12 | I/P | P0 | Registrar documento fiscal externo antes da saída. Referência repetida não cria outra venda nem títulos duplicados. |
| RF13 | I | P0 | Registrar entrega parcial, recusa e devolução. Pedido 10, faturado 8, entregue 6 conserva as três quantidades e abre ajuste de 2. |
| RF14 | I | P0 | Separar condição e forma de pagamento. Venda de R$ 1.000 em 7/14 dias gera duas parcelas de R$ 500 e instrumento próprio por parcela. |
| RF15 | I/P | P0 | Consultar pendências e registrar recebimentos. Baixa parcial reduz saldo sem apagar título; estorno conserva histórico. |
| RF16 | P | P0 | Auditar alterações relevantes. Corte, preço autorizado, baixa e troca de carteira incluem ator, instante, motivo e correlação. |
| RF17 | P | P0 | Exibir relatórios de vendas, cortes, validade e pendências. Totais reconciliam com os documentos de origem e filtros informados. |
| RF18 | P | P1 | Integrar emissão fiscal e bancos. Retorno repetido do provedor não duplica documentos ou pagamentos. |

## Requisitos não funcionais

| ID | Compromisso proposto | Evidência |
| --- | --- | --- |
| RNF01 | Operação offline para coleta; autenticação inicial exige conexão | Teste com modo avião após carga inicial |
| RNF02 | HTTPS, segredos fora do Git e menor privilégio | Revisão de configuração e autorização |
| RNF03 | Rastreabilidade e recuperação | Teste de restauração e histórico de ajustes |
| RNF04 | Integridade monetária e concorrência | Testes de arredondamento, corte e reserva |
| RNF05 | Usabilidade em campo | Piloto com vendedor real e feedback registrado |
| RNF06 | Desempenho dimensionado pelo volume real | Definir usuários, itens e pedidos em D12; executar carga representativa antes da expansão |

## Condição de aceite global

Executar o roteiro de ponta a ponta de E7: coleta offline → envio → corte → reserva → separação → referência fiscal → saída → entrega parcial → ajuste → recebimento. Nenhum teste pode depender de dados pessoais reais no repositório.
