namespace StrategyImplementationLib;

using ForkLib;
using StrategyContractLib;

public class SourceHierarchyTakingForksStrategy : ITakingForksStrategy
{
    public void TakeForksMove(IForkOwner forkOwner, Fork leftFork, Fork rightFork)
    {
        Fork firstFork = leftFork.OrderNumber < rightFork.OrderNumber ? leftFork : rightFork;
        Fork secondFork = leftFork.OrderNumber > rightFork.OrderNumber ? leftFork : rightFork;


        if (firstFork.Owner != forkOwner.GetName())
        {
            firstFork.Take(forkOwner);
        }

        if (secondFork.Owner != forkOwner.GetName())
        {
            secondFork.Take(forkOwner);
        }
    }
}
