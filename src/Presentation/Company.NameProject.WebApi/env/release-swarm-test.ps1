.\release-swarm-base -UserShh  "david.almeida@crecos.corp" `
                     -AcrName  "192.168.82.174:5000" `
                     -ImageName  "gmv-service-img" `
                     -StackName  "gmv-service" `
                     -SwarmManager  "srvdockercer1" `
                     -EnvName "QA" `
                     -Replicate 3