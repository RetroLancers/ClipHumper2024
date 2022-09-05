namespace ClipHunta2;

public class FrameEvent
{
    public FrameEvent(string eventName, int frameNumber, int second, int fps)
    {
        FrameNumber = frameNumber;
        EventName = eventName;
        Second = second;
        FPS = fps;
    }

    public int FrameNumber { get; }
    public int FPS { get; }
    public int Second { get; }
    public string EventName { get; }

    public override string ToString()
    {
        return $"{EventName} {FrameNumber} {Second} {FPS}";
    }
}