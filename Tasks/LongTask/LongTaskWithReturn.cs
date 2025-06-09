using System.Collections.Concurrent;
using Serilog;

namespace ClipHunta2.Tasks.LongTask;

/// <summary>
/// Represents a long-running task that processes items from a queue and returns a result for each item.
/// </summary>
/// <typeparam name="T">The type of the input item to be processed.</typeparam>
/// <typeparam name="TR">The type of the result returned after processing an item.</typeparam>
public partial class LongTaskWithReturn<T, TR> : LongTask
{
    private readonly ConcurrentQueue<LongTaskQueueItemWithReturn<T, TR>> _queue = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LongTaskWithReturn{T, TR}"/> class.
    /// </summary>
    /// <param name="cts">The cancellation token source.</param>
    public LongTaskWithReturn(CancellationTokenSource cts) : base(cts)
    {
    }

    /// <summary>
    /// Performs a single iteration of the task. It takes an item, processes it using <see cref="_action(T)"/>,
    /// and sets the result on the item's return queue.
    /// </summary>
    protected override async Task _iteration()
    {
        var queueItem = await _take();
        if (queueItem == null)
        {
            await _sleep(DefaultSleep);
            return;
        }

        var watch = System.Diagnostics.Stopwatch.StartNew(); // More concise way to start stopwatch

        try
        {
            var result = await _action(queueItem.Item);
            queueItem.ReturnQueue.SetValue(result);
            watch.Stop();
            // Consider using Log.Debug or Log.Information for execution time if it's consistently useful
            // Console.WriteLine($"Execution Time for {typeof(T).Name}: {watch.ElapsedMilliseconds} ms");
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, "Error in _iteration for task {TaskType} while processing item of type {ItemType}", this.GetType().Name, typeof(T).Name);
            queueItem.ReturnQueue.SetException(e); // Optionally notify the caller about the error
        }
    }

    /// <summary>
    /// Gets the number of items currently in the task's queue.
    /// </summary>
    /// <returns>The count of items in the queue.</returns>
    public override int Count()
    {
        return _queue.Count;
    }

    /// <summary>
    /// Attempts to take an item from the queue.
    /// </summary>
    /// <returns>A queue item with a return mechanism, or null if no item could be retrieved.</returns>
    protected async Task<LongTaskQueueItemWithReturn<T, TR>?> _take()
    {
        if (_queue.TryDequeue(out var tmp))
        {
            return tmp;
        }
        return default;
    }

    /// <summary>
    /// Synchronously takes an item from the queue. This method blocks until an item is available or the take operation completes.
    /// </summary>
    /// <remarks>
    /// This method can lead to deadlocks if not used carefully, especially in UI contexts or with async code.
    /// Prefer asynchronous operations where possible.
    /// </remarks>
    /// <returns>The result of the take operation, which might be a queue item or null.</returns>
    public LongTaskQueueItemWithReturn<T, TR>? Take()
    {
        // Blocking call, can be problematic. Consider alternatives if this causes issues.
        return _take().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Adds a work item (that expects a return value) to the processing queue.
    /// </summary>
    /// <param name="work">The work item to add.</param>
    public async Task Put(LongTaskQueueItemWithReturn<T, TR> work)
    {
        // Similar to LongTask<T>.Put, Task.Run usage and async signature could be reviewed.
        Task.Run(() => { _queue.Enqueue(work); });
        await Task.CompletedTask;
    }

    /// <summary>
    /// Puts an item into the queue and asynchronously waits for its processing result.
    /// </summary>
    /// <param name="item">The item to process.</param>
    /// <returns>The result of processing the item, or null if processing fails or returns no result.</returns>
    public async Task<TR?> PutAndGet(T item)
    {
        var returnQueue = new ReturnQueue(); // Consider making ReturnQueue IDisposable if it holds resources
        var work = new LongTaskQueueItemWithReturn<T, TR>(item, returnQueue);
        _queue.Enqueue(work); // Directly enqueue, Task.Run likely not needed here

        // Asynchronously wait for the result.
        // The GetReturn method in ReturnQueue should ideally be async or provide an async mechanism.
        // For now, assuming GetReturn might block or has its own async handling internally.
        TR? retval = await returnQueue.GetReturnAsync();

        // No need to null out local variables like 'work' and 'returnQueue' here, they go out of scope.
        return retval;
    }

    /// <summary>
    /// The core action to be performed on each item, producing a result of type <typeparamref name="TR"/>.
    /// Derived classes should override this method to define specific processing logic.
    /// </summary>
    /// <param name="value">The item to be processed.</param>
    /// <returns>The result of processing the item.</returns>
    protected virtual async Task<TR?> _action(T value)
    {
        return await Task.FromResult<TR?>(default); // More explicit async default
    }
}