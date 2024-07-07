namespace ClipHunta2.Tasks;

public static class FrameEventHelpers
{
    private const int SecondsThreshold = 60 * 7;

    public static bool IsCloseToGroup(this FrameEvent @this, FrameEventGroup others)
    {
        return others.Events.Any(other => other.IsCloseTo(@this));
    }


    private static bool IsCloseTo(this FrameEvent @this, FrameEvent other)
    {
        var frameDistance = Math.Abs(other.FrameNumber - @this.FrameNumber);
        return frameDistance < SecondsThreshold;
    }
}