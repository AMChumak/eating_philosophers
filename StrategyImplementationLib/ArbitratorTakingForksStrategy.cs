namespace StrategyImplementationLib;

using System.Diagnostics.CodeAnalysis;
using ForkLib;
using StrategyContractLib;

public class ArbitratorTakingForksStrategy : ITakingForksStrategy
{
    private Fork? _firstFork;
    private Fork? _secondFork;
    private bool _connected;

    public required IArbitrator Arbitrator { get; set; }
    public required string OwnerName { get; init; }

    [SetsRequiredMembers]
    public ArbitratorTakingForksStrategy(IArbitrator arbitrator, string name)
    {
        _connected = false;
        Arbitrator = arbitrator;
        OwnerName = string.Intern(name);
    }

    public void TakeForksMove(IForkOwner forkOwner, Fork leftFork, Fork rightFork)
    {
        if (!_connected)
        {
            Arbitrator.WantForks(forkOwner.GetName(), TakeForksCommandImplementation);
            _connected = true;
        }
        else if (_firstFork != null &&
                 _firstFork.Owner != forkOwner.GetName())
        {
            _firstFork.Take(forkOwner);
        }
        else if (_secondFork != null &&
                 _secondFork.Owner != forkOwner.GetName())
        {
            _secondFork.Take(forkOwner);
        }

        if (leftFork.Owner == OwnerName &&
            rightFork.Owner == OwnerName)
        {
            _firstFork = null;
            _secondFork = null;
            Arbitrator.UnwantForks(forkOwner.GetName(), TakeForksCommandImplementation);
            _connected = false;
        }
    }

    private void TakeForksCommandImplementation(string name, Fork firstFork, Fork secondFork)
    {
        if (name == OwnerName)
        {
            _firstFork = firstFork;
            _secondFork = secondFork;
        }
    }
}
