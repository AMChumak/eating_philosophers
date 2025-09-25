namespace StrategyContractLib;

using ForkLib;

public interface ITakingForksStrategy
{
    void TakeForksMove(IForkOwner forkOwner, Fork leftFork, Fork rightFork);
}
