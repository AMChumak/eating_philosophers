using ForkLib;

namespace PhilosopherLib;

public interface IStatistics
{
    Task Overwatch(CancellationToken token);
    void PrintStatistics();

    void AddPhilosopher(IPhilosopher philosopher);
    void OnPhilosopherChangedStatus(IPhilosopher philosopher, PhilosopherState state);
    void OnForkOwnerChanged(IFork fork, IForkOwner? owner);
}
