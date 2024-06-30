using System.Collections.Concurrent;
using System.ComponentModel;

namespace ClipHunta2;

public class FrameEventHandler
{
    private static readonly ConcurrentQueue<FrameEvent> FrameEvents = [];
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
        while (true)
        {
            task();
            Thread.Sleep(1000);
        }
    }

    private static void task()
    {
        List<FrameEvent> sortedEvents;
        if (FrameEvents.IsEmpty)
        {
            return;
        }


        while (!FrameEvents.IsEmpty)
        {
            if (!FrameEvents.TryDequeue(out var frameEvent))
            {
                break;
            }

            frameEvent.Processed = true;
            var closeGroup = FrameEventGroups.FirstOrDefault(a => frameEvent.IsCloseToGroup(a));
            if (closeGroup != null)
            {
                closeGroup.Add(frameEvent);
                if (closeGroup.Events.Count >= 2 && !closeGroup.Processed)
                {
                    Console.WriteLine($"Dispatching Clip 2: {closeGroup}");
                    _ = StartDelayedMultiEvent(closeGroup);
                }

                //      Console.WriteLine($"Kill Event at {frameEvent.Second}, added to previous group now size {closeGroup.Events.Count}");
                continue;
            }

            FrameEventGroup group = [frameEvent];
            FrameEventGroups.Add(group);
        }
    }

    static async Task StartDelayedMultiEvent(FrameEventGroup closeGroup)
    {
        await Task.Delay(TimeSpan.FromSeconds(10)); //waits for 1 minute
        OnMultiKill?.Invoke(new MultiKillEventArgs(closeGroup));
    }

    public static void AddEvent(FrameEvent frameEvent)
    {
        FrameEvents.Enqueue(frameEvent);
    }
}