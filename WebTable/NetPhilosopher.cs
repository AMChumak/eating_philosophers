namespace WebTable;

using PhilosopherLib;
using ForkLib;


public class NetPhilosopher : IPhilosopher
{
    private TakingStatus _leftForkTakingStatus;
    private TakingStatus _rightForkTakingStatus;
    public string Name {get; private set;}
    public PhilosopherState State {get; set;}
    public PhilosopherAction Action {get; set;}
    public IFork LeftFork { get; init; }
    public IFork RightFork { get; init; }
    public int Score {get; private set; } = 0;
    public event PhilosopherChangedState? ChangedState;


    public NetPhilosopher (string name, IFork leftFork, IFork rightFork)
    {
        LeftFork = leftFork;
        RightFork = rightFork;
        Name = name;
        State = PhilosopherState.Thinking;
        Action = PhilosopherAction.None;
    }

    public string GetName()
    {
        return Name;
    }

    public void SetTakingStatus(IFork fork, TakingStatus status)
    {
        if (fork.Equals(LeftFork))
        {
            _leftForkTakingStatus = status;

            if (status == TakingStatus.InProgress)
            {
                Action = PhilosopherAction.TakingLeftFork;
            }
        }
        else if (fork.Equals(RightFork))
        {
            _rightForkTakingStatus = status;

            if (status == TakingStatus.InProgress)
            {
                Action = PhilosopherAction.TakingRightFork;
            }
        }
    }

    public TakingStatus GetTakingStatus(IFork fork)
    {
        if (fork.Equals(LeftFork))
        {
            return _leftForkTakingStatus;
        }
        else if (fork.Equals(RightFork))
        {
            return _rightForkTakingStatus;
        }

        return TakingStatus.Inaction;
    }

    public void ReleaseFork(IFork fork)
    {
        if (fork.Equals(LeftFork))
        {
            LeftFork.Release(this);
            Action = PhilosopherAction.ReleaseForks;
        }
        else if (fork.Equals(RightFork))
        {
            RightFork.Release(this);
            Action = PhilosopherAction.ReleaseForks;
        }
    }

    public void ReleaseFork(int fork)
    {
        if (fork.Equals(LeftFork.OrderNumber))
        {
            LeftFork.Release(this);
            Action = PhilosopherAction.ReleaseForks;
        }
        else if (fork.Equals(RightFork))
        {
            RightFork.Release(this);
            Action = PhilosopherAction.ReleaseForks;
        }
    }

    public void UpdateState(PhilosopherState newState)
    {
        if (State == PhilosopherState.Eating)
        {
            Score++;
        }

        State = newState;
        ChangedState?.Invoke(this, newState);
    }

    public bool TakeFork(IFork fork)
    {
        if (fork.Equals(LeftFork))
        {
            return LeftFork.Take(this);
        }

        if (fork.Equals(RightFork))
        {
            return RightFork.Take(this);
        }

        return false;
    }

    public bool TakeFork(int fork)
    {
        if (fork.Equals(LeftFork.OrderNumber))
        {
            return LeftFork.Take(this);
        }

        if (fork.Equals(RightFork.OrderNumber))
        {
            return RightFork.Take(this);
        }

        return false;
    }
}