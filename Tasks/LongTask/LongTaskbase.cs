namespace ClipHunta2.Tasks.LongTask;

/// <summary>
/// Base class for long-running tasks.
/// Provides core functionality for starting, stopping, and managing the task lifecycle.
/// </summary>
public abstract class LongTask
{
    private readonly CancellationTokenSource _cts;
    private readonly AutoResetEvent _are = new(false); // This AutoResetEvent seems unused. Consider removing if not needed.

    /// <summary>
    /// Default sleep duration used in task iterations if no work is present.
    /// </summary>
    protected static readonly TimeSpan DefaultSleep = TimeSpan.FromMilliseconds(250);
    private Task? _task;

    /// <summary>
    /// Initializes a new instance of the <see cref="LongTask"/> class.
    /// </summary>
    /// <param name="cts">The cancellation token source to control task cancellation.</param>
    public LongTask(CancellationTokenSource cts)
    {
        _cts = cts;
    }

    /// <summary>
    /// Delays the task execution for the specified amount of time.
    /// </summary>
    /// <param name="amount">The duration to sleep.</param>
    protected async Task _sleep(TimeSpan amount)
    {
        await Task.Delay(amount, _cts.Token); // Pass cancellation token to Task.Delay
    }

    /// <summary>
    /// Gets the current count of items being processed or queued by the task.
    /// </summary>
    /// <returns>The count of items.</returns>
    public abstract int Count();

    /// <summary>
    /// Returns a string representation of the task's status, typically including the count of items.
    /// </summary>
    /// <returns>A string representing the task's status.</returns>
    public override string ToString()
    {
        return $"Total: {Count()}";
    }

    /// <summary>
    /// Starts the long-running task.
    /// </summary>
    /// <exception cref="Exception">Thrown if the task is already running.</exception>
    public virtual void StartTask()
    {
        if (_task != null)
        {
            throw new Exception("Task is still running");
        }
    
        _task = Task.Run(async () =>
        {
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    await _iteration();
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
            // Consider adding more specific exception handling if needed
            finally
            {
                _task = null;
            }
        });
    }

    /// <summary>
    /// Represents a single iteration of the task's work.
    /// This method is called repeatedly until the task is cancelled.
    /// Derived classes should override this method to implement their specific logic.
    /// </summary>
    protected virtual async Task _iteration()
    {
        // Default implementation does nothing.
        await Task.CompletedTask; // Explicitly show it's a no-op.
    }
}