using System.Text.RegularExpressions;
using Serilog;

namespace ClipHunta2;

public class EventRouterTask : LongTask<(StreamDefinition streamDefinition, string? text, int frameNumber, int second,
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

    public static string CurrentUsername = "";
    public static Regex _currentAccount = new Regex(@"\(VN\) (\w+) L", RegexOptions.Compiled);
    private static Regex _isKill = new Regex("THATTALLGUY.*>", RegexOptions.Compiled); 
 

    protected override async Task<string?> _action(
        (StreamDefinition streamDefinition, string? text, int frameNumber, int second,
            int fps, StreamCaptureStatus streamCaptureStatus) value)
    {
        var text = value.text;
      
    
        FrameEventHandler.SetLastFrameSeenByEventRouter(value.frameNumber);
        if (!(text?.Trim().Length > 0))
        {
            value.streamCaptureStatus.IncrementEventsRouted();
            value.streamCaptureStatus.IncrementFinishedCount();
            return null;
        }
        //  Bitmap bitmap = PixConverter.ToBitmap(pix);
        // using Mat mat = bitmap.ToMat();
        //  Cv2.ImShow($"{value.streamDefinition.StreamerName}", mat);
        //  Cv2.WaitKey(1);
        if (text.Contains(CurrentUsername) && CurrentUsername.Length > 0)
        {
            //          Console.WriteLine(text.Trim());
        }

        if (CurrentUsername.Length > 0 && _isKill.IsMatch(text))
        {
//                  Console.WriteLine(text);
            Console.WriteLine($"Dispatching Event at Second/Frame: {value.second} / {value.frameNumber}");
            FrameEventHandler.AddEvent(new FrameEvent("Kill", value.frameNumber, value.second, value.fps));
        }
        else
        {
            var match = _currentAccount.Match(text);
            if (match.Success)
            {
                if (CurrentUsername != match.Groups[1].Value && match.Groups[1].Value.Length > 0)
                {
                    CurrentUsername = match.Groups[1].Value;
                    _isKill = new Regex($"{CurrentUsername}.*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    Console.WriteLine($"Username switched to {CurrentUsername}");
                }
            }
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