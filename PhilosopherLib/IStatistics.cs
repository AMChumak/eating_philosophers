using ForkLib;

namespace PhilosopherLib;

public interface IStatistics
{
    Task Overwatch(CancellationToken token);
    void PrintStatistics();

    void AddPhilosopher(Philosopher philosopher);
    void OnPhilosopherChangedStatus(Philosopher philosopher, PhilosopherState state);
    void OnForkOwnerChanged(Fork fork, IForkOwner? owner);
}
