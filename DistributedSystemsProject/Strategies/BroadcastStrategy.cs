using DistributedSystemsProject.Models;
using static DistributedSystemsProject.Logging.Logger;

namespace DistributedSystemsProject.Strategies;

public class BroadcastStrategy(
    string id,
    string[] peers,
    int packetLossProbability = 0,
    int nodeBrokenProbability = 0)
    : PropagationStrategy(id, peers, packetLossProbability)
{
    public override async Task StartAsync(string initialMessage)
    {
        if (Received)
        {
            return;
        }

        Log(Id, $"RECEIVED;{Id};{Id};{initialMessage}");
        Received = true;

        var msg = new PropagationMessage
        {
            SenderId = Id,
            Payload = initialMessage
        };

        var sendTasks = Peers.Select(peer => SendAsync(peer, msg));

        await Task.WhenAll(sendTasks);
    }

    public override Task ReceiveMessage(PropagationMessage msg)
    {
        if (Random.Shared.Next(0, 100) < nodeBrokenProbability)
        {
            Log(Id, $"SIMULATED_BROKEN_NODE;{Id};{Id};{msg.Payload}");

            return Task.CompletedTask;
        }

        Log(Id, $"RECEIVED;{Id};{Id};{msg.Payload}");
        Received = true;

        return Task.CompletedTask;
    }
}