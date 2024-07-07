namespace ClipHunta2.Tasks;

public class MultiKillEventArgs(FrameEventGroup group)
{
    public FrameEventGroup Group { get; } = group;
}