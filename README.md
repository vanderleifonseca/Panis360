# Panis360

Base funcional de desenvolvimento para distribuidora B2B de produtos de panificação. Solução em C#/.NET 10 com API, Blazor e aplicativo MAUI. Abra esta pasta no Visual Studio Code. O status exato de validação está em docs/VALIDACAO.md.

## 1. Extrair e abrir

Extraia o ZIP em `C:\maui`, obtendo `C:\maui\Panis360`. No PowerShell:

```powershell
cd C:\maui\Panis360
code .
```

Esperado: VS Code abre a pasta com src, tests, scripts, docs e Panis360.sln. Aceite as recomendações de extensões C#, C# Dev Kit e .NET MAUI. Requisitos: SDK .NET 10; para o aplicativo, workloads MAUI, SDK Android e JDK compatíveis. A primeira restauração precisa de internet.

## 2. Preparar e escolher a senha inicial

No terminal integrado do VS Code:

```powershell
dotnet --version
dotnet workload list
.\scripts\Preparar.ps1
```

Esperado: SDK 10.0.x; lista de workloads; solicitação do e-mail e da senha inicial com mínimo de 12 caracteres; compilação sem erros. O script não instala workloads e não altera a política de execução do Windows. Caso a política impeça scripts, utilize os comandos manuais abaixo conforme as regras da sua máquina:

```powershell
dotnet user-secrets set "Bootstrap:Email" "admin@panis360.local" --project src/Panis360.Api
```

Para a senha, prefira executar Preparar.ps1 após resolver a política com o responsável pela máquina. Como alternativa, defina `Bootstrap__Password` no ambiente do processo da API, sem gravá-la no repositório. As senhas de user-secrets são configurações locais, não um cofre criptografado. Alterar Bootstrap após o primeiro cadastro não altera a senha já persistida do usuário.

## 3. Iniciar a API — terminal 1

```powershell
dotnet run --project src/Panis360.Api --launch-profile http
```

Esperado: `Now listening on: http://localhost:5207`. Abra `http://localhost:5207/health`: deve retornar status `ok`. Mantenha este terminal aberto. A primeira execução cria o banco SQLite local da demonstração e as condições à vista, 7/14 e 7/14/21.

## 4. Iniciar o painel — terminal 2

```powershell
dotnet run --project src/Panis360.Web --launch-profile http
```

Esperado: `Now listening on: http://localhost:5116`. Abra `http://localhost:5116` e entre com o e-mail e a senha definidos no passo 2. Mantenha este terminal aberto. A conexão com a API é realizada no servidor Blazor.

## 5. Testar o primeiro fluxo

1. Configurações: cadastre um vendedor. A senha precisa ter 12 caracteres.
2. Clientes: clique Carregar / atualizar, cadastre uma padaria e vincule ao vendedor.
3. Produtos: cadastre Fermento, unidade UN e preço R$ 23,50.
4. Estoque: carregue os dados e registre um lote com validade futura e 10 unidades.
5. Pedidos: carregue os dados, escolha o cliente, condição 7/14, boleto e 12 unidades; envie.
6. O pedido mantém 12 solicitadas. Em Ajustar corte, altere atendimento para 10 e informe falta de estoque.
7. Confirme a venda. Esperado: total R$ 235,00, baixa das 10 unidades e duas parcelas de R$ 117,50.
8. Financeiro: carregue os títulos. Receba R$ 50,00 em uma parcela. Esperado: saldo da parcela R$ 67,50.
9. Entre como vendedor. Esperado: somente a carteira vinculada; sem permissão para cortes, estoque ou baixa financeira.

As telas utilizam Carregar / atualizar para buscar os dados. O botão Confirmar venda altera estoque e financeiro de verdade no banco local. Boleto e PIX são apenas formas de pagamento registradas, sem integração bancária.

## 6. Testes automatizados

```powershell
dotnet run --project tests/Panis360.Tests
```

Esperado: verificações OK e resumo de aprovação. O teste usa banco temporário próprio e não modifica o banco do aplicativo. Verifica parcelamento, cortes, idempotência, falta de estoque, lotes vencidos e recebimentos.

## 7. Aplicativo Android

Com a API em execução, conecte o celular por USB com depuração autorizada, ou inicie o emulador:

```powershell
adb devices
adb reverse tcp:5207 tcp:5207
dotnet build src/Panis360.App -f net10.0-android
```

Esperado: aparelho listado como `device`, redirecionamento da porta e compilação bem-sucedida com o ambiente Android instalado. A solução principal exclui o MAUI para permitir compilar backend/painel em computadores sem workloads móveis. O código do aplicativo está em src/Panis360.App e pode ser aberto pela extensão .NET MAUI.

Para instalar e executar no dispositivo selecionado:

```powershell
dotnet build src/Panis360.App -t:Run -f net10.0-android
```

Esperado: Panis360 Vendas no Android. Se houver vários aparelhos, selecione um pela extensão MAUI; para adb use `adb -s SERIAL reverse tcp:5207 tcp:5207`. No aplicativo, endereço da API: `http://127.0.0.1:5207`. Essa configuração utiliza USB/emulador; não exige abrir a API na rede local. HTTP é permitido apenas no manifesto Android de Debug; Release exige HTTPS.

O `adb reverse` se perde ao desconectar o cabo, suspender o PC ou reiniciar o adb, e o app passa a mostrar `Connection failure`. Para reaplicá-lo automaticamente em todos os aparelhos conectados, mantenha aberto em outro terminal (ou rode a tarefa "Panis360: acesso do celular (USB)" no VS Code):

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Celular.ps1
```

Para usar pelo Wi-Fi, sem cabo: celular e computador na mesma rede. Uma única vez, em um PowerShell como administrador, libere a porta no firewall (rede privada):

```powershell
Set-NetConnectionProfile -InterfaceAlias "Wi-Fi" -NetworkCategory Private
New-NetFirewallRule -DisplayName "Panis360 API 5207" -Direction Inbound -Protocol TCP -LocalPort 5207 -Action Allow -Profile Private
```

Inicie a API no perfil wifi, que escuta em todas as interfaces e aceita qualquer Host:

```powershell
dotnet run --project src/Panis360.Api --launch-profile wifi
```

Descubra o IPv4 do adaptador Wi-Fi com `ipconfig` e, no aplicativo, informe `http://IP_DO_PC:5207` (ex.: `http://192.168.3.196:5207`). Teste antes no navegador do celular: `http://IP_DO_PC:5207/health`. Use apenas em rede confiável; não exponha este perfil à internet.

Entre com a conta do vendedor; atualize carteira e catálogo; abra um cliente; crie um pedido e salve na fila local. Sem conexão, a fila permanece no SQLite. Ao reconectar, toque Sincronizar pedidos pendentes. Preços serão confirmados no servidor, podendo diferir do total estimado offline. Cada pedido pendente preserva seu identificador ao ser reenviado.

O código inclui Windows também. Compile no Windows com `dotnet build src/Panis360.App -f net10.0-windows10.0.19041.0`. Não foram incluídos alvos iOS/macOS.

## 8. Usar PostgreSQL

Você pode usar seu PostgreSQL instalado ou o serviço opcional em compose.yaml. Para Docker:

```powershell
Copy-Item .env.example .env
code .env
```

Substitua a senha de exemplo por uma senha sua, salve e execute:

```powershell
docker compose up -d db
.\scripts\Configurar-Postgres.ps1
```

Esperado: banco disponível na porta 5432 e script solicitando a conexão. Informe Host=localhost;Port=5432;Database=panis360;Username=panis360;Password=SUA_SENHA. Reinicie a API no perfil http. Em um banco novo, o esquema e o administrador inicial serão criados. Os dados da demonstração SQLite não são transferidos. Se já houver PostgreSQL na porta 5432, utilize-o diretamente ou ajuste a porta do compose e a conexão.

O arquivo SQL inicial está em database/postgresql-inicial.sql. Ele é alternativa ao EnsureCreated, não um passo adicional obrigatório. Bancos PostgreSQL criados por esse arquivo antes da coluna de usuário nas movimentações de estoque devem executar database/postgresql-002-usuario-movimentacao.sql; em desenvolvimento, a API aplica essa atualização sozinha na inicialização. Atualizações futuras do modelo exigirão migrations; consulte docs/ARQUITETURA.md.

## Solução de problemas

- Não conecta ao painel/API: mantenha os dois terminais abertos; verifique as portas com `Test-NetConnection localhost -Port 5207` e `Test-NetConnection localhost -Port 5116`. Esperado: TcpTestSucceeded True.
- API fecha no primeiro uso: confira Bootstrap:Password e o log do terminal.
- Sessão expirada: entre novamente. Recarregar o navegador completamente pode exigir novo login.
- NETSDK1147: workloads ausentes; instale os componentes MAUI correspondentes antes de compilar o app.
- Falha de Java/Android: confira SDK/JDK e o diagnóstico da extensão MAUI; backend e painel continuam compiláveis separadamente.
- Modelo alterado: EnsureCreated não atualiza banco existente. Não apague banco com dados reais; prepare migrations e backup.

## Documentação

- docs/ARQUITETURA.md: decisões, segurança e limitações.
- docs/VALIDACAO.md: verificações realizadas e pendentes.
- docs/ENDPOINTS.md: rotas da API.

Esta entrega prepara o código para desenvolvimento e testes. Integrações fiscais/bancárias, devoluções, estornos, agendamento automático e endurecimento para produção são etapas posteriores.
