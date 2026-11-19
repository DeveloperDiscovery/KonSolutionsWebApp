# Genera la carpeta de instalacion de la web: Instalador.exe + Host (el servidor) + Sitio (la aplicacion Blazor publicada). Windows x64, no necesita instalar .NET en el servidor.
# Uso (en una maquina con el SDK de .NET 9):   .\publicar.ps1 -Salida C:\temp\KonSolutionsWebInstalacion
param([string]$Salida = ".\KonSolutionsWebInstalacion")
$ErrorActionPreference = "Stop"
$raiz = Split-Path $PSScriptRoot -Parent   # la carpeta KonSolutionsWebApp
if (Test-Path $Salida) { Remove-Item $Salida -Recurse -Force }
$tmp = Join-Path ([IO.Path]::GetTempPath()) ("konweb_" + [guid]::NewGuid().ToString("N"))
dotnet publish "$raiz\KonSolutions.Web\KonSolutions.Web.csproj" -c Release -o $tmp
if ($LASTEXITCODE -ne 0) { throw "Fallo la publicacion de la web" }
New-Item -ItemType Directory -Path $Salida | Out-Null
Move-Item "$tmp\wwwroot" "$Salida\Sitio"
Remove-Item $tmp -Recurse -Force
dotnet publish "$raiz\KonSolutions.WebHost\KonSolutions.WebHost.csproj" -c Release -r win-x64 --self-contained true -o "$Salida\Host"
if ($LASTEXITCODE -ne 0) { throw "Fallo la publicacion del servidor" }
dotnet publish "$PSScriptRoot\KonSolutionsWebInstalador.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o $Salida
if ($LASTEXITCODE -ne 0) { throw "Fallo la publicacion del instalador" }
Write-Host ""
Write-Host "Listo en $Salida. Copia esa carpeta al servidor y ejecuta Instalador.exe como Administrador."
