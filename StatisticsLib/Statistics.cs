using ForkLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhilosopherLib;
using SimulationSettingsLib;

namespace StatisticsLib;

public class Statistics : IStatistics
{
    private ILogger<Statistics> _logger;
    private readonly object _lockobj = new object();
    private IOptions<SimulationSettings> _settings;
    private DateTime _start;
    private List<Philosopher> _philosophers = [];
    private List<Fork> _forks = [];
    private List<DateTime> _lastStateStarts = [];
    private List<DateTime> _lastWaitingStarts = [];
    private List<TimeSpan> _waitingSums = [];
    private List<int> _waitingCounts = [];
    private List<DateTime> _lastEatingStarts = [];
    private List<DateTime> _forkFreeStarts = [];
    private List<TimeSpan> _forkFreeTimes = [];
    private List<TimeSpan> _forkEatingTimes = [];
    private List<(int, int)> _ownForks = [];

    public Statistics(ITableManager tableManager, ILogger<Statistics> logger, IOptions<SimulationSettings> settings)
    {
        _logger = logger;
        _settings = settings;

        _forks = tableManager.GetForks();
        _forkFreeStarts = Enumerable.Repeat(DateTime.MinValue, _forks.Count).ToList();
        _forkFreeTimes = Enumerable.Repeat(TimeSpan.Zero, _forks.Count).ToList();
        _forkEatingTimes = Enumerable.Repeat(TimeSpan.Zero, _forks.Count).ToList();

        foreach (var fork in _forks)
        {
            fork.OwnerChanged += OnForkOwnerChanged;
        }
    }

    private void PrintSystemState()
    {
        int countHalfOwners = 0;
        _logger.Log(LogLevel.Information, "---------------- TIME {0} ----------------", DateTime.Now);
        for (int i = 0; i < _philosophers.Count; ++i)
        {
            // Check potential deadlock
            if ((_philosophers[i].LeftFork.Owner == _philosophers[i].Name && _philosophers[i].RightFork.Owner != _philosophers[i].Name) ||
                (_philosophers[i].LeftFork.Owner != _philosophers[i].Name && _philosophers[i].RightFork.Owner == _philosophers[i].Name))
            {
                countHalfOwners++;
            }

            TimeSpan waiting = DateTime.Now - _lastStateStarts[i];
            TimeSpan spentTime = DateTime.Now - _start;
            _logger.Log(LogLevel.Information, "{0}: {1} (In state: {2} ms) [Current action: {3}] Throuhput: {4}", _philosophers[i].Name, _philosophers[i].State.ToString(), waiting.TotalMilliseconds, _philosophers[i].Action.ToString(), _philosophers[i].Score * 1000 / spentTime.TotalMilliseconds);
        }

        for (int i = 0; i < _forks.Count; ++i)
        {
            if (_forks[i].Owner != "")
            {
                _logger.Log(LogLevel.Information, "Fork {0}: In Use (Owner: {1}) ", i, _forks[i].Owner);
            }
            else
            {
                _logger.Log(LogLevel.Information, "Fork {0}: Available", i);
            }
        }

        if (countHalfOwners == _philosophers.Count)
            throw new Exception("Deadlock exception");
    }

    public async Task Overwatch(CancellationToken token)
    {
        _start = DateTime.Now;
        while (!token.IsCancellationRequested && (DateTime.Now - _start).TotalSeconds < _settings.Value.DurationSec)
        {
            await Task.Delay(_settings.Value.DisplayIntervalMs, token);
            PrintSystemState();
        }
    }

    public void PrintStatistics()
    {
        TimeSpan totalTime = TimeSpan.FromSeconds(_settings.Value.DurationSec);
        _logger.Log(LogLevel.Information, "---------------- STATISTICS ----------------");

        _logger.Log(LogLevel.Information, "\nPhilosophers:\n");

        for (int i = 0; i < _philosophers.Count; ++i)
        {
            _logger.Log(LogLevel.Information, "\t{0}: Throughput: {1}, Average waiting: {2} ms", _philosophers[i].Name, _philosophers[i].Score * 1000 / totalTime.TotalMilliseconds, _waitingSums[i].TotalMilliseconds / _waitingCounts[i]);
        }

        _logger.Log(LogLevel.Information, "\n\nForks:\n");

        for (int i = 0; i < _forks.Count; ++i)
        {
            double availablePercents = (double)_forkFreeTimes[i].TotalMilliseconds * 100 / totalTime.TotalMilliseconds;
            double inEatingPercents = (double)_forkEatingTimes[i].TotalMilliseconds * 100 / totalTime.TotalMilliseconds;
            double blockedPercents = (double)(totalTime - _forkFreeTimes[i] - _forkEatingTimes[i]).TotalMilliseconds * 100 / totalTime.TotalMilliseconds;
            if (blockedPercents < 0)
            {
                blockedPercents = 0;
                inEatingPercents = 100 - availablePercents;
            }
            _logger.Log(LogLevel.Information, "\tFork {0}: Available: {1} / Blocked: {2} / In Use: {3}", i, availablePercents, blockedPercents, inEatingPercents);
        }
    }

    public void OnPhilosopherChangedStatus(Philosopher philosopher, PhilosopherState state)
    {
        int philosopherI = -1;
        for (int i = 0; i < _philosophers.Count; ++i)
        {
            if (philosopher == _philosophers[i])
            {
                philosopherI = i;
                break;
            }
        }

        if (philosopherI == -1)
            return;

        _lastStateStarts[philosopherI] = DateTime.Now;

        switch (state)
        {
            case PhilosopherState.Thinking:
                {
                    if (_lastEatingStarts[philosopherI] == DateTime.MinValue)
                        return;

                    _forkEatingTimes[_ownForks[philosopherI].Item1] += _lastStateStarts[philosopherI] - _lastEatingStarts[philosopherI];
                    _forkEatingTimes[_ownForks[philosopherI].Item2] += _lastStateStarts[philosopherI] - _lastEatingStarts[philosopherI];

                    break;
                }
            case PhilosopherState.Hungry:
                {
                    _lastWaitingStarts[philosopherI] = _lastStateStarts[philosopherI];
                    break;
                }
            case PhilosopherState.Eating:
                {
                    _lastEatingStarts[philosopherI] = _lastStateStarts[philosopherI];
                    _waitingSums[philosopherI] += _lastEatingStarts[philosopherI] - _lastWaitingStarts[philosopherI];
                    _waitingCounts[philosopherI] += 1;


                    List<int> ph_forks = [];

                    for (int j = 0; j < _forks.Count; ++j)
                    {
                        if (_forks[j].Owner == philosopher.Name)
                        {
                            ph_forks.Add(j);
                        }
                    }

                    _ownForks[philosopherI] = (ph_forks[0], ph_forks[1]);

                    break;
                }
            default:
                break;
        }
    }

    public void OnForkOwnerChanged(Fork fork, IForkOwner? owner)
    {
        for (int i = 0; i < _forks.Count; ++i)
        {
            if (fork == _forks[i])
            {
                if (owner == null)
                {
                    _forkFreeStarts[i] = DateTime.Now;
                }
                else
                {
                    if (_forkFreeStarts[i] == DateTime.MinValue)
                    {
                        _forkFreeTimes[i] += DateTime.Now - _start;
                    }
                    else
                    {
                        _forkFreeTimes[i] += DateTime.Now - _forkFreeStarts[i];
                    }
                }
            }
        }
    }

    public void AddPhilosopher(Philosopher philosopher)
    {
        lock(_lockobj)
        {
            _philosophers.Add(philosopher);
            _lastStateStarts.Add(DateTime.MinValue);
            _lastWaitingStarts.Add(DateTime.MinValue);
            _waitingSums.Add(TimeSpan.Zero);
            _waitingCounts.Add(0);
            _lastEatingStarts.Add(DateTime.MinValue);
            _ownForks.Add((-1, -1));
            philosopher.ChangedState += OnPhilosopherChangedStatus;
        }
    }
}