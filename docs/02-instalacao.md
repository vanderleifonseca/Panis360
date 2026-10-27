# 02 — Instalação e execução

Comandos para PowerShell no Windows, a partir da raiz do repositório. Backend e painel também compilam em Linux e macOS (o CI roda em Ubuntu).

## Requisitos

| Para | Precisa de |
| --- | --- |
| API, painel e testes | SDK .NET 10 (`global.json` fixa 10.0.100 ou superior na mesma versão principal) |
| Aplicativo Android | Workload `maui`, SDK Android e JDK compatível, celular com depuração USB ou emulador |
| Aplicativo Windows | Workload `maui` no Windows 10 (build 17763) ou superior |
| PostgreSQL (opcional) | PostgreSQL instalado ou Docker |

Editor recomendado: Visual Studio Code com as extensões C#, C# Dev Kit e .NET MAUI. Abra `Panis360.code-workspace` ou a pasta raiz. As tarefas prontas ficam em **Terminal → Executar tarefa** (`Panis360: API`, `Panis360: painel`, `Panis360: testes`, `Panis360: compilar Android`, `Panis360: acesso do celular (USB)`).

## 1. Obter o código

```powershell
git clone https://github.com/vanderleifonseca/Panis360.git
cd Panis360
code .
```

## 2. Preparar e definir o administrador

```powershell
dotnet --version
.\scripts\Preparar.ps1
```

O script pede o e-mail (padrão `admin@panis360.local`) e a senha inicial do administrador, com no mínimo 12 caracteres. Ele grava os dois em `dotnet user-secrets` do projeto da API e compila a solução. Não instala workloads nem altera a política de execução do Windows.

Se a política de execução bloquear scripts, faça manualmente:

```powershell
dotnet user-secrets set "Bootstrap:Email" "admin@panis360.local" --project src/Panis360.Api
dotnet user-secrets set "Bootstrap:Password" "SUA_SENHA_COM_12+" --project src/Panis360.Api
dotnet build Panis360.sln
```

O administrador só é criado quando o banco **não tem nenhum usuário**. Mudar `Bootstrap` depois disso não altera a senha já gravada. Os user-secrets ficam no seu perfil do Windows, fora do repositório, mas não são um cofre criptografado.

## 3. Iniciar a API (terminal 1)

```powershell
dotnet run --project src/Panis360.Api --launch-profile http
```

Esperado: `Now listening on: http://localhost:5207`. Teste em `http://localhost:5207/health`, que deve responder `{"status":"ok","system":"Panis360"}`.

Na primeira execução, a API cria o banco SQLite `src/Panis360.Api/panis360.db`, o administrador e as condições À vista, 7/14 e 7/14/21.

## 4. Iniciar o painel (terminal 2)

```powershell
dotnet run --project src/Panis360.Web --launch-profile http
```

Esperado: `Now listening on: http://localhost:5116`. Abra no navegador e entre com o administrador. O painel chama a API pelo servidor, no endereço configurado em `ApiUrl` (`src/Panis360.Web/appsettings.json`).

Mantenha os dois terminais abertos. Se só o painel estiver rodando, as telas abrem, mas nenhum dado carrega.

## 5. Roteiro de teste do fluxo completo

1. **Configurações:** cadastre um usuário Vendedor (senha com 12 caracteres ou mais).
2. **Clientes:** clique em *Carregar / atualizar*, cadastre uma padaria e vincule ao vendedor.
3. **Produtos:** cadastre "Fermento", unidade UN, preço R$ 23,50.
4. **Estoque:** registre um lote com validade futura e 10 unidades.
5. **Pedidos:** escolha o cliente, a condição 7/14 dias, Boleto e 12 unidades; envie.
6. Em **Ajustar corte**, altere o atendido para 10 e informe "falta de estoque". O pedido continua mostrando 12 solicitadas.
7. **Confirmar venda.** Esperado: total R$ 235,00, baixa das 10 unidades e duas parcelas de R$ 117,50.
8. **Contas a receber:** receba R$ 50,00 de uma parcela. Esperado: saldo da parcela R$ 67,50.
9. Saia e entre como o vendedor. Esperado: só a carteira dele, sem acesso a corte, estoque ou recebimento.

*Confirmar venda* altera estoque e financeiro de verdade no banco local.

## 6. Aplicativo Android

O projeto `src/Panis360.App` fica **fora** de `Panis360.sln`, para que backend e painel compilem em máquinas sem workloads móveis.

### Pelo cabo USB ou pelo emulador (recomendado)

Com a API rodando:

```powershell
adb devices                          # aparelho deve aparecer como "device"
adb reverse tcp:5207 tcp:5207        # o celular acessa a API do PC pela porta 5207
dotnet build src/Panis360.App -t:Run -f net10.0-android
```

No aplicativo, o endereço da API é `http://127.0.0.1:5207` (já vem preenchido). Com vários aparelhos, use `adb -s SERIAL reverse tcp:5207 tcp:5207`.

O `adb reverse` se perde ao desconectar o cabo, suspender o PC ou reiniciar o adb, e então o app mostra `Connection failure`. Para reaplicá-lo automaticamente, deixe rodando em outro terminal:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Celular.ps1
```

### Pelo Wi-Fi

Celular e PC na mesma rede **confiável**. Uma única vez, em PowerShell como administrador:

```powershell
Set-NetConnectionProfile -InterfaceAlias "Wi-Fi" -NetworkCategory Private
New-NetFirewallRule -DisplayName "Panis360 API 5207" -Direction Inbound -Protocol TCP -LocalPort 5207 -Action Allow -Profile Private
```

Inicie a API no perfil `wifi`, que escuta em todas as interfaces e aceita qualquer Host:

```powershell
dotnet run --project src/Panis360.Api --launch-profile wifi
```

Descubra o IPv4 do PC com `ipconfig` e informe no app `http://IP_DO_PC:5207`. Antes, teste no navegador do celular: `http://IP_DO_PC:5207/health`. Não exponha este perfil à internet.

### Usar o aplicativo

Entre com a conta do vendedor, toque em *Atualizar carteira e catálogo*, abra um cliente, monte o pedido e toque em **Salvar pedido na fila local**. Sem conexão, o pedido fica no SQLite do aparelho. Ao reconectar, toque em **Sincronizar pedidos pendentes**. O preço final é sempre o do servidor e pode diferir do total estimado offline.

HTTP só é permitido no manifesto Android de **Debug**; um build Release exige HTTPS.

### Windows

```powershell
dotnet build src/Panis360.App -f net10.0-windows10.0.19041.0
```

Não há alvo iOS ou macOS.

## 7. Usar PostgreSQL

SQLite é o padrão para demonstração. Para PostgreSQL, use um banco **novo e vazio**. Os dados do SQLite **não** são transferidos.

Com Docker (o serviço só escuta em `127.0.0.1:5432`):

```powershell
Copy-Item .env.example .env
code .env                        # troque a senha de exemplo e salve
docker compose up -d db
```

Depois, com Docker ou com um PostgreSQL próprio:

```powershell
.\scripts\Configurar-Postgres.ps1
```

Informe a conexão, por exemplo `Host=localhost;Port=5432;Database=panis360;Username=panis360;Password=SUA_SENHA`. O script grava `ConnectionStrings:Main` e `Database:Provider=PostgreSQL` nos user-secrets da API. Reinicie a API: em Development, ela cria o esquema e o administrador.

Também é possível criar o esquema manualmente com `database/postgresql-inicial.sql`, em vez de deixar a API criá-lo (nunca os dois no mesmo banco). Bancos criados por esse script antes da coluna de usuário nas movimentações precisam de `database/postgresql-002-usuario-movimentacao.sql`; em Development, a API aplica essa atualização sozinha.

## Configurações

| Chave | Projeto | Padrão | Uso |
| --- | --- | --- | --- |
| `Bootstrap:Email` / `Bootstrap:Password` | Api | `admin@panis360.local` / — | Administrador criado no banco vazio |
| `Database:Provider` | Api | `SQLite` | `SQLite` ou `PostgreSQL` |
| `ConnectionStrings:Main` | Api | `Data Source=panis360.db` | Conexão do banco (obrigatória para PostgreSQL) |
| `Database:CreateSchema` | Api | `true` | Cria o esquema em Development |
| `AllowedHosts` | Api | `localhost;127.0.0.1;10.0.2.2` | Hosts aceitos (o perfil `wifi` usa `*`) |
| `ApiUrl` | Web | `http://localhost:5207` | Endereço da API usado pelo painel |

A API também lê `appsettings.Local.json` (ignorado pelo Git) e variáveis de ambiente, como `Bootstrap__Password`.

## Solução de problemas

| Sintoma | Causa provável e solução |
| --- | --- |
| Painel abre mas não carrega dados, ou dá erro de conexão | A API não está rodando. Confira com `Test-NetConnection localhost -Port 5207` e `-Port 5116` |
| API fecha ao iniciar pela primeira vez | `Bootstrap:Password` ausente ou com menos de 12 caracteres. Veja a mensagem no terminal |
| "Sessão expirada" no painel | A sessão vive na memória da conexão do navegador e dura 12 horas. Recarregar a página pode exigir novo login |
| App mostra `Connection failure` | `adb reverse` perdido. Rode `scripts/Celular.ps1` ou repita o comando |
| `NETSDK1147` | Workload MAUI ausente. Rode `dotnet workload install maui` |
| Erro de Java ou SDK Android | Confira o SDK e o JDK no diagnóstico da extensão .NET MAUI. API e painel continuam compilando |
| Erro de coluna ou tabela após mudar o modelo | `EnsureCreated` não atualiza bancos existentes. **Não apague um banco com dados reais**; faça backup e prepare uma migration |
| `429 Too Many Requests` no login | Mais de 10 tentativas por minuto do mesmo IP. Aguarde um minuto |
