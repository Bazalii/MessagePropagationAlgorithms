using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using DistributedSystemsProject.Models;
using static DistributedSystemsProject.Logging.Logger;

namespace DistributedSystemsProject.Strategies;

public abstract class PropagationStrategy(
    string id,
    string[] peers,
    int packetLossProbability = 0)
{
    protected readonly string Id = id;
    protected readonly string[] Peers = peers;
    protected bool Received;

    public abstract Task StartAsync(string initialMessage);
    public abstract Task ReceiveMessage(PropagationMessage msg);

    protected async Task SendAsync(string peer, PropagationMessage msg)
    {
        if (Random.Shared.Next(0, 100) < packetLossProbability)
        {
            Log(Id, $"SIMULATED_DROP;{Id};{peer};{msg.Payload}");

            return;
        }

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(peer, 9000);
            var stream = client.GetStream();
            var json = JsonSerializer.Serialize(msg);
            var data = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(data);

            Log(Id, $"SENT;{Id};{peer};{msg.Payload}");
        }
        catch (Exception ex)
        {
            Log(Id, $"SEND_FAIL;{Id};{peer};{ex.Message}");
        }
    }
}