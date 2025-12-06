using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

enum ForkState
{
    Available,
    InUse,
};

public class Table
{
    private const int COUNT_FORK = 5;

    private bool[] _forks;
    public Table()
    {
        _forks = new bool[COUNT_FORK];

        for (int i = 0; i < COUNT_FORK; ++i)
            _forks[i] = false;
    }
    public void Take(int forkId)
    {
        Debug.Assert(!_forks[forkId]);
        _forks[forkId] = true;
    }

    public void Free(int forkId)
    {
        Debug.Assert(_forks[forkId]);
        _forks[forkId] = false;
    }

    public bool IsForkFree(int forkId)
    {
        return !_forks[forkId];
    }
}