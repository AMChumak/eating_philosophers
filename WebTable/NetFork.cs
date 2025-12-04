using static System.Threading.Thread;
using System.Diagnostics.CodeAnalysis;
using ForkLib;

namespace WebTable;

public class NetFork: IFork
{
    private IForkOwner? _owner;

    private object _lock = new();

    public string Owner => _owner?.GetName() ?? "";


    public required int OrderNumber { get; init; }

    public event ForkChangedOwner? OwnerChanged;

    [SetsRequiredMembers]
    public NetFork(int orderNumber)
    {
        OrderNumber = orderNumber;
    }

    public bool Take(IForkOwner candidat)
    {
        candidat.SetTakingStatus(this, TakingStatus.InProgress);

        lock(_lock)
        {

            if (_owner != null && _owner != candidat)
            {
                return false;
            }

            _owner = candidat;
            candidat.SetTakingStatus(this, TakingStatus.Completed);
            OwnerChanged?.Invoke(this, candidat);
        }
        return true;
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
