namespace ClipHunta2.TaskManagers.LongTask;

public class LongTaskManger<T> where T : Tasks.LongTask.LongTask
{
    private readonly CancellationTokenSource? _cts;
    protected T[] LongTasks;
    private static LongTaskManger<T> _instance;
    protected static CancellationTokenSource CancellationToken;

    protected virtual T createOne()
    {
        return default;
    }

    public override string ToString()
    {
        return string.Join("\n", LongTasks.Select(a => a.ToString()));
    }


    public T? GetLongTasker()
    {
        var tmp = LongTasks;
        if (tmp.Length == 0)
        {
            return null;
        }

        return tmp.OrderBy(SortTasks).First();
    }
    public T? GetTopTasker()
    {
        var tmp = LongTasks;
        if (tmp.Length == 0)
        {
            return null;
        }

        return tmp.OrderByDescending(SortTasks).First();
    }
    private static int SortTasks(T t)
    {
        return t.Count();
    }

    public void AddLongTasker()
    {
        var longTask = createOne();
        longTask.StartTask();
        List<T> tmp =
        [
            ..LongTasks,
            longTask
        ];
        LongTasks = tmp.ToArray();
        tmp.Clear();
        tmp = null;
    }
}