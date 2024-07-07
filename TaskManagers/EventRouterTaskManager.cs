using ClipHunta2.TaskManagers.LongTask;
using ClipHunta2.Tasks;

namespace ClipHunta2.TaskManagers;

public sealed class EventRouterTaskManager : LongTaskManger<EventRouterTask>
{
    private static EventRouterTaskManager? _instance;

    private EventRouterTaskManager()
    {
        LongTasks = [];
    }

    protected override EventRouterTask createOne()
    {
        return new EventRouterTask(CancellationToken);
    }

    public static EventRouterTaskManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = new EventRouterTaskManager();
            CancellationToken = new CancellationTokenSource();
        }

        return _instance;
    }
}