namespace StrategyImplementationLib;

using ForkLib;
using StrategyContractLib;

public class SimpleArbitrator : IArbitrator
{
    private event TakeForksCommand? _philosopherTakeForks;

    private readonly IReadOnlyList<IForkOwner> _owners;
    private readonly IReadOnlyList<Fork> _forks;
    private List<string> _hungries = [];
    private List<string> _serviced = [];

    public SimpleArbitrator( IReadOnlyList<IForkOwner> owners, IReadOnlyList<Fork> forks)
    {
        _owners = owners;
        _forks = forks;
    }
    public void Manage()
    {
        /* Choose serviced owners */
        for (int i = 0; i < _hungries.Count; ++i)
        {
            bool isNeighbour = false;

            for (int j = 0; j < _serviced.Count; ++j)
            {
                for (int k = 0; k < _owners.Count; ++k)
                {
                    if ((_owners[k].GetName() == _hungries[i] &&
                         _owners[(k + 1) % _owners.Count].GetName() == _serviced[j]) ||
                        (_owners[k].GetName() == _serviced[j] &&
                         _owners[(k + 1) % _owners.Count].GetName() == _hungries[i]))
                    {
                        isNeighbour = true;
                        break;
                    }
                }
            }

            if (isNeighbour)
                continue;

            _serviced.Add(string.Intern(_hungries[i]));
            _hungries.RemoveAt(i);
        }

        /* Command for each serviced */

        foreach (string client in _serviced)
        {
            for (int i = 0; i < _owners.Count; ++i)
            {
                if (_owners[i].GetName() == client)
                {
                    _philosopherTakeForks?.Invoke(client, _forks[i], _forks[(i + 1) % _forks.Count]);
                    break;
                }
            }
        }
    }

    public void WantForks(string name, TakeForksCommand handler)
    {
        _philosopherTakeForks += handler;
        _hungries.Add(string.Intern(name));
    }

    public void UnwantForks(string name, TakeForksCommand handler)
    {
        _philosopherTakeForks -= handler;
        _hungries.Remove(name);
        _serviced.Remove(name);
    }
}


