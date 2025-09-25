namespace PhilosopherLib;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Cryptography;
using ForkLib;
using StrategyContractLib;

public enum PhilosopherState
{
    Thinking,
    Hungry,
    Eating
}

public static class PhilosopherStateExtensions
{
    public static string ToString(this PhilosopherState state)
    {
        return state switch
        {
            PhilosopherState.Thinking => "Thinking",
            PhilosopherState.Hungry => "Hungry",
            PhilosopherState.Eating => "Eating",
            _ => ""
        };
    }
}

public enum PhilosopherAction
{
    TakingLeftFork,
    TakingRightFork,
    ReleaseForks,
    None
}

public static class PhilosopherActionExtensions
{
    public static string ToString(this PhilosopherAction state)
    {
        return state switch
        {
            PhilosopherAction.TakingLeftFork => "Takes left fork",
            PhilosopherAction.TakingRightFork => "Takes right fork",
            PhilosopherAction.ReleaseForks => "Release forks",
            PhilosopherAction.None => "None",
            _ => ""
        };
    }
}

public class Philosopher : IForkOwner
{
    private int _stateDuration;
    private readonly ITakingForksStrategy _takingForksStrategy;
    private TakingStatus _leftForkTakingStatus;
    private TakingStatus _rightForkTakingStatus;

    public required string Name { get; init; }
    public PhilosopherState State { get; private set; }

    public int InStateDuration { get; private set; }
    public PhilosopherAction Action { get; private set; }
    public required Fork LeftFork { get; init; }
    public required Fork RightFork { get; init; }

    public int Score { get; private set; } = 0;


    [SetsRequiredMembers]
    public Philosopher(string name, Fork leftFork, Fork rightFork, ITakingForksStrategy takingForksStrategy)
    {
        State = PhilosopherState.Thinking;
        _stateDuration = Random.Shared.Next(3, 10);
        Action = PhilosopherAction.None;
        _takingForksStrategy = takingForksStrategy;
        Name = name;
        LeftFork = leftFork;
        RightFork = rightFork;
    }

    public string GetName()
    {
        return Name;
    }

    public void SetTakingStatus(Fork fork, TakingStatus status)
    {
        if (fork.Equals(LeftFork))
        {
            _leftForkTakingStatus = status;

            if (status == TakingStatus.Completed)
            {
                Action = PhilosopherAction.TakingLeftFork;
                _leftForkTakingStatus = TakingStatus.Inaction;
            }
        }
        else if (fork.Equals(RightFork))
        {
            _rightForkTakingStatus = status;

            if (status == TakingStatus.Completed)
            {
                Action = PhilosopherAction.TakingRightFork;
                _rightForkTakingStatus = TakingStatus.Inaction;
            }
        }
    }

    public TakingStatus GetTakingStatus(Fork fork)
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

    public void ReleaseFork(Fork fork)
    {
        if (fork.Equals(LeftFork))
        {
            _leftForkTakingStatus = TakingStatus.Inaction;
            Action = PhilosopherAction.ReleaseForks;
        }
        else if (fork.Equals(RightFork))
        {
            _rightForkTakingStatus = TakingStatus.Inaction;
            Action = PhilosopherAction.ReleaseForks;
        }
    }

    public void UpdateState()
    {
        switch (State)
        {
            case PhilosopherState.Thinking:
                ChangeStateAfterThinking();
                break;
            case PhilosopherState.Hungry:
                ChangeStateAfterHungry();
                break;
            case PhilosopherState.Eating:
                ChangeStateAfterEating();
                break;
            default:
                break;
        }
    }

    private void ChangeStateAfterThinking()
    {
        if (InStateDuration == _stateDuration)
        {
            State = PhilosopherState.Hungry;
            Action = PhilosopherAction.None;
            _stateDuration = 0;
            InStateDuration = 0;
        }
        else
        {
            Action = PhilosopherAction.None;
            InStateDuration++;
        }
    }

    private void ChangeStateAfterHungry()
    {
        if (LeftFork.Owner == Name &&
            RightFork.Owner == Name)
        {
            Action = PhilosopherAction.None;
            State = PhilosopherState.Eating;
            _stateDuration = Random.Shared.Next(4, 5);
            InStateDuration = 0;
        }
        else
        {
            Action = PhilosopherAction.None;
            InStateDuration++;
        }
    }

    private void ChangeStateAfterEating()
    {
        if (InStateDuration == _stateDuration)
        {
            LeftFork.Release(this);
            RightFork.Release(this);
            Action = PhilosopherAction.ReleaseForks;
            State = PhilosopherState.Thinking;
            _stateDuration = Random.Shared.Next(3, 10);
            InStateDuration = 0;
            Score++;
        }
        else
        {
            Action = PhilosopherAction.None;
            InStateDuration++;
        }
    }

    public void Move()
    {
        if (State == PhilosopherState.Hungry)
        {
            _takingForksStrategy.TakeForksMove(this, LeftFork, RightFork);
        }
    }
}
