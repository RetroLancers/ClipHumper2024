namespace ClipHunta2;

public sealed class TrainingDataTaskManager : LongTaskManger<TrainingDataTask>
{
    private static TrainingDataTaskManager _instance;

    public TrainingDataTaskManager()
    {
        _longTasks = Array.Empty<TrainingDataTask>();
    }

    public override TrainingDataTask createOne()
    {
        return new TrainingDataTask(_cancellationToken);
    }

    public static TrainingDataTaskManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = new TrainingDataTaskManager();
            _cancellationToken = new CancellationTokenSource();
        }

        return _instance;
    }
}