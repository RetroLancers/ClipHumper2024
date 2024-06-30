namespace ClipHunta2;

public sealed class ClipTaskManager : LongTaskManger<ClipTask>
{
    private static ClipTaskManager? _instance;

    public ClipTaskManager()
    {
        _longTasks = [];
    }

    public override ClipTask createOne()
    {
        return new ClipTask(_cancellationToken);
    }

    public static ClipTaskManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = new ClipTaskManager();
            _cancellationToken = new CancellationTokenSource();
        }

        return _instance;
    }
}