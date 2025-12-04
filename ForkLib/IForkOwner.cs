namespace ForkLib;

public enum TakingStatus
{
    Inaction,
    InProgress,
    Completed,
}

public interface IForkOwner
{
    string GetName();

    void SetTakingStatus(IFork fork, TakingStatus status);

    TakingStatus GetTakingStatus(IFork fork);

    void ReleaseFork(IFork fork);
}
