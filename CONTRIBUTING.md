# Como contribuir

## Fluxo

1. Abra uma **issue** com o objetivo e os critérios de aceite (use o modelo *Tarefa* ou *Problema*).
2. Crie uma branch curta a partir da `main`: `feat/...`, `fix/...` ou `docs/...`.
3. Faça mudanças pequenas e atualize a documentação afetada em `docs/`.
4. Antes de enviar, rode localmente:

   ```powershell
   dotnet build Panis360.sln
   dotnet run --project tests/Panis360.Tests
   ```

   Se mexeu no app: `dotnet build src/Panis360.App -f net10.0-android`.
5. Abra o **pull request** usando o modelo. O CI (*Compilar e verificar*) precisa passar.

## Mensagens de commit

Prefixos curtos em português: `feat:`, `fix:`, `docs:`, `refactor:`, `test:`, `chore:`. Exemplo: `feat: adiciona estorno de recebimento`.

## Onde colocar cada coisa

| Mudança | Lugar |
| --- | --- |
| Entidade ou campo persistido | `src/Panis360.Domain/Entities.cs` e `AppDb.cs`, mais um caminho de atualização do banco ([05 — Modelo de dados](docs/05-modelo-dados.md)) |
| DTO de entrada ou saída | `src/Panis360.Contracts/Models.cs` (afeta API, painel e app) |
| Regra pura (cálculo, validação) | `src/Panis360.Application/Rules.cs` |
| Operação com transação | `src/Panis360.Infrastructure/Operations.cs` |
| Endpoint | `src/Panis360.Api/Program.cs`, com a política de perfil adequada |
| Tela do painel | `src/Panis360.Web/Components/Pages/` |
| Teste | `tests/Panis360.Tests/Program.cs` |

## Regras

- A autorização é sempre feita **na API**. Esconder um botão não é controle de acesso.
- Valores monetários usam `decimal` e `Rules.Money`, nunca `float` ou `double`.
- Operações que podem ser reenviadas precisam de identificador idempotente.
- Uma mudança de regra de negócio atualiza [03 — Regras de negócio](docs/03-regras-negocio.md); um endpoint novo atualiza [06 — API](docs/06-api.md).
- Uma mudança estrutural gera um novo ADR em `docs/adr/`.

## Segurança

O repositório é **público**. Nunca envie senhas, tokens, strings de conexão, certificados, arquivos `.env`, bancos (`*.db`, `*.db.bak*`) ou dados reais de clientes. Use dados fictícios em exemplos, testes e capturas de tela. Segredos locais ficam em `dotnet user-secrets`.
