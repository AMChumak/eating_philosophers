namespace ForkLib;

public interface ITableManager
{
    List<Fork> GetForks();
    int TakeSeat(string philosopherName);
    Fork GetLeftFork(int seatIndex);
    Fork GetRightFork(int seatIndex);
}