namespace ForkLib;

public interface ITableManager
{
    List<IFork> GetForks();
    int TakeSeat(string philosopherName);
    IFork GetLeftFork(int seatIndex);
    IFork GetRightFork(int seatIndex);
}