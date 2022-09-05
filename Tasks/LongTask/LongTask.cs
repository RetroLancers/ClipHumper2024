using System.Collections.Concurrent;
using Serilog;

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

        try
        {
            await _action(value.Item);
        }
        catch (Exception e)
        {
            Log.Logger.Error("Error in _iteration {Message} Stack: {Stack}", e.Message, e.StackTrace);
        }
    }

    protected virtual async Task _action(T value)
    {
    }

    protected virtual LongTask<T>? GetTop()
    {
        return null;
    }

    private async Task<LongTaskQueueItem<T>?> _take()
    {
        if (_queue.TryDequeue(out var tmp))
            return tmp;
        var man = GetTop();

        if (man == null) return default;
        if (man._queue.Count < 2) return default;
        if (man._queue.TryDequeue(out var tmp2))
            return tmp2;

        return default;
    }

    public async Task Put(LongTaskQueueItem<T> work)
    {
        Task.Run(() => { _queue.Enqueue(work); });
    }
}