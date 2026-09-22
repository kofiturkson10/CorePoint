<#
Bygger klienten, publicerar API:t och paketerar allt till server/publish.zip,
redo för `az webapp deploy`.

Själva zippningen sköts av csproj:et (ZipDirectory-target, se
CompanyPortal.Api.csproj) och körs alltså inne i dotnet-processen (riktig .NET).
Den ska INTE göras här i PowerShell: Windows PowerShell 5.1 kör på .NET
Framework, vars ZipFile-implementation inte normaliserar sökvägsseparatorer
till '/' på Windows - det gav bakåtstreck i zip-postnamnen och kraschade
deployen på Linux App Service (rsync kunde inte extrahera "katalogerna" rätt).
#>
param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$serverDir = $PSScriptRoot
$clientDir = Join-Path (Split-Path -Parent $serverDir) "client"
$wwwrootDir = Join-Path $serverDir "wwwroot"
$publishDir = Join-Path $serverDir "publish"
$zipPath = Join-Path $serverDir "publish.zip"

Write-Host "==> Bygger klienten (vite build)"
Push-Location $clientDir
npm run build
Pop-Location

Write-Host "==> Uppdaterar server/wwwroot med senaste klientbygget"
if (Test-Path $wwwrootDir) {
    Remove-Item $wwwrootDir -Recurse -Force
}
Copy-Item (Join-Path $clientDir "dist") $wwwrootDir -Recurse

Write-Host "==> Publicerar och zippar API:t (dotnet publish -c $Configuration)"
if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}
dotnet publish (Join-Path $serverDir "CompanyPortal.Api.csproj") -c $Configuration -o $publishDir

Write-Host ""
Write-Host "==> Klart: $zipPath"
Write-Host "Deploya med:"
Write-Host "  az webapp deploy --resource-group rg-foretagsportal-ovning --name app-fp-wucbout2vvcwe --src-path `"$zipPath`" --type zip"
