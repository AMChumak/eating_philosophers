namespace WebCoordinator;

public class EventHandler
{
    private Coordinator coordinator;
    private Table table;

    public EventHandler(Coordinator coordinator, Table table)
    {
        this.coordinator = coordinator;
        this.table = table;
    }

    public void ProccessNoticeFork(int philId)
    {
        CoordTask task = new CoordTask
        {
            PhilId = philId
        };

        coordinator.AddTask(task);
    }

    public void ProcessRealiseFork(int forkId)
    {
        table.Free(forkId);
        _ = coordinator.TryDoTask();
    }
}