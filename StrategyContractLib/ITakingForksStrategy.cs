namespace StrategyContractLib;

using ForkLib;

public interface ITakingForksStrategy
{
    void TakeForksMove(IForkOwner forkOwner, IFork leftFork, IFork rightFork);
}
