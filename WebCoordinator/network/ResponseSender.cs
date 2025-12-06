
namespace WebCoordinator;

using NetworkContracts;
using Publisher;

public class ResponseSender
{
    IPublisher publisher;
    public ResponseSender(IPublisher publisher)
    {
        this.publisher = publisher;
    }

    public void SendForkPerm(int philId)
    {
        var response = new ForkPermissionResponse
        {
            PhilosopherId = philId,
            Granted = true,
            Timestamp = DateTime.UtcNow,
            MessageType = "ForkPermissionResponse"
        };

        string responseQueue = $"philosopher_{philId}_responses";
        publisher.SendMessage(response, responseQueue);
    }
}