using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using DistributedSystemsProject.Models;
using DistributedSystemsProject.Strategies;
using Docker.DotNet;
using Docker.DotNet.Models;

var nodeId = Environment.GetEnvironmentVariable("NODE_ID") ?? Guid.NewGuid().ToString();
var strategyName = Environment.GetEnvironmentVariable("STRATEGY") ?? "gossip";
var isStarter = Environment.GetEnvironmentVariable("IS_STARTER") == "true";
var packetLossProbability = int.Parse(Environment.GetEnvironmentVariable("PACKET_LOSS_PROBABILITY")!);
var nodeBrokenProbability = int.Parse(Environment.GetEnvironmentVariable("NODE_BROKEN_PROBABILITY")!);

Console.WriteLine($"[{nodeId}] Starting with strategy: {strategyName}");

var hostname = Dns.GetHostName();

var client = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
var containers = await client.Containers.ListContainersAsync(new ContainersListParameters { All = true });

var current = containers.FirstOrDefault(c => c.ID.StartsWith(hostname));
var currentNodeName = current?.Names?.First()?.TrimStart('/')!;

Console.WriteLine($"Node name: {currentNodeName}");
var peers = await GetPeerContainersAsync(currentNodeName);

PropagationStrategy strategy = strategyName switch
{
    "gossip" => new GossipStrategy(nodeId, peers, packetLossProbability, nodeBrokenProbability),
    "broadcast" => new BroadcastStrategy(nodeId, peers, packetLossProbability, nodeBrokenProbability),
    "multicast" => new MulticastStrategy(nodeId, peers, packetLossProbability, nodeBrokenProbability),
    "singlecast" => new SinglecastStrategy(
        currentNodeName, nodeId, peers, packetLossProbability, nodeBrokenProbability),
    _ => throw new Exception("Unknown strategy")
};

var receiveLock = new ReaderWriterLockSlim();

if (isStarter)
{
    await strategy.StartAsync($"INIT_FROM_{nodeId}");
}

_ = Task.Run(() => ListenAsync(strategy));

await Task.Delay(Timeout.Infinite);

async Task ListenAsync(PropagationStrategy strategy)
{
    var listener = new TcpListener(IPAddress.Any, 9000);
    listener.Start();
    Console.WriteLine($"[{nodeId}] Listening on port 9000");

    while (true)
    {
        var client = await listener.AcceptTcpClientAsync();
        _ = Task.Run(
            async () =>
            {
                await using var stream = client.GetStream();
                var buffer = new byte[1024];
                var read = await stream.ReadAsync(buffer);
                var payload = Encoding.UTF8.GetString(buffer, 0, read);
                var msg = JsonSerializer.Deserialize<PropagationMessage>(payload);
                try
                {
                    receiveLock.EnterWriteLock();
                    await strategy.ReceiveMessage(msg);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                finally
                {
                    receiveLock.ExitWriteLock();
                }
            });
    }
}

async Task<string[]> GetPeerContainersAsync(string currentName)
{
    return containers
        .Where(
            response => response.Names.Any(n => n.Contains("node")) &&
                        response.Names.Any(n => n.Contains(currentName) is false))
        .Select(c => c.Names.First().TrimStart('/'))
        .ToArray();
}