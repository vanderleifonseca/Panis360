$ErrorActionPreference = 'Stop'
# Mantém tcp:5207 do celular/emulador apontando para a API do PC via USB (adb reverse).
# O redirecionamento se perde ao desconectar o cabo, suspender o PC ou reiniciar o adb; este script o reaplica.
$adb = (Get-Command adb -ErrorAction SilentlyContinue).Source
if (-not $adb) {
 $candidatos = @("$env:ANDROID_HOME\platform-tools\adb.exe", "$env:LOCALAPPDATA\Android\Sdk\platform-tools\adb.exe", "${env:ProgramFiles(x86)}\Android\android-sdk\platform-tools\adb.exe")
 $adb = $candidatos | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1
}
if (-not $adb) { throw 'adb não encontrado. Instale o Android SDK Platform-Tools.' }
Write-Host 'Panis360 - mantendo acesso USB à API (porta 5207). Ctrl+C para encerrar.'
while ($true) {
 $aparelhos = & $adb devices | Select-String '^(\S+)\s+device$' | ForEach-Object { $_.Matches[0].Groups[1].Value }
 foreach ($serial in $aparelhos) {
  $ativos = & $adb -s $serial reverse --list 2>$null
  if (-not ($ativos -match 'tcp:5207 tcp:5207')) {
   & $adb -s $serial reverse tcp:5207 tcp:5207 | Out-Null
   Write-Host "$(Get-Date -Format HH:mm:ss) acesso liberado em $serial"
  }
 }
 if (-not (Test-NetConnection localhost -Port 5207 -InformationLevel Quiet -WarningAction SilentlyContinue)) {
  Write-Host "$(Get-Date -Format HH:mm:ss) atenção: API não está respondendo na porta 5207"
 }
 Start-Sleep -Seconds 5
}
