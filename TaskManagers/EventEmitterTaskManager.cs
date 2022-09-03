namespace ClipHunta2;

public sealed class EventEmitterTaskManager : LongTaskManger<EventEmitterTask>
{
    private static EventEmitterTaskManager _instance;

    public EventEmitterTaskManager()
    {
        _longTasks = Array.Empty<EventEmitterTask>();
    }

    public override EventEmitterTask createOne()
    {
        return new EventEmitterTask(_cancellationToken);
    }

    public static EventEmitterTaskManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = new EventEmitterTaskManager();
            _cancellationToken = new CancellationTokenSource();
        }

        return _instance;
    }
}