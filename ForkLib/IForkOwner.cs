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

    void SetTakingStatus(Fork fork, TakingStatus status);

    TakingStatus GetTakingStatus(Fork fork);

    void ReleaseFork(Fork fork);
}
