namespace Publisher;

public class RabbitMQSettings
{
    public required string HostName { get; set; }
    public int Port { get; set; }
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public required string VirtualHost { get; set; }
    public required string ExchangeName { get; set; }
    public required string QueueName { get; set; }
}