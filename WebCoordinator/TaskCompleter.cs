namespace WebCoordinator;
using Publisher;

public class TaskCompleter
{
    ResponseSender responseSender;

    public TaskCompleter(IPublisher publisher)
    {
        responseSender = new ResponseSender(publisher);
    }

    public void  DoTask(CoordTask task)
    {
        responseSender.SendForkPerm(task.PhilId);
    }
}