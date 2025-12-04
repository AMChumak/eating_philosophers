namespace PhilosopherLib;

using ForkLib;

public delegate void PhilosopherChangedState(IPhilosopher philosopher, PhilosopherState state);
public interface IPhilosopher: IForkOwner
{
    public string Name { get; }
    public PhilosopherState State { get; }
    public PhilosopherAction Action { get; }
    public IFork LeftFork { get; init; }
    public IFork RightFork { get; init; }
    public int Score { get;}
    public event PhilosopherChangedState? ChangedState;
}