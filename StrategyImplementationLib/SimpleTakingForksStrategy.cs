namespace StrategyImplementationLib;

using ForkLib;
using StrategyContractLib;

public class SimpleTakingForksStrategy : ITakingForksStrategy
{
    public void TakeForksMove(IForkOwner forkOwner, Fork leftFork, Fork rightFork)
    {
        if (leftFork.Owner != forkOwner.GetName())
        {
            leftFork.Take(forkOwner);
        }

        if (rightFork.Owner != forkOwner.GetName())
        {
            rightFork.Take(forkOwner);
        }
    }
}
