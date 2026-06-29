param(
    [Parameter(Mandatory)][string]$UserShh,       # usuario@servidor para SSH
    [Parameter(Mandatory)][string]$AcrName,       # Registry Docker  (ej. 192.168.x.x:5000)
    [Parameter(Mandatory)][string]$ImageName,     # Nombre de la imagen sin tag
    [Parameter(Mandatory)][string]$StackName,     # Nombre del servicio en Swarm
    [Parameter(Mandatory)][string]$SwarmManager,  # Hostname/IP del nodo manager
    [Parameter(Mandatory)][string]$EnvName,       # Development | QA | Production
    [Parameter(Mandatory)][int]   $Replicate      # Número de réplicas
)

$ErrorActionPreference = "Stop"

# Fija el directorio de trabajo al del script para que rutas relativas sean estables.
$scriptDir = Split-Path -Parent $PSCommandPath
Set-Location $scriptDir

# Limpia la carpeta published antes de generar un nuevo artefacto.
$publishedPath = Join-Path $scriptDir "..\published"
if (Test-Path -Path $publishedPath) {
    Write-Host "Eliminando carpeta published existente..."
    Remove-Item -Recurse -Force $publishedPath
}

# Rutas absolutas para docker build
$webApiDir      = (Resolve-Path (Join-Path $scriptDir "..")).Path
$dockerfilePath = Join-Path $webApiDir "Dockerfile"

if (!(Test-Path "logs")) { New-Item -ItemType Directory logs | Out-Null }
$LogFile = "logs/deploy_$(Get-Date -Format 'yyyyMMdd_HHmmss').log"

function Log($msg) {
    $time = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $line = "$time | $msg"
    Write-Host $line
    Add-Content $LogFile $line
}

# ─── 1) VERSIÓN ──────────────────────────────────────────────────────────────

Write-Host "🚀 INICIO DEPLOY - $EnvName"
$envNorm = $EnvName.ToLower().Trim()

$versionFile = switch ($envNorm) {
    "development" { "change/version-dev"  }
    "qa"          { "change/version-qa"   }
    "production"  { "change/version-prod" }
    default       { throw "EnvName no reconocido: $EnvName" }
}

$currentVersion = Get-Content $versionFile
Write-Host "✅ Versión actual: $currentVersion"

$parts      = $currentVersion -split '\.'
$newVersion = "$($parts[0]).$($parts[1]).$([int]$parts[2] + 1)"
Write-Host "✅ Versión nueva : $newVersion"

$fullImage = "${AcrName}/${ImageName}:${newVersion}"

# ─── 2) INFO DE GIT ──────────────────────────────────────────────────────────

Write-Host "✅ Repositorio: $(git remote get-url origin)"
$branch = git rev-parse --abbrev-ref HEAD
$commit = git rev-parse --short HEAD
Write-Host "✅ Rama: $branch  |  Commit: $commit"

# ─── 3) BUILD ────────────────────────────────────────────────────────────────

Write-Host "📦 Publicando aplicación..."
dotnet publish $webApiDir -c release -o (Join-Path $webApiDir "published")
if ($LASTEXITCODE -ne 0) { Log "❌ Error en dotnet publish"; exit 1 }

Write-Host "🐳 Generando imagen Docker..."
$buildDate = Get-Date -Format "yyyy-MM-dd"
Log "BUILD INICIO"
docker build -t $fullImage `
    --label version=$newVersion `
    --label build_date=$buildDate `
    --label commit=$commit `
    --label branch=$branch `
    -f $dockerfilePath $webApiDir
Log "BUILD FIN"
if ($LASTEXITCODE -ne 0) { Log "❌ Error en docker build"; exit 1 }

# ─── 4) PUSH ─────────────────────────────────────────────────────────────────

Write-Host "📤 Push a Registry $AcrName..."
Log "PUSH INICIO"
docker push $fullImage
Log "PUSH FIN"
if ($LASTEXITCODE -ne 0) { Log "❌ Error en docker push"; exit 1 }

# ─── 5) DEPLOY EN SWARM ──────────────────────────────────────────────────────

Write-Host "🚀 Desplegando stack en Swarm ($SwarmManager)..."
ssh $UserShh@${SwarmManager} @"
docker service rm $StackName 2>/dev/null
docker pull $fullImage
docker service create \
  --name $StackName \
  --replicas $Replicate \
  --network docker-cresa-attachable \
  --restart-condition any \
  --restart-delay 5s \
  --restart-max-attempts 3 \
  --update-parallelism 1 \
  --update-delay 10s \
  --update-order start-first \
  -e ASPNETCORE_ENVIRONMENT=$EnvName \
  $fullImage
"@
if ($LASTEXITCODE -ne 0) { Write-Host "❌ Error al crear servicio $StackName." -ForegroundColor Red; exit 1 }

Start-Sleep -Seconds 10

# ─── 6) ACTUALIZAR VERSIÓN Y CHANGELOG ───────────────────────────────────────

$newVersion | Out-File $versionFile -Encoding utf8 -NoNewline

$header = "`n## [$newVersion] - $buildDate`n- (Describir cambios aquí)`n"
Add-Content "change/CHANGELOG.md" $header

Write-Host "🎉 DEPLOY COMPLETADO — versión $newVersion"
