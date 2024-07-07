using ClipHunta2.TaskManagers.LongTask;
using ClipHunta2.Tasks;

namespace ClipHunta2.TaskManagers;

public sealed class ClipTaskManager : LongTaskManger<ClipTask>
{
    private static ClipTaskManager? _instance;

    public ClipTaskManager()
    {
        LongTasks = [];
    }

    protected override ClipTask createOne()
    {
        return new ClipTask(CancellationToken);
    }

    public static ClipTaskManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = new ClipTaskManager();
            CancellationToken = new CancellationTokenSource();
        }

        return _instance;
    }
}