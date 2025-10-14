using ForkLib;
using PhilosopherLib;

namespace EatingPhilosophers;

public class Statistics
{
    private DateTime _start;
    private readonly List<Philosopher> _philosophers;
    private readonly List<Fork> _forks;
    private List<DateTime> _lastStateStarts;
    private List<DateTime> _lastWaitingStarts;
    private List<TimeSpan> _waitingSums;
    private List<int> _waitingCounts;
    private List<DateTime> _lastEatingStarts;
    private List<DateTime> _forkFreeStarts;
    private List<TimeSpan> _forkFreeTimes;
    private List<TimeSpan> _forkEatingTimes;
    private List<(int,int)> _ownForks;

    public int IntervalDuration { get; set; }

    public Statistics(List<Philosopher> philosophers, List<Fork> forks, int intervalMs)
    {
        _philosophers = philosophers;
        _forks = forks;
        _lastStateStarts = Enumerable.Repeat(DateTime.MinValue, philosophers.Count).ToList();
        _lastWaitingStarts = Enumerable.Repeat(DateTime.MinValue, philosophers.Count).ToList();
        _waitingSums = Enumerable.Repeat(TimeSpan.Zero, philosophers.Count).ToList();
        _waitingCounts = Enumerable.Repeat(0, philosophers.Count).ToList();
        _lastEatingStarts = Enumerable.Repeat(DateTime.MinValue, philosophers.Count).ToList();
        _forkFreeStarts = Enumerable.Repeat(DateTime.MinValue, forks.Count).ToList();
        _forkFreeTimes = Enumerable.Repeat(TimeSpan.Zero, forks.Count).ToList();
        _forkEatingTimes = Enumerable.Repeat(TimeSpan.Zero, forks.Count).ToList();
        _ownForks = Enumerable.Repeat((-1, -1), philosophers.Count).ToList();
        IntervalDuration = intervalMs;

        foreach (var philosopher in _philosophers)
        {
            philosopher.ChangedState += OnPhilosopherChangedStatus;
        }

        foreach (var fork in _forks)
        {
            fork.OwnerChanged += OnForkOwnerChanged;
        }
    }

    private void PrintSystemState()
    {
        Console.WriteLine("---------------- TIME {0} ----------------", DateTime.Now);
        for (int i = 0; i < _philosophers.Count; ++i)
        {
            TimeSpan waiting = DateTime.Now - _lastStateStarts[i];
            TimeSpan spentTime = DateTime.Now - _start;
            Console.WriteLine("{0}: {1} (In state: {2} steps) [Current action: {3}] Throuhput: {4}", _philosophers[i].Name, _philosophers[i].State.ToString(), waiting.TotalMilliseconds, _philosophers[i].Action.ToString(), _philosophers[i].Score * 1000 / spentTime.TotalMilliseconds);
        }

        for (int i = 0; i < _forks.Count; ++i)
        {
            if (_forks[i].Owner != "")
            {
                Console.WriteLine("Fork {0}: In Use (Owner: {1}) ", i, _forks[i].Owner);
            }
            else
            {
                Console.WriteLine("Fork {0}: Available", i);
            }
        }
    }

    public void Overwatch(int countIntervals)
    {
        _start = DateTime.Now;
        for (int i = 0; i < countIntervals; i++)
        {
            Thread.Sleep(IntervalDuration);
            PrintSystemState();
        }
    }

    public void PrintStatistics(TimeSpan totalTime)
    {
        Console.WriteLine("---------------- STATISTICS ----------------");

        Console.WriteLine("\nPhilosophers:\n");

        for (int i = 0; i < _philosophers.Count; ++i)
        {
            Console.WriteLine("\t{0}: Throughput: {1}, Average waiting: {2} ms", _philosophers[i].Name, _philosophers[i].Score * 1000 / totalTime.TotalMilliseconds, _waitingSums[i].TotalMilliseconds / _waitingCounts[i]);
        }

        Console.WriteLine("\n\nForks:\n");

        for (int i = 0; i < _forks.Count; ++i)
        {
            double availablePercents = (double)_forkFreeTimes[i].TotalMilliseconds * 100 / totalTime.TotalMilliseconds;
            double blockedPercents = (double)(totalTime - _forkFreeTimes[i] - _forkEatingTimes[i]).TotalMilliseconds * 100 / totalTime.TotalMilliseconds;
            double inEatingPercents = (double)_forkEatingTimes[i].TotalMilliseconds * 100 / totalTime.TotalMilliseconds;
            Console.WriteLine("\tFork {0}: Available: {1} / Blocked: {2} / In Use: {3}", i, availablePercents, blockedPercents, inEatingPercents);
        }
    }

    private void OnPhilosopherChangedStatus(Philosopher philosopher, PhilosopherState state)
    {
        int philosopherI = -1 ;
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

    private void OnForkOwnerChanged(Fork fork, IForkOwner? owner)
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


}