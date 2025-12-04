namespace StrategyImplementationLib;

using ForkLib;
using StrategyContractLib;

public class SourceHierarchyTakingForksStrategy : ITakingForksStrategy
{
    public void TakeForksMove(IForkOwner forkOwner, IFork leftFork, IFork rightFork)
    {
        IFork firstFork = leftFork.OrderNumber < rightFork.OrderNumber ? leftFork : rightFork;
        IFork secondFork = leftFork.OrderNumber > rightFork.OrderNumber ? leftFork : rightFork;


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
