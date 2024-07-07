namespace ClipHunta2.Tasks.LongTask;

public class LongTaskQueueItemWithReturn<T, TR> : LongTaskQueueItem<T>
{
    public LongTaskQueueItemWithReturn(T item, LongTaskWithReturn<T, TR>.ReturnQueue returnQueue) : base(item)
    {
        Item = item;
        ReturnQueue = returnQueue;
    }


    public LongTaskWithReturn<T, TR>.ReturnQueue ReturnQueue { get; }
}

public class LongTaskQueueItem<T>
{
    public LongTaskQueueItem(T item)
    {
        Item = item;
    }

    public T Item { get; protected init; }
}