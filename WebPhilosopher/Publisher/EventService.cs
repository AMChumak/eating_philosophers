using System.Threading.Tasks;
using NetworkContracts;
using System;
namespace WebPhilosopher;

public class EventService
{
    private readonly IPublisher publisher;

    public int Seat = 0;

    public EventService(IPublisher publisher)
    {
        this.publisher = publisher;
    }

    public async Task<bool> WaitForPermissionAsync(TimeSpan timeout)
    {
        var request = new NoticeForksMessage
        {
            PhilosopherId = Seat
        };

        string responseQueueName = $"philosopher_{Seat}_responses";

        bool ResponseValidator(ForkPermissionResponse response)
        {
            return response.PhilosopherId == Seat &&
                   response.Granted;
        }

        return await publisher.SendAndWaitForResponseAsync<NoticeForksMessage, ForkPermissionResponse>(
            request,
            responseQueueName,
            ResponseValidator,
            timeout
        );
    }

    public void ReleaseForks(int forkId)
    {
        var message = new ForkReleasedMessage
        {
            ForkId = forkId
        };
        publisher.SendMessage(message);
    }

}