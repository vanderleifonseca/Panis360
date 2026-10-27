## Mudança

O problema e o comportamento resultante.

Fecha #

## Validação

- [ ] `dotnet build Panis360.sln` sem erros
- [ ] `dotnet run --project tests/Panis360.Tests` aprovado
- [ ] App compilado (`dotnet build src/Panis360.App -f net10.0-android`), se alterado
- [ ] Testado manualmente: descreva o roteiro e o resultado

## Checklist

- [ ] Permissões aplicadas na API, não só na interface.
- [ ] Mudança de modelo com caminho de atualização do banco (`database/` ou `SchemaUpgrade.cs`).
- [ ] Documentação em `docs/` atualizada.
- [ ] Sem senhas, tokens, bancos ou dados reais de clientes.
