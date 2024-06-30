using System.Collections;
using System.Text;
using FuzzySharp;

namespace ClipHunta2;

using System;
 

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
            foreach (var ev in Events.OrderBy(a => a.Second))
            {
                sb.Append($"{ev.Second} ({ev.Target}), ");
            }
        }
        finally
        {
            Monitor.Exit(Events);
        }

        return sb.ToString().Trim().Trim(',');
    }

    static bool IsSimilarStringPresent(IEnumerable<string> list, string input)
    {
        const int similarityTolerance = 60; // adjust this as needed

        return list.Select(item => Fuzz.PartialRatio(input, item)).All(partialRatio => partialRatio <= similarityTolerance);
    }

    public void Add(FrameEvent lastEvent)
    {
        Monitor.Enter(Events);
        try
        {
            var names = Events.Where(a => a.Target != null).Select(a => a.Target!.ToLower()).ToArray();
            var passesFuzz = true;
            if (!string.IsNullOrEmpty(lastEvent.Target))
            {
                passesFuzz = IsSimilarStringPresent(names!, lastEvent.Target.ToLower());
            }

            var frameEvents = Events.Where(a => a.Target != null).ToArray();
            switch (Events.Count)
            {
                // case > 0 when !Events.All(a => Math.Abs(a.Second - lastEvent.Second) > 1):
                //     Console.WriteLine($"Skipping event - To Close : {lastEvent}");
                //     return;
                case > 0 when   !passesFuzz:
                    //Console.WriteLine($"Skipping event - name seen : {lastEvent}");
                    return;
                default:
                    //Console.WriteLine($"Adding event : {lastEvent}");
                    Events.Add(lastEvent);
                    break;
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
    private const int SecondsThreshold = 60 * 7;

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
    public string? Target { get; }

    public FrameEvent(string? eventName, int frameNumber, int second, int fps, string? target = null)
    {
        Target = target;
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
        return $"{EventName} {FrameNumber} {Second} {FPS} -> ({Target})";
    }
}