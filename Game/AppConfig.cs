namespace Game;

public static class AppConfig
{
    public const string ServiceName = "Microservices.Game.API";
    public static readonly string InstanceUuid = Guid.NewGuid().ToString();
    public static readonly string InstanceName = AppDomain.CurrentDomain.FriendlyName;
    public static readonly string HostName = Environment.MachineName;

    public static void ShowApplicationInfo()
    {
        Console.WriteLine("\n====================================================================");
        Console.WriteLine($"[ServiceName] {ServiceName}");
        Console.WriteLine($"[InstanceUuid] {InstanceUuid}");
        Console.WriteLine($"[InstanceName] {InstanceName}");
        Console.WriteLine($"[HostName] {HostName}");
        Console.WriteLine("====================================================================\n");
    }
}
