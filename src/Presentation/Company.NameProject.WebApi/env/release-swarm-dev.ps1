# release-swarm-dev.ps1 — Deploy a entorno Development
# Ajusta los parámetros según tu infraestructura.

.\release-swarm-base `
    -UserShh      "usuario@servidor.corp" `
    -AcrName      "192.168.x.x:5000" `
    -ImageName    "nameproject-service" `
    -StackName    "nameproject-service" `
    -SwarmManager "srvdockerdev1" `
    -EnvName      "Development" `
    -Replicate    1
