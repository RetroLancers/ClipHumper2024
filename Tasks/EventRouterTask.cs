using System.Text.RegularExpressions;
using Serilog;

namespace ClipHunta2;

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
    public static Regex _currentAccount = new Regex(@"([A-Z0-9]{3,})\n", RegexOptions.Compiled);
    private static Regex _isKill = new Regex(@"^[3WMX]{0,2}\s?(\w{3,})\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex _isAssist = new Regex(@"ASSIST\s+(\w{3,})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex _isDeath = new Regex(@"LIMINATED", RegexOptions.Compiled);
    private static Regex cleanText = new Regex("[^A-Za-z0-9]", RegexOptions.Compiled);
    private static string[] _forbidden = ["LCONTROL", "LEAVE"];


    private static readonly object DeathLock = new();
    private static int _lastDeath = 0;

    public static void SetLastDeath(int second)
    {
        lock (DeathLock)
        {
            _lastDeath = second;
        }
    }

    protected override async Task<string?> _action(
        (StreamDefinition streamDefinition, string? text, string? portraitText, ColorReport[] dominantColor, int frameNumber, int second,
            int fps, StreamCaptureStatus streamCaptureStatus) value)
    {
        var text = value.text;
        var portraitText = value.portraitText;

        FrameEventHandler.SetLastFrameSeenByEventRouter(value.frameNumber);


        if (string.IsNullOrEmpty(portraitText))
        {
            value.streamCaptureStatus.IncrementEventsRouted();
            value.streamCaptureStatus.IncrementFinishedCount();
            return null;
        }

        var deathMatch = _isDeath.Match(portraitText);
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
//       && color.averageColor.ToHex().EndsWith("FF")
        if (!value.dominantColor.All(color => color.dominantPrimaryColor == "Red" 
                                       )
            )
        {
            value.streamCaptureStatus.IncrementEventsRouted();
            value.streamCaptureStatus.IncrementFinishedCount();
            return null;
        }
        // var leftColor = value.dominantColor[0];
        // var rightColor = value.dominantColor[1];
        // if (leftColor.dominantPrimaryColor != "Red" || !leftColor.averageColor.ToHex().EndsWith("FF"))
        // {
        //     value.streamCaptureStatus.IncrementEventsRouted();
        //     value.streamCaptureStatus.IncrementFinishedCount();
        //     return null;
        // }
        // if (rightColor.dominantPrimaryColor != "Red" || !rightColor.averageColor.ToHex().EndsWith("FF"))
        // {
        //     value.streamCaptureStatus.IncrementEventsRouted();
        //     value.streamCaptureStatus.IncrementFinishedCount();
        //     return null;
        // }

        HandlePossibleKillEvent(value, portraitText);


        value.streamCaptureStatus.IncrementEventsRouted();
        value.streamCaptureStatus.IncrementFinishedCount();
        return null;
    }

    private static void HandlePossibleKillEvent(
        (StreamDefinition streamDefinition, string? text, string? portraitText, ColorReport[] dominantColor, int frameNumber, int second, int fps, StreamCaptureStatus streamCaptureStatus)
            value,
        string portraitText)
    {
        var killType = "Kill";
        var killMatch = _isKill.Match(portraitText);
        if (!killMatch.Success)
        {
            killMatch = _isAssist.Match(portraitText);
            if (killMatch.Success)
            {
                killType = "Assist";
            }
        }

        if (!killMatch.Success)
        {
            if (portraitText.Trim().Length > 0)
            {
                // Console.WriteLine(portraitText.Trim());
            }

            return;
        }


        var target = cleanText.Replace(killMatch.Groups[1].Value, "");
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
        // else
        // {
        //     if (portraitText.Trim().Length > 0)
        //     {
        //         Console.WriteLine(portraitText.Trim());
        //     }
        // }
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