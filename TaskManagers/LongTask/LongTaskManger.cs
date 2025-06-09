namespace ClipHunta2.TaskManagers.LongTask;

/// <summary>
/// Manages a collection of long-running tasks of a specific type <typeparamref name="T"/>.
/// Responsible for creating, adding, and providing access to these tasks.
/// </summary>
/// <typeparam name="T">The type of <see cref="Tasks.LongTask.LongTask"/> being managed.</typeparam>
public class LongTaskManger<T> where T : Tasks.LongTask.LongTask
{
    private readonly CancellationTokenSource? _cts; // This seems unused. Consider removing if not intended for future use.

    /// <summary>
    /// Array holding the managed long tasks.
    /// </summary>
    protected T[] LongTasks;

    /// <summary>
    /// A cancellation token source that can be used by derived classes when creating tasks.
    /// This is static and shared among all instances of LongTaskManager of the same generic type T,
    /// which might be intended for global cancellation of a certain type of task.
    /// </summary>
    protected static CancellationTokenSource CancellationToken;

    /// <summary>
    /// Factory method for creating a new instance of a task of type <typeparamref name="T"/>.
    /// Derived classes should override this to provide specific task instantiation logic.
    /// </summary>
    /// <returns>A new task instance, or default if creation is not possible.</returns>
    protected virtual T createOne()
    {
        return default;
    }

    /// <summary>
    /// Returns a string representation of the manager, typically listing the status of all managed tasks.
    /// </summary>
    /// <returns>A string summarizing the state of the managed tasks.</returns>
    public override string ToString()
    {
        return string.Join("\n", LongTasks.Select(a => a.ToString()));
    }

    /// <summary>
    /// Gets a task from the managed tasks, typically the one with the least work (for load balancing).
    /// Tasks are sorted by their item count in ascending order.
    /// </summary>
    /// <returns>The task with the fewest items in its queue, or null if no tasks are managed.</returns>
    public T? GetLongTasker()
    {
        var tmp = LongTasks;
        if (tmp == null || tmp.Length == 0) // Added null check for tmp
        {
            return null;
        }

        return tmp.OrderBy(SortTasks).FirstOrDefault(); // Changed to FirstOrDefault
    }

    /// <summary>
    /// Gets a task from the managed tasks, typically the one with the most work.
    /// Tasks are sorted by their item count in descending order.
    /// </summary>
    /// <returns>The task with the most items in its queue, or null if no tasks are managed.</returns>
    public T? GetTopTasker()
    {
        var tmp = LongTasks;
        if (tmp == null || tmp.Length == 0) // Added null check for tmp
        {
            return null;
        }

        return tmp.OrderByDescending(SortTasks).FirstOrDefault(); // Changed to FirstOrDefault
    }

    /// <summary>
    /// Sorting key function for ordering tasks based on their current item count.
    /// </summary>
    /// <param name="t">The task to get the count from.</param>
    /// <returns>The item count of the task.</returns>
    private static int SortTasks(T t)
    {
        return t.Count();
    }

    /// <summary>
    /// Creates a new task using <see cref="createOne"/>, starts it, and adds it to the collection of managed tasks.
    /// </summary>
    public void AddLongTasker()
    {
        var longTask = createOne();
        if (longTask == null)
        {
            // Optionally log or handle the case where a task cannot be created
            return;
        }
        longTask.StartTask();

        List<T> tmpList = LongTasks == null ? new List<T>() : new List<T>(LongTasks);
        tmpList.Add(longTask);
        LongTasks = tmpList.ToArray();
        // No need to Clear and null List<T> 'tmpList' as it will go out of scope.
    }
}