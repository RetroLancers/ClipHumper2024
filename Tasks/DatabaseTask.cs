namespace ClipHunta2;

public class DatabaseTask : LongTask<object>
{
    public DatabaseTask(CancellationTokenSource cts) : base(cts)
    {
    }

    protected override Task _action(object value)
    {
        return null;
        return base._action(value);
    }
}