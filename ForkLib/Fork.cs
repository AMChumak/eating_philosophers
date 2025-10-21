using static System.Threading.Thread;

namespace ForkLib;

public delegate void ForkChangedOwner(Fork fork, IForkOwner? owner);
public class Fork
{
    private IForkOwner? _owner;

    private object _lock = new();

    public string Owner => _owner?.GetName() ?? "";

    private int _acquisitionTimeMs;

    public event ForkChangedOwner? OwnerChanged;

    public Fork(int acquisitionTimeMs)
    {
        _acquisitionTimeMs = acquisitionTimeMs;
    }

    public void Take(IForkOwner candidat)
    {
        candidat.SetTakingStatus(this, TakingStatus.InProgress);
        Sleep(20);

        lock(_lock)
        {

            while (_owner != null && _owner != candidat)
            {
                Monitor.Wait(_lock);
            }

            _owner = candidat;
            candidat.SetTakingStatus(this, TakingStatus.Completed);
            OwnerChanged?.Invoke(this, candidat);
        }
    }

    public void Release(IForkOwner candidat)
    {
        if (_owner != candidat)
            return;

        lock(_lock)
        {
            _owner = null;
            candidat.SetTakingStatus(this, TakingStatus.Inaction);
            OwnerChanged?.Invoke(this, null);

            Monitor.PulseAll(_lock);
        }
    }
}
