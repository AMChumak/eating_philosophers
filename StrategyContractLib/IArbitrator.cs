namespace StrategyContractLib;

using ForkLib;

public delegate void TakeForksCommand(string name, Fork firstFork, Fork secondFork);
public interface IArbitrator
{
    void WantForks(string name, TakeForksCommand handler);
    void UnwantForks(string name, TakeForksCommand handler);
    void Manage();
}
