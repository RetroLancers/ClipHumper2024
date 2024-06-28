using Serilog;

namespace ClipHunta2;

public class EventRouterTask : LongTask<(StreamDefinition streamDefinition, FrameEvent[] frameEvents,
    StreamCaptureStatus streamCaptureStatus)>
{
    public static List<(StreamDefinition streamDefinition, FrameEvent frameEvent)> EventsrecvReverent { get; } = new();

    public EventRouterTask(CancellationTokenSource cts) : base(cts)
    {
    }

    protected bool IsSameEvent(FrameEvent a, FrameEvent b)
    {
        return IsSameSecond(a, b);
    }

    private static void AddEvent((StreamDefinition streamDefinition, FrameEvent[] frameEvents) value)
    {
        Monitor.Enter(EventsrecvReverent);
        try
        {
            EventsrecvReverent.AddRange(value.frameEvents.Select(frame => (value.streamDefinition, frame)));
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Error recording frame event intoeventsrecv");
        }
        finally
        {
            Monitor.Exit(EventsrecvReverent);
        }
    }

    protected bool IsSameSecond(FrameEvent a, FrameEvent b)
    {
        return b.EventName == a.EventName && b.Second == a.Second;
    }

    protected string[] blockedByElim = new[] { "elim" };

    protected override async Task<string?> _action(
        (StreamDefinition streamDefinition, FrameEvent[] frameEvents, StreamCaptureStatus streamCaptureStatus) value)
    {
        if (value.frameEvents.Length > 0)
        {
            Console.WriteLine(string.Join("," ,value.frameEvents.Select(a=>a.ToString())));
            AddEvent((value.streamDefinition, value.frameEvents));
        }


        switch (value.streamDefinition.StreamCaptureType)
        {
            case StreamCaptureType.Clip:
                
                break;
            case StreamCaptureType.Stream:

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        value.streamCaptureStatus.IncrementEventsRouted();
        value.streamCaptureStatus.IncrementFinishedCount();
        return null;
    }
}

public static class TwitchHelper
{
    private static object GetTwitchApi(string streamer)
    {
        return null;
    }

    public static object CreateClip(string streamer)
    {
        return null;
    }
}

public class ClipRecord
{
}