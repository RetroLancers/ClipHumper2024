namespace ClipHunta2;

public abstract class FrameTester
{
    private readonly string _eventName;

    public FrameTester(string eventName)
    {
        _eventName = eventName;
    }

    public abstract string GetName();
    public abstract bool Test(string text);
}