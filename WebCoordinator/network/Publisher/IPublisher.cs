using System;
using System.Threading.Tasks;

namespace Publisher;

public interface IPublisher : IDisposable
{
    void SendMessage<T>(T message, string queueName) where T : class;
    Task StartListening(string queueName, Func<string, Task> messageHandler);
    void StopListening(string queueName);
}