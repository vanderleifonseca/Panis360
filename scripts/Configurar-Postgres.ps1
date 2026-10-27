$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot -Parent)
Write-Host 'Use um banco PostgreSQL novo e vazio. Este comando não transfere dados do SQLite.'
$connection = Read-Host 'Conexão PostgreSQL (Host=localhost;Port=5432;Database=panis360;Username=...;Password=...)' -AsSecureString
$pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($connection)
try {
 $plain = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer)
 & dotnet user-secrets set 'ConnectionStrings:Main' $plain --project src/Panis360.Api
 if ($LASTEXITCODE -ne 0) { throw 'Falha ao salvar a conexão.' }
 & dotnet user-secrets set 'Database:Provider' 'PostgreSQL' --project src/Panis360.Api
 if ($LASTEXITCODE -ne 0) { throw 'Falha ao configurar PostgreSQL.' }
} finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer); $plain=$null }
Write-Host 'Provedor configurado. Ao iniciar em Development, a API criará o esquema no banco vazio.'
