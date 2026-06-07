# Diagnóstico IIS — MMoneyWeb (rodar como Administrator no servidor)
$siteName = "mmoneyweb.com"
$sitePath = "C:\inetpub\vhosts\mmoneyweb.com"
$poolName = "mmoneyweb.com"

Write-Host "=== Site ===" -ForegroundColor Cyan
& C:\Windows\System32\inetsrv\appcmd list site $siteName

Write-Host "`n=== Pool ===" -ForegroundColor Cyan
& C:\Windows\System32\inetsrv\appcmd list apppool $poolName

Write-Host "`n=== aspNetCore config ===" -ForegroundColor Cyan
& C:\Windows\System32\inetsrv\appcmd list config $siteName -section:system.webServer/aspNetCore

Write-Host "`n=== Ultimas linhas do log IIS (W3SVC4) ===" -ForegroundColor Cyan
$w3log = Get-ChildItem "C:\inetpub\logs\LogFiles\W3SVC4\u_ex*.log" -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1
if ($w3log) {
    Get-Content $w3log.FullName -Tail 8
} else {
    Write-Host "Log W3SVC4 nao encontrado."
}

Write-Host "`n=== stdout logs ===" -ForegroundColor Cyan
Get-ChildItem "$sitePath\logs" -ErrorAction SilentlyContinue

Write-Host "`n=== Teste HTTP ===" -ForegroundColor Cyan
try {
    $r = Invoke-WebRequest -Uri "http://www.mmoneyweb.com" -MaximumRedirection 0 -UseBasicParsing -ErrorAction Stop
    Write-Host "Status:" $r.StatusCode
} catch {
    if ($_.Exception.Response) {
        Write-Host "Status:" ([int]$_.Exception.Response.StatusCode)
    } else {
        Write-Host $_.Exception.Message
    }
}
