using Microsoft.Extensions.Options;
using SimulationSettingsLib;

namespace ForkLib;

public class TableManager : ITableManager
{
    private readonly IOptions<SimulationSettings> _settings;
    private readonly List<Fork> _forks;
    private readonly Dictionary<string, int> _philosopherSeats;
    private readonly object _seatLock = new object();
    private int _nextSeatIndex = 0;

    public TableManager(IOptions<SimulationSettings> settings)
    {
        _settings = settings;
        _forks = [];
        for (int i = 0; i < 5; ++i)
        {
            _forks.Add(new Fork(_settings.Value.ForkAcquisitionTimeMs, i));
        }
        _philosopherSeats = new Dictionary<string, int>();
    }

    public List<Fork> GetForks()
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

    public Fork GetLeftFork(int seatIndex)
    {
        if (seatIndex < 0 || seatIndex >= 5)
            throw new ArgumentOutOfRangeException(nameof(seatIndex), "Seat index must be between 0 and 4");

        return _forks[seatIndex];
    }

    public Fork GetRightFork(int seatIndex)
    {
        if (seatIndex < 0 || seatIndex >= 5)
            throw new ArgumentOutOfRangeException(nameof(seatIndex), "Seat index must be between 0 and 4");

        int rightForkIndex = (seatIndex + 1) % 5;
        return _forks[rightForkIndex];
    }

}