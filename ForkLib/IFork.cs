namespace ForkLib;

public delegate void ForkChangedOwner(IFork fork, IForkOwner? owner);
public interface IFork
{
    public string Owner { get; }
    int OrderNumber { get; }
    public event ForkChangedOwner? OwnerChanged;
    public bool Take(IForkOwner candidat);
    public void Release(IForkOwner candidat);
}