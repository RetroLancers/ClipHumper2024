namespace ClipHunta2.Tasks;

public class FrameEvent
{
    public string? Target { get; }

    public FrameEvent(string? eventName, int frameNumber, int second, int fps, string? target = null)
    {
        Target = target;
        FrameNumber = frameNumber;
        EventName = eventName;
        Second = second;
        Fps = fps;
    }

    public int FrameNumber { get; }
    public int Fps { get; }
    public int Second { get; }
    public string? EventName { get; }
    public bool Processed { get; set; }


    public override string ToString()
    {
        return $"{EventName} {FrameNumber} {Second} {Fps} -> ({Target})";
    }
}