using Microsoft.Extensions.Options;
using SimulationSettingsLib;
using ForkLib;

namespace WebTable;

public class NetTableManager : ITableManager
{
    private readonly IOptions<SimulationSettings> _settings;
    private readonly List<IFork> _forks;
    private readonly Dictionary<string, int> _philosopherSeats;
    private readonly object _seatLock = new object();
    private int _nextSeatIndex = 0;

    public NetTableManager(IOptions<SimulationSettings> settings)
    {
        _settings = settings;
        _forks = [];
        for (int i = 0; i < 5; ++i)
        {
            _forks.Add(new NetFork(i));
        }
        _philosopherSeats = new Dictionary<string, int>();
    }

    public List<IFork> GetForks()
    {
        return _forks;
    }

    public int TakeSeat(string philosopherName)
    {
        lock (_seatLock)
        {
            if (_philosopherSeats.ContainsKey(philosopherName))
            {
                return _philosopherSeats[philosopherName];
            }

            if (_nextSeatIndex >= 5)
            {
                throw new InvalidOperationException("No more seats available at the table");
            }

            int seatIndex = _nextSeatIndex;
            _philosopherSeats[philosopherName] = seatIndex;
            _nextSeatIndex++;

            return seatIndex;
        }
    }

    public IFork GetLeftFork(int seatIndex)
    {
        if (seatIndex == 4)
            return _forks[0];

        if (seatIndex < 0 || seatIndex >= 5)
            throw new ArgumentOutOfRangeException(nameof(seatIndex), "Seat index must be between 0 and 4");

        return _forks[seatIndex];
    }

    public IFork GetRightFork(int seatIndex)
    {
        if (seatIndex == 4)
            return _forks[4];

        if (seatIndex < 0 || seatIndex >= 5)
            throw new ArgumentOutOfRangeException(nameof(seatIndex), "Seat index must be between 0 and 4");

        int rightForkIndex = (seatIndex + 1) % 5;
        return _forks[rightForkIndex];
    }

}