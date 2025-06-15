using DistributedSystemsProject.Models;
using static DistributedSystemsProject.Logging.Logger;

namespace DistributedSystemsProject.Strategies;

public class MulticastStrategy(
    string id,
    string[] peers,
    int packetLossProbability = 0,
    int nodeBrokenProbability = 0)
    : PropagationStrategy(id, peers, packetLossProbability)
{
    private bool _broken;

    public override async Task StartAsync(string initialMessage)
    {
        if (_broken || Random.Shared.Next(0, 100) < nodeBrokenProbability)
        {
            Log(Id, $"SIMULATED_BROKEN_NODE;{Id};{Id};{initialMessage}");

            _broken = true;

            return;
        }

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

        foreach (var peer in Peers.OrderBy(_ => Guid.NewGuid()).Take(5))
        {
            await SendAsync(peer, msg);
        }
    }

    public override Task ReceiveMessage(PropagationMessage msg)
    {
        return StartAsync(msg.Payload);
    }
}