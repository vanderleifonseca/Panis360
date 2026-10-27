$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot -Parent)
Write-Host 'Panis360 - preparação do ambiente local'
& dotnet --version
if ($LASTEXITCODE -ne 0) { throw 'Instale o SDK .NET 10 antes de continuar.' }
$email = Read-Host 'E-mail do administrador (Enter: admin@panis360.local)'
if ([string]::IsNullOrWhiteSpace($email)) { $email = 'admin@panis360.local' }
$secure = Read-Host 'Crie a senha inicial do administrador (mínimo 12 caracteres)' -AsSecureString
$pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
try {
 $password = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer)
 if ($password.Length -lt 12) { throw 'Senha deve ter pelo menos 12 caracteres.' }
 & dotnet user-secrets set 'Bootstrap:Email' $email --project src/Panis360.Api
 if ($LASTEXITCODE -ne 0) { throw 'Não foi possível salvar o e-mail.' }
 & dotnet user-secrets set 'Bootstrap:Password' $password --project src/Panis360.Api
 if ($LASTEXITCODE -ne 0) { throw 'Não foi possível salvar a senha.' }
} finally {
 [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer)
 $password = $null
}
& dotnet restore Panis360.sln
if ($LASTEXITCODE -ne 0) { throw 'Falha na restauração.' }
& dotnet build Panis360.sln --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Falha na compilação.' }
Write-Host 'Pronto. Abra dois terminais e siga README.md para iniciar API e painel.'
