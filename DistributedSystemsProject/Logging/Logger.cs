namespace DistributedSystemsProject.Logging;

public static class Logger
{
    public static void Log(
        string id,
        string line)
    {
        var timestamp = DateTime.UtcNow.ToString("O");
        var formatted = $"{timestamp};{line}";
        Console.WriteLine(formatted);

        try
        {
            File.AppendAllText($"/logs/{id}.log", formatted + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to write log: {ex.Message}");
        }
    }
}