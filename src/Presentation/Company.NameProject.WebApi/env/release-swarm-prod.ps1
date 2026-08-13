.\release-swarm-base-prod -UserShh  "david.almeida@crecos.corp" `
                          -AcrName  "192.168.81.155:5000" `
                          -ImageName  "gmv-service-img-qa" `
                          -StackName  "gmv-service" `
                          -SwarmManager  "srvdockerpro1" `
                          -EnvName "Production" `
                          -Replicate 1