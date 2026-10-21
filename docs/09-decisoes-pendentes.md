# 09 — Decisões pendentes

Status inicial de todas: **aberta**. Registrar resposta, responsável, data e documentos afetados antes de encerrar. Responsáveis abaixo são papéis sugeridos, não pessoas já designadas.

| ID | Pergunta | Responsável sugerido | Impacto e momento |
| --- | --- | --- | --- |
| D01 | Quantas empresas/filiais e depósitos existem? | Direção | Limites do MVP e banco; antes de E1 |
| D02 | Qual emissor fiscal existe e como registra devolução/recusa? | Contabilidade/faturamento | Processo externo de E5 e integração E8 |
| D03 | Qual banco/provedor de cobrança e modalidades utilizadas? | Financeiro | Integração E8; não presumir banco de outro projeto |
| D04 | Praça é município, bairro ou região? Há substituição temporária? | Comercial | Carteira e exclusividade; antes de E2 |
| D05 | Quais critérios de corte, bloqueio, crédito e exceção? | Direção/financeiro | Aprovação e prioridade; antes de E4 |
| D06 | Vencimento conta de emissão, saída ou entrega? Ajusta fim de semana? | Financeiro | Parcelas; antes de E6 |
| D07 | Quais percentuais/unidades de juros, multa e desconto são válidos? | Financeiro e assessoria responsável | Não parametrizar exemplos automaticamente; antes de E6 |
| D08 | Há venda fracionada, embalagens diferentes e conversões? | Estoque/comercial | Precisão, corte e modelo; antes de E2 |
| D09 | Há validade mínima por cliente/produto? | Estoque/comercial | Elegibilidade de lotes; antes de E4 |
| D10 | Aceita cliente PF ou somente PJ? Como cadastrar novo cliente em campo? | Comercial | Documento e fluxo; antes de E2 |
| D11 | Quanto tempo pode operar offline? Como tratar aparelho perdido e troca de carteira? | Direção/TI | Segurança e sincronização; antes de E3 |
| D12 | Quantos vendedores, clientes, produtos e pedidos/dia? Qual indisponibilidade tolerável? | Direção/TI | Carga, infraestrutura e recuperação; antes do piloto |
| D13 | Existem frete, desconto por item, impostos no preço e renegociação? | Financeiro/comercial | Totais e conciliação; antes de E5/E6 |

## Decisões já propostas, ainda sujeitas à ratificação

Android MAUI; administrativo Blazor; API ASP.NET Core; PostgreSQL; SQLite offline; monólito modular; corte manual assistido no MVP; integrações automáticas após piloto. Não representam implementações já realizadas.

## Registro de resolução

Para cada decisão, adicionar: `Resposta`, `Responsável`, `Data`, `Justificativa`, `Requisitos afetados` e `Issue/PR`. Mudanças estruturais geram novo ADR, preservando o anterior.
