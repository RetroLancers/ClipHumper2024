using System.Text.RegularExpressions;
using ClipHunta2.Tasks.LongTask;

namespace ClipHunta2.Tasks;

using ColorReport = (SixLabors.ImageSharp.Color averageColor, string dominantPrimaryColor);

public class EventRouterTask : LongTask<(StreamDefinition streamDefinition, string? text, string? portraitText, ColorReport[] dominantColor, int frameNumber, int second,
    int fps,
    StreamCaptureStatus streamCaptureStatus)>
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
    /// <param name="value">A tuple containing the stream definition, text, portrait text, dominant colors, frame number, second, frames per second, and stream capture status.</param>
    /// <returns>Returns a string.</returns>
    protected override async Task<string?> _action(
        (StreamDefinition streamDefinition, string? text, string? portraitText, ColorReport[] dominantColor, int frameNumber, int second,
            int fps, StreamCaptureStatus streamCaptureStatus) value)
    {
      
        var portraitText = value.portraitText;

        FrameEventHandler.SetLastFrameSeenByEventRouter(value.frameNumber);


        if (string.IsNullOrEmpty(portraitText))
        {
            value.streamCaptureStatus.IncrementEventsRouted();
            value.streamCaptureStatus.IncrementFinishedCount();
            return null;
        }

        var deathMatch = IsDeath.Match(portraitText);
        if (deathMatch.Success || (value.second - _lastDeath < 8 && _lastDeath != 0))
        {
            value.streamCaptureStatus.IncrementEventsRouted();
            value.streamCaptureStatus.IncrementFinishedCount();
            if (deathMatch.Success)
            {
                SetLastDeath(value.second);
            }

            return null;
        } 
        
        if (value.dominantColor.Any(color => color.dominantPrimaryColor != "Red"))
        {
            value.streamCaptureStatus.IncrementEventsRouted();
            value.streamCaptureStatus.IncrementFinishedCount();
            return null;
        }
       

        HandlePossibleKillEvent(value, portraitText);


        value.streamCaptureStatus.IncrementEventsRouted();
        value.streamCaptureStatus.IncrementFinishedCount();
        return null;
    }

    /// <summary>
    /// Handles a possible kill event by extracting relevant information from the provided parameters and dispatching a frame event.
    /// </summary>
    /// <param name="value">A tuple containing the stream definition, text, portrait text, dominant colors, frame number, second, frames per second, and stream capture status.</param>
    /// <param name="portraitText">The portrait text to be analyzed.</param>
    private static void HandlePossibleKillEvent(
        (StreamDefinition streamDefinition, string? text, string? portraitText, ColorReport[] dominantColor, int frameNumber, int second, int fps, StreamCaptureStatus streamCaptureStatus)
            value,
        string portraitText)
    {
        var killType = "Kill";
        var killMatch = IsKill.Match(portraitText);
        if (!killMatch.Success)
        {
            killMatch = IsAssist.Match(portraitText);
            if (killMatch.Success)
            {
                killType = "Assist";
            }
        }

        if (!killMatch.Success)
        {
        

            return;
        }


        var target = CleanText.Replace(killMatch.Groups[1].Value, "");
        var leftColor = value.dominantColor[0];
        var rightColor = value.dominantColor[1];
        var trimTarget = target.Trim();

        if (string.IsNullOrEmpty(trimTarget) || trimTarget.Length <= 2) 
            return;
        if (char.IsDigit(trimTarget[0])) 
            return;
        Console.WriteLine($"{trimTarget} , {leftColor.dominantPrimaryColor}, {leftColor.averageColor}, {rightColor.dominantPrimaryColor}, {rightColor.averageColor}");
        var frameEvent = new FrameEvent(killType, value.frameNumber, value.second, value.fps, trimTarget);
        Console.WriteLine($"Dispatching Event: {frameEvent}");
        FrameEventHandler.AddEvent(frameEvent);
   
    }
}

 