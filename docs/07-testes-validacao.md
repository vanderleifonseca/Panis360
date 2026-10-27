# 07 — Testes e validação

## Testes automatizados

```powershell
dotnet run --project tests/Panis360.Tests
```

É um programa de console (sem framework de testes) que roda as verificações em sequência e para na primeira falha. Usa um banco SQLite temporário, apagado ao final, e não toca no banco do aplicativo. Saída esperada: `Concluído: 15 verificações aprovadas.`

| # | Verificação | Regra coberta |
| --- | --- | --- |
| 1 | Parcelamento 7/14 de R$ 235,00 → 117,50 + 117,50 | Parcelas iguais |
| 2 | R$ 100,00 em três → soma 100 e última 33,34 | Ajuste de centavos |
| 3 | Percentuais que não somam 100% são rejeitados | Validação da condição |
| 4 | Reenvio do mesmo pedido não duplica | Idempotência |
| 5 | Reenvio por outro vendedor é rejeitado | Idempotência + carteira |
| 6 | Venda acima do estoque é rejeitada | Estoque suficiente |
| 7 | Falha na venda não gera venda nem títulos | Atomicidade |
| 8 | Corte preserva a quantidade solicitada | Corte |
| 9 | Total da venda usa a quantidade cortada | Total |
| 10 | Lote vencido não é usado e a baixa segue a validade | FEFO |
| 11 | Venda gera parcelas com vencimentos corretos | Parcelamento |
| 12 | Confirmar novamente não duplica a venda | Idempotência da venda |
| 13 | Recebimento parcial e reenvio sem duplicar | Idempotência do recebimento |
| 14 | Recebimento acima do saldo é rejeitado | Saldo |
| 15 | Cliente bloqueado não gera novo pedido | Bloqueio |

Para adicionar um teste, use `Check(condição, "nome")` ou `Reject(ação, "nome")` em `tests/Panis360.Tests/Program.cs`.

## Integração contínua

O workflow **Compilar e verificar** (`.github/workflows/build.yml`) roda em todo push e pull request: restaura, compila `Panis360.sln` em Release e executa os testes. O status aparece no selo do README.

## Validado

- Compilação de `Panis360.sln` (Domain, Contracts, Application, Infrastructure, Api, Web e Tests) sem avisos nem erros, no Windows e no Ubuntu (CI).
- As 15 verificações automatizadas.
- API e painel em execução local no Windows: health, login e telas de pedidos carregando dados da API.
- Testes HTTP manuais da API: login, recusa sem token, permissão por perfil, isolamento de carteira, pedido e reenvio, corte, venda e reenvio, parcelas, recebimento parcial e reenvio, visitas, logout e invalidação da sessão.
- Código compartilhado do app MAUI compilado contra as mesmas referências.

## Ainda falta validar

| Item | Como validar |
| --- | --- |
| App MAUI em aparelho real (Android e Windows) | Instalar, entrar, usar offline (modo avião), reiniciar o app e sincronizar |
| SecureStorage, SQLite no aparelho, reenvio após timeout | Testes manuais no dispositivo |
| PostgreSQL com concorrência real | Duas vendas simultâneas do mesmo lote; backup e restauração |
| Navegação completa do painel em vários navegadores | Roteiro de [02 — Instalação](02-instalacao.md#5-roteiro-de-teste-do-fluxo-completo) |
| Scripts PowerShell em máquinas diferentes | Rodar `Preparar.ps1`, `Configurar-Postgres.ps1` e `Celular.ps1` |

Esta base serve para desenvolvimento e homologação. Ela não passou por homologação fiscal, bancária ou de produção.

## Lacunas de teste conhecidas

- Não há testes automatizados de endpoint HTTP, de autorização por perfil, do painel ou do app.
- Não há testes contra PostgreSQL.
- Ajuste de lote, histórico de movimentações e cancelamento de pedido não têm verificação automatizada.
