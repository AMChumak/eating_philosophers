using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NetworkContracts;
using Publisher;
namespace WebCoordinator;

public class MessageProcessor:IHostedService, IDisposable
{
    private const string COORDINATOR_QUEUE = "coordinator_queue";

    private Coordinator coordinator;
    private EventHandler eventHandler;
    private IPublisher publisher;

    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private bool disposed = false;

    public MessageProcessor(IPublisher publisher)
    {
        this.publisher = publisher;
        Table table = new Table();
        coordinator = new Coordinator(publisher, table);
        eventHandler = new EventHandler(coordinator, table);
    }

    public async Task Start()
    {
        Console.WriteLine("MessageProcessor started listening for messages from coordinator_queue...");

        await publisher.StartListening(COORDINATOR_QUEUE, HandleMessage);

        Console.WriteLine("MessageProcessor is now actively listening for messages...");

        await Task.Delay(10000, cancellationTokenSource.Token);
    }

    public void Stop()
    {
        cancellationTokenSource.Cancel();
        publisher.StopListening(COORDINATOR_QUEUE);
    }

    private async Task HandleMessage(string messageJson)
    {
        ProcessMessage(messageJson);
    }

    private void ProcessMessage(string messageJson)
    {
        try
        {
            using var jsonDoc = JsonDocument.Parse(messageJson);
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("messageType", out var messageTypeElement))
            {
                var messageType = messageTypeElement.GetString();

                switch (messageType)
                {
                    case "NoticeFork":
                        ProcessNoticeForkMessage(messageJson);
                        break;

                    case "ForkReleased":
                        ProcessForkReleasedMessage(messageJson);
                        break;

                    default:
                        Console.WriteLine($"Unknown message type: {messageType}");
                        break;
                }
            }
            else
            {
                Console.WriteLine($"Message without messageType property: {messageJson}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing message JSON: {ex.Message}");
        }
    }

    private void ProcessNoticeForkMessage(string messageJson)
    {
        try
        {
            var noticeMessage = JsonSerializer.Deserialize<NoticeForksMessage>(messageJson);
            if (noticeMessage != null)
            {
                eventHandler.ProccessNoticeFork(noticeMessage.PhilosopherId);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing NoticeFork message: {ex.Message}");
        }
    }

    private void ProcessForkReleasedMessage(string messageJson)
    {
        try
        {
            var releaseMessage = JsonSerializer.Deserialize<ForkReleasedMessage>(messageJson);
            if (releaseMessage != null)
            {
                eventHandler.ProcessRealiseFork(releaseMessage.ForkId);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing ForkReleased message: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (!disposed)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            publisher?.Dispose();
            disposed = true;
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            await Start();
            Stop();
        });
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}