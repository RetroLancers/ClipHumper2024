using System.Collections.Concurrent;

namespace ClipHunta2;

public partial class LongTask<T> : LongTask
{
    public LongTask(CancellationTokenSource cts) : base(cts)
    {
    }

    private readonly ConcurrentQueue<LongTaskQueueItem<T>> _queue = new();

    public override int Count()
    {
        return _queue.Count;
    }

    protected override async Task _iteration()
    {
        var value = await _take();
        if (value == null)
        {
            await _sleep(DefaultSleep);
            return;
        }

        await _action(value.Item);
    }

    protected virtual async Task _action(T value)
    {
    }

    private async Task<LongTaskQueueItem<T>?> _take()
    {
        if (_queue.TryDequeue(out var tmp))
            return tmp;
        return default;
    }

    public async Task Put(LongTaskQueueItem<T> work)
    {
        _queue.Enqueue(work);
    }
}

public partial class LongTaskWithReturn<T, TR> : LongTask
{
    private readonly ConcurrentQueue<LongTaskQueueItemWithReturn<T, TR>> _queue = new();

    protected override async Task _iteration()
    {
        var value = await _take();
        if (value == null)
        {
            await _sleep(DefaultSleep);
            return;
        }

        var tmp = await _action(value.Item);

        value.ReturnQueue.SetValue(tmp);
    }

    public override int Count()
    {
        return _queue.Count;
    }


    private async Task<LongTaskQueueItemWithReturn<T, TR>?> _take()
    {
        if (_queue.TryDequeue(out var tmp))
            return tmp;
        return default;
    }

    public async Task Put(LongTaskQueueItemWithReturn<T, TR> work)
    {
        _queue.Enqueue(work);
    }

    public async Task<TR?> PutAndGet(T item)
    {
        var returnQueue = new ReturnQueue();
        var work = new LongTaskQueueItemWithReturn<T, TR>(item, returnQueue);
        _queue.Enqueue(work);
        var retval = returnQueue.GetReturn();
        work = null;
        returnQueue = null;
        return retval;
    }

    protected virtual async Task<TR?> _action(T value)
    {
        return default;
    }


    public LongTaskWithReturn(CancellationTokenSource cts) : base(cts)
    {
    }
}