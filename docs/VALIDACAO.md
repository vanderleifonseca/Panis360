# Validação da entrega

## Executado neste ambiente

- SDK usado: .NET 10.0.402 em Linux.
- Solução Panis360.sln: compilação aprovada, 0 avisos e 0 erros. Inclui Domain, Contracts, Application, Infrastructure, Api, Web e Tests. MAUI fica fora da solução principal para não exigir workload móvel na compilação do backend.
- Testes automatizados em tests/Panis360.Tests: 15 verificações aprovadas em SQLite temporário.
- API iniciada em Development com senha gerada exclusivamente para testes e banco temporário fora do pacote.
- Testes HTTP: health, login, recusa sem token, permissão por perfil, isolamento da carteira, recusa de pedido para cliente de outro vendedor, criação e reenvio do pedido, falta de estoque, corte, venda, reenvio da venda, parcelas, recebimento parcial, reenvio do recebimento, isolamento de títulos, visitas, logout e invalidação da sessão.
- Blazor: compilação e resposta HTTP 200 do documento inicial contendo o carregador blazor.web.js.
- MAUI: os arquivos C# compartilhados (sem Platforms) foram compilados em um projeto temporário net10.0 com as mesmas referências de Controls, SQLite e Contracts; 0 avisos e 0 erros. Isso verifica tipos e sintaxe compartilhada, não a compilação nativa Android/Windows.
- PostgreSQL: esquema SQL inicial gerado pelo modelo EF/Npgsql sem conexão ao banco.

## Ainda exige verificação no ambiente de destino

- Navegação interativa e aparência do painel em navegador: não executadas. A instalação do navegador de teste não ficou disponível; o teste HTTP do documento inicial não substitui a execução de componentes interativos.
- Compilação, instalação e execução MAUI em Android e Windows: não executadas, pois este ambiente não dispõe dos respectivos SDKs nativos.
- SecureStorage, SQLite no aparelho, perda de conexão e reenvio real após timeout: testar no dispositivo.
- PostgreSQL em execução, transações concorrentes, backup e restauração: não executados. Os testes de persistência desta entrega usam SQLite.
- Scripts PowerShell: revisados, mas não executados neste ambiente Linux.

## Cobertura das 15 verificações automatizadas

1. Duas parcelas iguais do exemplo R$ 235,00.
2. Três parcelas e soma correta dos centavos.
3. Percentuais que não somam 100% são rejeitados.
4. Reenvio não duplica pedido.
5. Reenvio por outro vendedor é rejeitado.
6. Estoque insuficiente impede venda.
7. Falha de venda não gera venda ou títulos.
8. Corte mantém a quantidade originalmente solicitada.
9. Total da venda considera a quantidade aceita.
10. Lotes vencidos não são usados e a baixa segue vencimento.
11. Geração de parcelas e vencimentos.
12. Reconfirmação não duplica venda.
13. Recebimento parcial e reenvio sem duplicação.
14. Recebimento acima do saldo é rejeitado.
15. Cliente bloqueado não gera novo pedido.

Consulte ARQUITETURA.md para as funcionalidades implementadas e as evoluções ainda necessárias. Esta base se destina a desenvolvimento e homologação, não representa homologação fiscal, bancária ou de produção.
