namespace ClipHunta2;

public sealed class DatabaseTaskManager : LongTaskManger<DatabaseTask>
{
    private static DatabaseTaskManager? _instance;

    public DatabaseTaskManager()
    {
        _longTasks = [];
    }

    public override DatabaseTask createOne()
    {
        return new DatabaseTask(_cancellationToken);
    }

    public static DatabaseTaskManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = new DatabaseTaskManager();
            _cancellationToken = new CancellationTokenSource();
        }

        return _instance;
    }
}