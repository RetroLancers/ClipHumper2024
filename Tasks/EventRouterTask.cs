using System.Text.RegularExpressions;
using ClipHunta2.Tasks.LongTask;

namespace ClipHunta2.Tasks;

using ColorReport = (SixLabors.ImageSharp.Color averageColor, string dominantPrimaryColor);

public class EventRouterTask : LongTask<EventRouterPayload>
{
    public EventRouterTask(CancellationTokenSource cts) : base(cts)
    {
    }

    protected bool IsSameEvent(FrameEvent a, FrameEvent b)
    {
        return IsSameSecond(a, b);
    }


    protected bool IsSameSecond(FrameEvent a, FrameEvent b)
    {
        return b.EventName == a.EventName && b.Second == a.Second;
    }

    public static string CurrentUsername = "PHR34KZ";
    public static Regex CurrentAccount = new(@"([A-Z0-9]{3,})\n", RegexOptions.Compiled);
    private static readonly Regex IsKill = new(@"^[3WMX]{0,2}\s?(\w{3,})\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex IsAssist = new(@"ASSIST\s+(\w{3,})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex IsHeal = new(@"HEALED\s+(\w{3,})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex IsSaved = new(@"SAVED\s+(\w{3,})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex IsDeath = new(@"LIMINATED", RegexOptions.Compiled);
    private static readonly Regex CleanText = new("[^A-Za-z0-9]", RegexOptions.Compiled);


    private static readonly object DeathLock = new();
    private static int _lastDeath;

    public static void SetLastDeath(int second)
    {
        lock (DeathLock)
        {
            _lastDeath = second;
        }
    }

    /// <summary>
    /// Executes the action for the EventRouterTask.
    /// </summary>
    /// <param name="payload">The payload containing event data.</param>
    /// <returns>Returns a string.</returns>
    protected override async Task<string?> _action(EventRouterPayload payload)
    {
        var portraitText = payload.PortraitText;

        FrameEventHandler.SetLastFrameSeenByEventRouter(payload.FrameNumber);


        if (string.IsNullOrEmpty(portraitText))
        {
            payload.StreamCaptureStatus.IncrementEventsRouted();
            payload.StreamCaptureStatus.IncrementFinishedCount();
            return null;
        }

        var deathMatch = IsDeath.Match(portraitText);
        if (deathMatch.Success || (payload.Second - _lastDeath < 8 && _lastDeath != 0))
        {
            payload.StreamCaptureStatus.IncrementEventsRouted();
            payload.StreamCaptureStatus.IncrementFinishedCount();
            if (deathMatch.Success)
            {
                SetLastDeath(payload.Second);
            }

            return null;
        }

        var isHealMatch = IsHeal.Match(portraitText);
        if (isHealMatch.Success)
        {
            var frameEvent = new FrameEvent("HEAL", payload.FrameNumber, payload.Second, payload.Fps, "STREAMER");
            Console.WriteLine($"Dispatching Event: {frameEvent}");
            FrameEventHandler.AddEvent(frameEvent);
        }

        var isSavedMatch = IsSaved.Match(portraitText);
        if (isSavedMatch.Success)
        {
            var frameEvent = new FrameEvent("SAVED", payload.FrameNumber, payload.Second, payload.Fps, "STREAMER");
            Console.WriteLine($"Dispatching Event: {frameEvent}");
            FrameEventHandler.AddEvent(frameEvent);
        }

        if (payload.DominantColors.Any(color => color.dominantPrimaryColor != "Red"))
        {
            payload.StreamCaptureStatus.IncrementEventsRouted();
            payload.StreamCaptureStatus.IncrementFinishedCount();
            return null;
        }


        HandlePossibleKillEvent(payload, portraitText);


        payload.StreamCaptureStatus.IncrementEventsRouted();
        payload.StreamCaptureStatus.IncrementFinishedCount();
        return null;
    }

    /// <summary>
    /// Handles a possible kill event by extracting relevant information from the provided parameters and dispatching a frame event.
    /// </summary>
    /// <param name="payload">The event payload.</param>
    /// <param name="portraitText">The portrait text to be analyzed.</param>
    private static void HandlePossibleKillEvent(EventRouterPayload payload, string portraitText)
    {
        var killType = "Kill";
        var killMatch = IsKill.Match(portraitText);
        if (!killMatch.Success)
        {
            killMatch = IsAssist.Match(portraitText);
            if (!killMatch.Success)
            {
                return;
            }

            killType = "Assist";
        }


        var target = CleanText.Replace(killMatch.Groups[1].Value, "");
        var leftColor = payload.DominantColors[0];
        var rightColor = payload.DominantColors[1];
        var trimTarget = target.Trim();

        if (string.IsNullOrEmpty(trimTarget) || trimTarget.Length <= 2)
            return;
        if (char.IsDigit(trimTarget[0]))
            return;
        Console.WriteLine($"{trimTarget} , {leftColor.dominantPrimaryColor}, {leftColor.averageColor}, {rightColor.dominantPrimaryColor}, {rightColor.averageColor}");
        var frameEvent = new FrameEvent(killType, payload.FrameNumber, payload.Second, payload.Fps, trimTarget);
        Console.WriteLine($"Dispatching Event: {frameEvent}");
        FrameEventHandler.AddEvent(frameEvent);
    }
}