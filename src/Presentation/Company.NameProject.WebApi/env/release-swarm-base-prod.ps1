param(
    [Parameter(Mandatory)][string]$UserShh,      # usuario@servidor para SSH (TEST y PROD)
    [Parameter(Mandatory)][string]$AcrName,      # Registry PROD   (ej. 192.168.x.x:5000)
    [Parameter(Mandatory)][string]$ImageName,    # Nombre de la imagen sin tag
    [Parameter(Mandatory)][string]$StackName,    # Nombre del servicio en Swarm PROD
    [Parameter(Mandatory)][string]$EnvName,      # Production
    [Parameter(Mandatory)][string]$Replicate     # Número de réplicas en PROD
)

# ─── Detectar versión desde el servicio en TEST ───────────────────────────────
$testRegistry = "192.168.x.x:5000"   # <── Ajustar: registry TEST
$managerTest  = "srvdockerqa1"        # <── Ajustar: hostname/IP manager TEST

Write-Host "=====================================================" -ForegroundColor Green
Write-Host "🚀 PROMOVIENDO IMAGEN TEST → PRODUCCIÓN..."            -ForegroundColor Green
Write-Host "=====================================================" -ForegroundColor Green

$Version = ssh $UserShh@$managerTest "docker service inspect $StackName --format '{{.Spec.TaskTemplate.ContainerSpec.Image}}' | cut -d'@' -f1 | rev | cut -d':' -f1 | rev"
if (-not $Version) { Write-Host "❌ No se pudo determinar la versión en TEST." -ForegroundColor Red; exit 1 }

$Version   = $Version.Trim()
$testImage = "${testRegistry}/${ImageName}:${Version}"
$fullImage = "${AcrName}/${ImageName}:${Version}"
Write-Host "🆕 Versión: $Version"

Write-Host "1. Descargando imagen de TEST..."
docker pull $testImage
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "2. Reetiquetando para PROD..."
docker tag $testImage $fullImage

Write-Host "3. Subiendo al registry de PROD..."
docker push $fullImage
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "4. Verificando imagen en PROD..."
docker pull $fullImage
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "🆗 Promoción completada." -ForegroundColor Cyan

$respuesta = Read-Host "¿Es primera publicación en producción de esta imagen? (S/N)"

if ($respuesta -in "S","s") {
    Write-Host "═══ COMANDO PARA CREAR SERVICIO EN PRODUCCIÓN ═══" -ForegroundColor Yellow
    Write-Host "docker service rm $StackName 2>/dev/null && \\"
    Write-Host "docker pull $fullImage && \\"
    Write-Host "docker service create --name $StackName \\"
    Write-Host "  --replicas $Replicate \\"
    Write-Host "  --network docker-cresa-attachable \\"
    Write-Host "  --restart-condition any --restart-delay 5s --restart-max-attempts 3 \\"
    Write-Host "  --update-parallelism 1 --update-delay 10s --update-order start-first \\"
    Write-Host "  -e ASPNETCORE_ENVIRONMENT=$EnvName \\"
    Write-Host "  $fullImage"
}
elseif ($respuesta -in "N","n") {
    Write-Host "═══ COMANDO PARA ACTUALIZAR SERVICIO EN PRODUCCIÓN ═══" -ForegroundColor Yellow
    Write-Host "docker pull $fullImage && \\"
    Write-Host "docker service update \\"
    Write-Host "  --image $fullImage \\"
    Write-Host "  --update-parallelism 1 --update-delay 10s --update-order start-first \\"
    Write-Host "  --env-add ASPNETCORE_ENVIRONMENT=$EnvName \\"
    Write-Host "  $StackName"
}
else {
    Write-Host "Respuesta no válida." -ForegroundColor Red
}
