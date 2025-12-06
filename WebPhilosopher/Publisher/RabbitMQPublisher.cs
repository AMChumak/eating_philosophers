namespace WebPhilosopher;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


public class RabbitMQPublisher : IPublisher, IDisposable
{
    private readonly IConnection connection;
    private readonly IChannel channel;
    private readonly string queueName;

    public RabbitMQPublisher(string queueName = "coordinator_queue")
    {
        var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
        var rabbitMqUser = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest";
        var rabbitMqPass = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest";
        var rabbitMqPort = Environment.GetEnvironmentVariable("RABBITMQ_PORT");
        var port = 5672;
        if (!string.IsNullOrEmpty(rabbitMqPort) && int.TryParse(rabbitMqPort, out int parsedPort))
            port = parsedPort;

        var factory = new ConnectionFactory()
        {
            HostName = rabbitMqHost,
            UserName = rabbitMqUser,
            Password = rabbitMqPass,
            Port = port
        };

        connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        channel = connection.CreateChannelAsync().GetAwaiter().GetResult();
        this.queueName = queueName;

        channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        Console.WriteLine($"RabbitMQ Publisher подключен к очереди '{queueName}'");
    }

    public void SendMessage<T>(T message) where T : class
    {
        try
        {
            var jsonString = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(jsonString);

            channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                body: body
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Ошибка отправки сообщения в RabbitMQ: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> SendAndWaitForResponseAsync<TRequest, TResponse>(
        TRequest request,
        string responseQueueName,
        Func<TResponse, bool> responseValidator,
        TimeSpan timeout)
        where TRequest : class
        where TResponse : class
    {
        var tcs = new TaskCompletionSource<bool>();
        var correlationId = Guid.NewGuid().ToString();

        await channel.QueueDeclareAsync(
            queue: responseQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );


        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var responseJson = Encoding.UTF8.GetString(ea.Body.ToArray());
                var response = JsonSerializer.Deserialize<TResponse>(responseJson);

                if (response != null && responseValidator(response))
                {
                    tcs.TrySetResult(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing response: {ex.Message}");
                tcs.TrySetResult(false);
            }
        };

        var consumerTag = await channel.BasicConsumeAsync(
            queue: responseQueueName,
            autoAck: true,
            consumer: consumer);


        SendMessage(request);

        var timeoutTask = Task.Delay(timeout);
        var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

        await channel.BasicCancelAsync(consumerTag);

        if (completedTask == tcs.Task)
        {
            return await tcs.Task;
        }
        else
        {
            Console.WriteLine("Timeout waiting for response");
            return false;
        }
    }

    public void Dispose()
    {
        channel?.CloseAsync().GetAwaiter().GetResult();
        connection?.CloseAsync().GetAwaiter().GetResult();

        channel?.Dispose();
        connection?.Dispose();
    }
}