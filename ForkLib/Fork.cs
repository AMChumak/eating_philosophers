namespace ForkLib;

public class Fork
{
    private IForkOwner? _owner;

    public string Owner => _owner?.GetName() ?? "";

    public void Take(IForkOwner candidat)
    {
        if (_owner != null)
            return;

        switch (candidat.GetTakingStatus(this))
        {
            case TakingStatus.Inaction:
                candidat.SetTakingStatus(this, TakingStatus.InProgress);
                break;
            case TakingStatus.InProgress:
                _owner = candidat;
                candidat.SetTakingStatus(this, TakingStatus.Completed);
                break;
            default:
                break;
        }
    }

    public void Release(IForkOwner candidat)
    {
        if (_owner != candidat)
            return;

        _owner = null;
    }
}
