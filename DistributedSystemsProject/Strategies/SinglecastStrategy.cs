using DistributedSystemsProject.Models;
using static DistributedSystemsProject.Logging.Logger;

namespace DistributedSystemsProject.Strategies;

public class SinglecastStrategy(
    string nodeName,
    string id,
    string[] peers,
    int packetLossProbability = 0,
    int nodeBrokenProbability = 0)
    : PropagationStrategy(id, peers, packetLossProbability)
{
    public override async Task StartAsync(string initialMessage)
    {
        if (Random.Shared.Next(0, 100) < nodeBrokenProbability)
        {
            Log(Id, $"SIMULATED_BROKEN_NODE;{Id};{Id};{initialMessage}");

            return;
        }

        if (Received is false)
        {
            Log(Id, $"RECEIVED;{Id};{Id};{initialMessage}");
            Received = true;
        }

        var msg = new PropagationMessage
        {
            SenderId = Id,
            Payload = initialMessage
        };

        var chain = Peers
            .Append(nodeName)
            .OrderBy(
                name =>
                {
                    var parts = name.Split('-');
                    return int.TryParse(parts.Last(), out var num) ? num : int.MaxValue;
                })
            .ToList();
        var indexOfPeer = chain.IndexOf(nodeName);

        if (indexOfPeer + 1 < chain.Count)
        {
            var next = chain[indexOfPeer + 1];

            var newMsg = new PropagationMessage
            {
                SenderId = Id,
                Payload = msg.Payload
            };

            await SendAsync(next, newMsg);
        }
    }

    public override Task ReceiveMessage(PropagationMessage msg)
    {
        return StartAsync(msg.Payload);
    }
}