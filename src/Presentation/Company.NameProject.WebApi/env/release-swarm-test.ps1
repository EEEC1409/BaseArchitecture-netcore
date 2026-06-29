# release-swarm-test.ps1 — Promover imagen de Development → QA (Test)
# Ajusta los parámetros según tu infraestructura.

.\release-swarm-base `
    -UserShh      "usuario@servidor.corp" `
    -AcrName      "192.168.x.x:5000" `
    -ImageName    "nameproject-service" `
    -StackName    "nameproject-service" `
    -SwarmManager "srvdockerqa1" `
    -EnvName      "QA" `
    -Replicate    1
