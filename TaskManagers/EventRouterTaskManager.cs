namespace ClipHunta2;

public sealed class EventRouterTaskManager : LongTaskManger<EventRouterTask>
{
    private static EventRouterTaskManager _instance;

    public EventRouterTaskManager()
    {
        _longTasks = Array.Empty<EventRouterTask>();
    }

    public override EventRouterTask createOne()
    {
        return new EventRouterTask(_cancellationToken);
    }

    public static EventRouterTaskManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = new EventRouterTaskManager();
            _cancellationToken = new CancellationTokenSource();
        }

        return _instance;
    }
}