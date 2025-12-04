namespace PhilosopherLib;
using System.Diagnostics.CodeAnalysis;
using ForkLib;
using StrategyContractLib;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SimulationSettingsLib;

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

public class Philosopher : IPhilosopher
{
    private IOptions<SimulationSettings> _settings;
    private int _stateDuration;
    private readonly ITakingForksStrategy _takingForksStrategy;
    private TakingStatus _leftForkTakingStatus;
    private TakingStatus _rightForkTakingStatus;
    public required string Name { get; init; }
    public PhilosopherState State { get; private set; }
    public PhilosopherAction Action { get; private set; }
    public required IFork LeftFork { get; init; }
    public required IFork RightFork { get; init; }

    public int Score { get; private set; } = 0;

    public event PhilosopherChangedState? ChangedState;

    [SetsRequiredMembers]
    public Philosopher(string name, IFork leftFork, IFork rightFork, ITakingForksStrategy takingForksStrategy, IOptions<SimulationSettings> settings)
    {
        _settings = settings;
        State = PhilosopherState.Thinking;
        _stateDuration = Random.Shared.Next(30, 100);
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
            Action = PhilosopherAction.ReleaseForks;
        }
        else if (fork.Equals(RightFork))
        {
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
        State = PhilosopherState.Hungry;
        Action = PhilosopherAction.None;
        _stateDuration = 0;
}

    private void ChangeStateAfterHungry()
    {
        Action = PhilosopherAction.None;
        State = PhilosopherState.Eating;
        _stateDuration = Random.Shared.Next(_settings.Value.EatingTimeMinMs, _settings.Value.EatingTimeMaxMs);
    }

    private void ChangeStateAfterEating()
    {
        Score++;
        LeftFork.Release(this);
        RightFork.Release(this);
        Action = PhilosopherAction.ReleaseForks;
        State = PhilosopherState.Thinking;
        _stateDuration = Random.Shared.Next(_settings.Value.ThinkingTimeMinMs, _settings.Value.EatingTimeMaxMs);
    }

    private async Task Move(CancellationToken token)
    {
        ChangedState?.Invoke(this, State);
        await Task.Delay(_stateDuration, token);

        if (State == PhilosopherState.Hungry)
        {
            _takingForksStrategy.TakeForksMove(this, LeftFork, RightFork);
        }

        UpdateState();
    }

    public async Task Live(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                await Move(token);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            LeftFork.Release(this);
            RightFork.Release(this);
        }
    }
}
