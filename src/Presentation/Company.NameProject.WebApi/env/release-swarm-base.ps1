param(
    [string]$UserShh, 
    [string]$AcrName, 
    [string]$ImageName,
    [string]$StackName,
    [string]$SwarmManager,
    [string]$EnvName,
    [int]$Replicate
)


$ErrorActionPreference = "Stop"

# Fija el directorio de trabajo al del script para que rutas relativas (change/, logs/) sean estables.
$scriptDir = Split-Path -Parent $PSCommandPath
Set-Location $scriptDir

# Limpia la carpeta published antes de generar un nuevo artefacto.
$publishedPath = Join-Path $scriptDir "..\published"
if (Test-Path -Path $publishedPath) {
    Write-Host "Eliminando carpeta published existente..."
    Remove-Item -Recurse -Force $publishedPath
}

# Calcula rutas absolutas para docker build.
$webApiDir = (Resolve-Path (Join-Path $scriptDir "..")).Path
$dockerfilePath = Join-Path $webApiDir "Dockerfile"

if (!(Test-Path "logs")) { New-Item -ItemType Directory logs | Out-Null }

$LogFile = "logs/deploy_$(Get-Date -Format 'yyyyMMdd_HHmmss').log"

function Log($msg) {
    $time = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $line = "$time | $msg"
    Write-Host $line
    Add-Content $LogFile $line
}

# =====================================
# 1) OBTENIENDO VERSION SEGUN AMBIENTE
# =====================================

Write-Host "🚀 INICIO DEPLOY - $EnvName"

$envNorm = $EnvName.ToLower().Trim()

$currentVersion = ""
if ($envNorm -eq "development") {
    $currentVersion = Get-Content change/version-dev    
}
if ($envNorm -eq "qa") {
    $currentVersion = Get-Content change/version-qa  
}
if ($envNorm -eq "production") {
    $currentVersion = Get-Content change/version-prod  
}
Write-Host "✅ Versión Actual::::::: $currentVersion"

$parts = $currentVersion -split '\.'
$patch = [int]$parts[2] + 1
$newVersion = "$($parts[0]).$($parts[1]).$patch"

Write-Host "✅ Versión Nueva::::::: $newVersion"

# 1. Construimos la ruta completa de la imagen
$fullImage = "${AcrName}/${ImageName}:${newVersion}"


Write-Host "✅ Recuperando datos de la rama"
# --- Paso 5: Obtener commit actual ---
Write-Host "✅ Repositorio actual: $(git remote get-url origin)"
$ramaDevop = git rev-parse --abbrev-ref HEAD
Write-Host "✅ Rama actual: $ramaDevop"
$commit = (git rev-parse --short HEAD)
Write-Host "✅ Commit actual: $commit"

# ===============================
# 2) BUILD IMAGEN
# ===============================

# Publicar la aplicación antes de construir la imagen
Write-Host "📦 Publicando aplicación..."
dotnet publish $webApiDir -c release -o (Join-Path $webApiDir "published")
if ($LASTEXITCODE -ne 0) {
    Log "❌ Error en dotnet publish"
    exit 1
}

Write-Host "🐳 Generando imagen Docker..."
$buildDate = Get-Date -Format "yyyy-MM-dd"

Log "INICIO"
docker build -t $fullImage `
  --label version=$newVersion `
  --label build_date=$buildDate `
  --label commit=$commit `
  --label repo=$ramaDevop `
  --label maintainer="correo@cresa.ec" `
    -f $dockerfilePath $webApiDir
Log "FIN"

if ($LASTEXITCODE -ne 0) {
    Log "❌ Error en docker build"
    exit 1
}

# ===============================
# 3) PUSH A DOCKER REGISTRY LOCAL
# ===============================
Write-Host "📤 Ejecutando Push a Docker Registry $AcrName..."
Log "INICIO"
docker push $fullImage
Log "FIN"

if ($LASTEXITCODE -ne 0) {
    Log "❌ Error en docker push"
    exit 1
}


# ===============================
# 5) DEPLOY STACK SWARM
# ===============================
Write-Host "🚀 Desplegando stack en Swarm..."

ssh $UserShh@${SwarmManager} "docker service rm $StackName 2>/dev/null; docker pull $fullImage && docker service create --name $StackName --replicas $Replicate --network docker-cresa-attachable --restart-condition any --restart-delay 5s --restart-max-attempts 3 --update-parallelism 1 --update-delay 10s --update-order start-first -e ASPNETCORE_ENVIRONMENT=$EnvName $fullImage"

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error al crear servicio $StackName." -ForegroundColor Red
    exit 1
}
Start-Sleep -Seconds 10

# ===============================
# 6) ACTUALIZAR LOG
# ===============================

Write-Host "🎉 DEPLOY COMPLETADO - versión $newVersion"
# --- Paso 3: Actualizar archivo VERSION ---
if ($envNorm -eq "development") {
    $newVersion | Out-File change/version-dev -Encoding utf8 -NoNewline
}
if ($envNorm -eq "qa") {
   $newVersion | Out-File change/version-qa -Encoding utf8 -NoNewline
}

# Limpia la carpeta published antes de generar un nuevo artefacto.
$publishedPath = Join-Path $scriptDir "..\published"
if (Test-Path -Path $publishedPath) {
    Write-Host "Eliminando carpeta published existente..."
    Remove-Item -Recurse -Force $publishedPath
}

$header = "`n## [$newVersion] - $buildDate`n- Descripción de cambios deben ir aqui o se debe obtener del
#devops para tener trazabilidad de los cambios...`n"
Add-Content "change/CHANGELOG.md" $header
Write-Host "📝 CHANGELOG actualizado."


