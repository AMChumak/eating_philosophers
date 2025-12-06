using System;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Publisher;

public class RabbitMQPublisher : IPublisher
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ConcurrentDictionary<string, string> _activeConsumers;
    private bool disposed = false;

    public RabbitMQPublisher(IOptions<RabbitMQSettings> options)
    {
        var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
        var rabbitMqUser = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest";
        var rabbitMqPass = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest";
        var rabbitMqPort = Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672";

        var factory = new ConnectionFactory()
        {
            HostName = rabbitMqHost,
            UserName = rabbitMqUser,
            Password = rabbitMqPass,
            Port = int.Parse(rabbitMqPort)
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        _activeConsumers = new ConcurrentDictionary<string, string>();

        Console.WriteLine("RabbitMQ Publisher initialized");
    }

    public async void SendMessage<T>(T message, string queueName) where T : class
    {
        if (disposed) throw new ObjectDisposedException(nameof(RabbitMQPublisher));

        try
        {
            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var jsonString = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(jsonString);

            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                body: body
            );

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message to queue '{queueName}': {ex.Message}");
            throw;
        }
    }

    public async Task StartListening(string queueName, Func<string, Task> messageHandler)
    {
        if (disposed) throw new ObjectDisposedException(nameof(RabbitMQPublisher));
        if (_activeConsumers.ContainsKey(queueName))
        {
            Console.WriteLine($"Already listening to queue '{queueName}'");
            return;
        }

        try
        {
            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var messageJson = Encoding.UTF8.GetString(ea.Body.ToArray());

                    await messageHandler(messageJson);

                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message from queue '{queueName}': {ex.Message}");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };

            var consumerTag = await _channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer
            );

            _activeConsumers[queueName] = consumerTag;
            Console.WriteLine($"Started listening to queue '{queueName}' with consumer tag '{consumerTag}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting listener for queue '{queueName}': {ex.Message}");
            throw;
        }
    }

    public void StopListening(string queueName)
    {
        if (_activeConsumers.TryRemove(queueName, out var consumerTag))
        {
            try
            {
                _channel.BasicCancelAsync(consumerTag);
                Console.WriteLine($"Stopped listening to queue '{queueName}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping listener for queue '{queueName}': {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        if (!disposed)
        {
            foreach (var queueName in _activeConsumers.Keys)
            {
                StopListening(queueName);
            }

            _channel?.CloseAsync().GetAwaiter().GetResult();
            _connection?.CloseAsync().GetAwaiter().GetResult();
            _channel?.Dispose();
            _connection?.Dispose();
            disposed = true;
            Console.WriteLine("RabbitMQ Publisher disposed");
        }
    }
}