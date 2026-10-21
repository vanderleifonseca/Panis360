# 08 — Qualidade, segurança e implantação

## Matriz mínima de testes

| Cenário | Resultado esperado |
| --- | --- |
| Vendedor consulta cliente de outra carteira | API impede acesso |
| Duas atribuições territoriais simultâneas | Apenas uma exclusividade válida |
| Pedido offline e reinício | Pedido preservado e pendente |
| Resposta perdida após gravação central | Reenvio recupera pedido sem duplicação |
| Mesma chave com corpo alterado | Conflito explícito |
| Sincronização interrompida entre páginas | Retomada sem perder mudanças |
| Corte com múltiplos e sobras | Soma aprovada respeita saldo e embalagens |
| Aprovações simultâneas | Nenhuma superalocação |
| Lote vence entre reserva e saída | Expedição bloqueada e revisão aberta |
| Cancelamento antes da saída | Reserva liberada uma vez |
| Referência fiscal repetida | Sem duplicar venda/títulos |
| Entrega parcial e devolução | Histórico, quarentena e ajuste conciliados |
| Parcelamento de R$ 100 em três | 33,33 + 33,33 + 33,34 |
| Recebimento parcial e estorno | Saldo recomposto sem apagar histórico |
| Restauração de backup | Pedidos, estoque e financeiro conciliados |

Testes de domínio para cálculos; integração com PostgreSQL para concorrência/restrições; dispositivo/emulador para offline e ciclo de vida Android; teste ponta a ponta para o fluxo do piloto.

## Segurança e dados

- Autorização por perfil, carteira e recurso em toda operação; mudanças sensíveis auditadas.
- Credenciais, certificados, tokens, strings de conexão e dados de clientes ficam fora do Git.
- Usar dados fictícios em exemplos, testes e capturas públicas.
- Segredos de desenvolvimento no gerenciador local apropriado; produção em serviço seguro de segredos.
- HTTPS e banco sem exposição pública desnecessária; acesso operacional restrito.
- Armazenamento seguro de tokens no Android; avaliar criptografia da base local e expiração offline em D11.
- Definir coleta mínima, retenção, responsáveis e atendimento a solicitações de titulares antes do piloto; evitar guardar conteúdo pessoal desnecessário nos logs.
- Auditoria não registra senhas, tokens ou dados bancários completos.

## Ambientes e implantação

Desenvolvimento, homologação e produção separados. Criar pipeline de build/testes após existir código; esta entrega documental não contém um pipeline executável. Promover versão identificada; revisar migration; executar backup; aplicar alteração; validar saúde, autenticação, pedido e consulta de saldo.

Rollback de aplicação não implica reversão automática de banco. Mudanças de esquema devem priorizar compatibilidade; recuperação de dados segue plano testado. Operações fiscais e financeiras externas precisam de reconciliação, não simples rollback técnico.

## Observabilidade

Monitorar fila local pendente, pedidos recebidos, falhas de sincronização, conflitos, saldo reservado, lotes próximos de vencer, falhas da outbox e divergências financeiras. CorrelationId liga requisição, pedido e eventos. Alertas, responsáveis e metas de recuperação serão definidos em D12.

## Antes do piloto

- Validar estoque inicial por lote e títulos de abertura.
- Confirmar processo de emissão fiscal externa e registro da referência.
- Testar backup e restauração; definir RPO/RTO e responsáveis.
- Treinar vendedor, estoque, logística e financeiro com roteiro único.
- Operar uma praça e reconciliar diariamente pedidos, movimentos, entregas e recebimentos.
