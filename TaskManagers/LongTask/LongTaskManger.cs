namespace ClipHunta2;

public class LongTaskManger<T> where T : LongTask
{
    private readonly CancellationTokenSource _cts;
    protected T[] _longTasks;
    private static LongTaskManger<T> _instance;
    protected static CancellationTokenSource _cancellationToken;

    public virtual T createOne()
    {
        return default;
    }

    public override string ToString()
    {
        return string.Join("\n", _longTasks.Select(a => a.ToString()));
    }


    public T? GetLongTasker()
    {
        var tmp = _longTasks;
        if (tmp.Length == 0)
        {
            return null;
        }

        return tmp.OrderBy(SortTasks).First();
    }
    public T? GetTopTasker()
    {
        var tmp = _longTasks;
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
        var _longTask = createOne();
        _longTask.StartTask();
        List<T> tmp = new List<T>(_longTasks);
        tmp.Add(_longTask);
        _longTasks = tmp.ToArray();
        tmp.Clear();
        tmp = null;
    }
}