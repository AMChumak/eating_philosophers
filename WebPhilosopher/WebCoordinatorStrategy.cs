namespace WebPhilosopher;

using ForkLib;
using StrategyContractLib;


public class WebCoordinatorStrategy : ITakingForksStrategy
{
    private EventService _eventService;
    public WebCoordinatorStrategy(EventService eventService)
    {
        _eventService = eventService;
    }
    public void TakeForksMove(IForkOwner forkOwner, IFork leftFork, IFork rightFork)
    {
        var result = false;

        while (!result)
        {
            result = _eventService.WaitForPermissionAsync(TimeSpan.FromSeconds(2)).GetAwaiter().GetResult();
        }

        leftFork.Take(forkOwner);
        rightFork.Take(forkOwner);
    }
}