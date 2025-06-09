using System.Collections.Concurrent;
using Serilog;

namespace ClipHunta2.Tasks.LongTask;

/// <summary>
/// Represents a generic long-running task that processes items from a queue.
/// </summary>
/// <typeparam name="T">The type of items to be processed by the task.</typeparam>
public class LongTask<T> : LongTask
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LongTask{T}"/> class.
    /// </summary>
    /// <param name="cts">The cancellation token source.</param>
    public LongTask(CancellationTokenSource cts) : base(cts)
    {
    }

    private readonly ConcurrentQueue<LongTaskQueueItem<T>> _queue = new();

    /// <summary>
    /// Gets the number of items currently in the task's queue.
    /// </summary>
    /// <returns>The count of items in the queue.</returns>
    public override int Count()
    {
        return _queue.Count;
    }

    /// <summary>
    /// Performs a single iteration of the task, which involves taking an item from the queue and processing it.
    /// </summary>
    protected override async Task _iteration()
    {
        var value = await _take();
        if (value == null)
        {
            await _sleep(DefaultSleep); // If queue is empty, sleep for a default duration
            return;
        }

        try
        {
            await _action(value.Item); // Process the item
        }
        catch (Exception e)
        {
            // Log any errors that occur during item processing
            Log.Logger.Error(e, "Error in _iteration for task {TaskType} while processing item of type {ItemType}", this.GetType().Name, typeof(T).Name);
        }
    }

    /// <summary>
    /// The core action to be performed on each item taken from the queue.
    /// Derived classes should override this method to define specific processing logic.
    /// </summary>
    /// <param name="value">The item to be processed.</param>
    protected virtual async Task _action(T value)
    {
        // Default implementation does nothing.
        await Task.CompletedTask;
    }

    /// <summary>
    /// Gets the "top" or highest priority task manager, typically for work stealing.
    /// This method is intended to be overridden by derived classes if they support work stealing from other task managers.
    /// </summary>
    /// <returns>The top task manager, or null if not applicable.</returns>
    protected virtual LongTask<T>? GetTop()
    {
        return null;
    }

    /// <summary>
    /// Attempts to take an item from the queue.
    /// If the local queue is empty, it may attempt to "steal" work from a "top" task manager if implemented by <see cref="GetTop"/>.
    /// </summary>
    /// <returns>A queue item, or null if no item could be retrieved.</returns>
    private async Task<LongTaskQueueItem<T>?> _take()
    {
        if (_queue.TryDequeue(out var tmp))
        {
            return tmp;
        }

        // Attempt to get work from another manager (work stealing)
        var topTaskManager = GetTop();
        if (topTaskManager == null || topTaskManager._queue.Count < 2) // Ensure there's enough work to steal
        {
            return default;
        }

        if (topTaskManager._queue.TryDequeue(out var stolenItem))
        {
            Log.Debug("Task {TaskType} stole item from {TopTaskType}", this.GetType().Name, topTaskManager.GetType().Name);
            return stolenItem;
        }

        return default;
    }

    /// <summary>
    /// Adds an item to the task's processing queue.
    /// </summary>
    /// <param name="work">The item to be added to the queue.</param>
    public async Task Put(LongTaskQueueItem<T> work)
    {
        // Enqueues the work item. Consider if Task.Run is strictly necessary here
        // or if _queue.Enqueue(work) is sufficient and if the Put method needs to be async.
        // For now, keeping existing behavior.
        Task.Run(() => { _queue.Enqueue(work); });
        await Task.CompletedTask; // To satisfy async, if Put remains async. If not, remove await and change signature.
    }
}