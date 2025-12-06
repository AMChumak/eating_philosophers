using System.Diagnostics;
using Publisher;

namespace WebCoordinator;

public struct CoordTask
{
    public int PhilId;
}

public class Coordinator
{
    Table table;
    TaskCompleter completer;
    private List<CoordTask> tasks;

    public Coordinator(IPublisher publisher, Table table)
    {
        this.table = table;
        completer = new TaskCompleter(publisher);

        tasks = new List<CoordTask>();
    }

    public void AddTask(CoordTask task)
    {
        tasks.Add(task);
        _ = TryDoTask();
    }

    public bool TryDoTask()
    {
        Debug.Assert(tasks.Count != 0);

        for (int i = 0; i < tasks.Count; i++)
        {
            CoordTask task = tasks[i];
            if (table.IsForkFree(task.PhilId) && table.IsForkFree((task.PhilId + 1) % 5))
            {
                table.Take(task.PhilId);
                table.Take((task.PhilId + 1) % 5);
                tasks.RemoveAt(i);
                completer.DoTask(task);
                return true;
            }
        }
        return false;
    }
}