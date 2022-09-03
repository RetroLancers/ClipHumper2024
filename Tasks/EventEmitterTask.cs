using Serilog;

namespace ClipHunta2;

public class EventEmitterTask : LongTask<Tuple<string, string[]>>
{
#if DEBUGTEST
   public List<Tuple<string, string[]>> eventsrecv {get;set;} = new();
#endif
    public EventEmitterTask(CancellationTokenSource cts) : base(cts)
    {
    }

    protected override async Task<string?> _action(Tuple<string, string[]> value)
    {
        //push to somewhere?

#if DEBUGTEST
    eventsrecv.Add(value);
#endif
        Log.Logger.Debug(string.Join(",", value));
        return null;
    }
}