param(
    [string]$UserShh, 
    [string]$AcrName, 
    [string]$ImageName,
    [string]$StackName,   
    [string]$EnvName,
    [string]$Replicate
)


$testRegistry = "192.168.82.174:5000"
$managerTest = "192.168.82.174"
# --- Detectar versión automáticamente ---

Write-Host "=====================================================" -ForegroundColor Green
Write-Host "🚀 PROMOVIENDO IMAGEN Write-Host TEST → PRODUCCIÓN..." -ForegroundColor Green 
Write-Host "=====================================================" -ForegroundColor Green

$Version = ssh $UserShh@$managerTest "docker service inspect $StackName --format '{{.Spec.TaskTemplate.ContainerSpec.Image}}' | cut -d'@' -f1 | rev | cut -d':' -f1 | rev"

if (-not $Version) {
    Write-Host "❌ No se pudo determinar la versión en TEST. Abortando..." -ForegroundColor Red
    exit 1
}

$Version = $Version.Trim()
Write-Host "🆕 Versión obtenida: $Version" -ForegroundColor Cyan

$testImage = "$testRegistry/${imageName}:$Version"
$fullImage = "$AcrName/${imageName}:$Version"

Write-Host "1. Descargando imagen de TEST..." -ForegroundColor Green
docker pull $testImage
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error al realizar pull desde TEST..."-ForegroundColor Red
    exit 1
}

Write-Host "2. Reetiquetando imagen para PROD..." -ForegroundColor Yellow
docker tag $testImage $fullImage

Write-Host "3. Subiendo imagen al registry de PROD..." -ForegroundColor Green
docker push $fullImage
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error al realizar push hacia registry PROD..."-ForegroundColor Red
    exit 1
}

Write-Host "4. Verificando imagen en PROD..." -ForegroundColor Yellow
docker pull $fullImage
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error al realizar pull desde registry PROD..."-ForegroundColor Red
    exit 1
}

Write-Host "🆗 Promoción completada." -ForegroundColor Cyan

$respuesta = Read-Host "¿Es primera publicación en ambiente prodcutivo de esta imagen? (S/N)"
if ($respuesta -eq "S" -or $respuesta -eq "s") {
Write-Host "=======================================================" -ForegroundColor Yellow 
Write-Host "✔️  Comando Generando para CREAR SERVICIO en production" -ForegroundColor Yellow
Write-Host "=======================================================" -ForegroundColor Yellow
Write-Host "======================================================="
Write-Host "docker service rm $StackName 2>/dev/null; \"
Write-Host "docker pull $fullImage && \"
Write-Host "docker service create --name $StackName \"
Write-Host "--replicas $Replicate \"
Write-Host "--network docker-cresa-attachable \"
Write-Host "--restart-condition any \"
Write-Host "--restart-delay 5s \"
Write-Host "--restart-max-attempts 3 \"
Write-Host "--update-parallelism 1 \"
Write-Host "--update-delay 10s \"
Write-Host "--update-order start-first \"
Write-Host "-e ASPNETCORE_ENVIRONMENT=$EnvName \"
Write-Host "$fullImage"

Write-Host "============================================================"
}
elseif ($respuesta -eq "N" -or $respuesta -eq "n") {
Write-Host "============================================================" -ForegroundColor Yellow 
Write-Host "✔️  Comando Generando para ACTUALIZAR SERVICIO en production" -ForegroundColor Yellow
Write-Host "============================================================" -ForegroundColor Yellow
Write-Host "============================================================"
Write-Host "docker pull $fullImage && \ "
Write-Host "docker service update \ "
Write-Host "--image $fullImage \ "
Write-Host "--update-parallelism 1 \ "
Write-Host "--update-delay 10s \ "
Write-Host "--update-order start-first \ "
Write-Host "--env-add ASPNETCORE_ENVIRONMENT=$EnvName \ "
Write-Host "$StackName"
Write-Host "================================================="
} 
else{
 
Write-Host "Has seleccionado que NO o la respuesta no es válida." -ForegroundColor Red

}

