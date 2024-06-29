using System.Collections;
using System.Text;

namespace ClipHunta2;

public class FrameEventGroup() : IEnumerable<FrameEvent>
{
    public readonly List<FrameEvent> Events = [];
    public bool Processed { get; set; } = false;


    public IEnumerator<FrameEvent> GetEnumerator()
    {
        return Events.GetEnumerator();
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("Kills at -> ");
        Monitor.Enter(Events);
        try
        {
            foreach (var ev in Events)
            {
                sb.Append(ev.Second + ", ");
            }
        }
        finally
        {
            Monitor.Exit(Events);
        }

        return sb.ToString().Trim();
    }

    public void Add(FrameEvent lastEvent)
    {
        Monitor.Enter(Events);
        try
        {
            if (Events.All(a => Math.Abs(a.Second - lastEvent.Second) > 1))
            {
                Events.Add(lastEvent);
            }
        }
        finally
        {
            Monitor.Exit(Events);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Events.GetEnumerator();
    }
}

public class MultiKillEventArgs(FrameEventGroup group)
{
    public FrameEventGroup Group { get; } = group;
}

public static class FrameEventHelpers
{
    private const int SecondsThreshold = 60 * 10;

    public static bool IsCloseToGroup(this FrameEvent @this, FrameEventGroup others)
    {
        return others.Events.Any(other => other.IsCloseTo(@this));
    }


    public static bool IsCloseTo(this FrameEvent @this, FrameEvent other)
    {
        var frameDistance = Math.Abs(other.FrameNumber - @this.FrameNumber);
        return frameDistance < SecondsThreshold;
    }
}

public class FrameEvent
{
    public FrameEvent(string? eventName, int frameNumber, int second, int fps)
    {
        FrameNumber = frameNumber;
        EventName = eventName;
        Second = second;
        FPS = fps;
    }

    public int FrameNumber { get; }
    public int FPS { get; }
    public int Second { get; }
    public string? EventName { get; }
    public bool Processed { get; set; }


    public override string ToString()
    {
        return $"{EventName} {FrameNumber} {Second} {FPS}";
    }
}