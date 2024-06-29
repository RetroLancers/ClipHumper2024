using System.ComponentModel;

namespace ClipHunta2;

public class FrameEventHandler
{
    private static readonly List<FrameEvent> FrameEvents = [];
    private const int SecondsThreshold = 60 * 10;
    private static int _lastFrameNumber = 0;
    private static readonly List<FrameEventGroup> FrameEventGroups = [];
    private static readonly BackgroundWorker BackWorker = new();

    public static List<FrameEventGroup> GetFrameEventGroups()
    {
        return FrameEventGroups;
    }
    public delegate void OnMultiKillDelegate(MultiKillEventArgs e);

    public static event OnMultiKillDelegate? OnMultiKill;

    public static void SetLastFrameSeenByEventRouter(int frameNumber)
    {
        _lastFrameNumber = frameNumber;
    }

    public static void StartHandler()
    {
        BackWorker.DoWork += ProcessEvents;
        BackWorker.RunWorkerCompleted += RestartWorker;
        BackWorker.RunWorkerAsync();
    }

    private static void RestartWorker(object? sender, RunWorkerCompletedEventArgs e)
    {
        Task.Delay(1000).Wait();
        BackWorker.RunWorkerAsync();
    }

    private static void ProcessEvents(object? o, DoWorkEventArgs b)
    {
        List<FrameEvent> sortedEvents;
        Monitor.Enter(FrameEvents);

        try
        {
            if (FrameEvents.Count == 0)
            {
                return;
            }

            sortedEvents = [..FrameEvents.OrderByDescending(a => a.FrameNumber).Where(a => !a.Processed)];
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return;
        }
        finally
        {
            Monitor.Exit(FrameEvents);
        }

        var lastEvent = sortedEvents.First();
        sortedEvents.RemoveAt(0);
        var frameDistance = Math.Abs(_lastFrameNumber - lastEvent.FrameNumber);

        if (frameDistance < SecondsThreshold)
        {
            return;
        }


        FrameEventGroup group = [lastEvent];
        while (sortedEvents.Count > 0)
        {
            var frameEvent = sortedEvents.First();

            frameEvent.Processed = true;
            sortedEvents.RemoveAt(0);
            if (frameEvent.IsCloseToGroup(group))
            {
                group.Add(frameEvent);
           //     Console.WriteLine($"Kill Event at {frameEvent.Second}, added to front group");
                if (group.Events.Count >= 2)
                {
                    OnMultiKill?.Invoke(new MultiKillEventArgs(group));
                }

                continue;
            }

            var closeGroup = FrameEventGroups.FirstOrDefault(a => frameEvent.IsCloseToGroup(a));
            if (closeGroup != null)
            {
                
                closeGroup.Add(frameEvent);
                if (closeGroup.Events.Count >= 2)
                {
                    OnMultiKill?.Invoke(new MultiKillEventArgs(closeGroup));
                }
          //      Console.WriteLine($"Kill Event at {frameEvent.Second}, added to previous group now size {closeGroup.Events.Count}");
                continue;
            }


            FrameEventGroups.Add(group);
         //   Console.WriteLine($"Kill Event Group Size: {group.Events.Count}");
            group = [frameEvent];
        }
    }

    public static void AddEvent(FrameEvent frameEvent)
    {
        Monitor.Enter(FrameEvents);
        try
        {
            FrameEvents.Add(frameEvent);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            Monitor.Exit(FrameEvents);
        }
    }
}