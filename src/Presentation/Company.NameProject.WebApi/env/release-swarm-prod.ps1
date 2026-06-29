# release-swarm-prod.ps1 — Promover imagen QA → Production
# Ajusta los parámetros según tu infraestructura.

.\release-swarm-base-prod `
    -UserShh   "usuario@servidor.corp" `
    -AcrName   "192.168.x.x:5000" `
    -ImageName "nameproject-service" `
    -StackName "nameproject-service" `
    -EnvName   "Production" `
    -Replicate "1"
