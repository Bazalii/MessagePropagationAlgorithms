# Message propagation in distributed systems

Project simulates distributed system with nodes that propagate messages using `gossip`, `broadcast`, `multicast` and `singlecast` strategies.
One run == one experiment == one message propagation round == one strategy.

How to run:
- Choose propagation strategy by using STRATEGY env variable in docker-compose. For example: `STRATEGY=multicast`
- Choose probability that node is broken by using PACKET_LOSS_PROBABILITY env variable in docker compose. For example: `PACKET_LOSS_PROBABILITY=0`
- Choose probability that node will lose packages by using NODE_BROKEN_PROBABILITY env variable in docker compose. For example: `NODE_BROKEN_PROBABILITY=0`
- Map logs folder inside container to one on your machine using volumes. For example: `./logs/multicast-healthy:/logs`
- Build image: `docker compose build`
- Run nodes: `docker compose up --scale node=101 --detach`
- Start message propagation. For example for multicast:
  - `docker exec -it distibutedsystemsproject-node-1 \
    env IS_STARTER=true STRATEGY=broadcast \
    dotnet DistributedSystemsProject.dll`
- Choose logs folder inside python script. For example: `log_dir = "../logs/multicast-healthy"`
- Run python graphs.py script to get graph of message propagation
- If you want to stop the containers and run new experiment use command: `docker compose down --remove-orphans` and repeat all the steps above