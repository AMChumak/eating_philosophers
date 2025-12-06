using System;
using System.Threading.Tasks;
using NetworkContracts;

namespace WebPhilosopher;

public interface IPublisher
{
    public void SendMessage<T>(T message) where T : class;

    Task<bool> SendAndWaitForResponseAsync<TRequest, TResponse>(
                                    TRequest request,
                                    string responseQueueName,
                                    Func<TResponse, bool> responseValidator,
                                    TimeSpan timeout)
                                    where TRequest : class
                                    where TResponse : class;
}