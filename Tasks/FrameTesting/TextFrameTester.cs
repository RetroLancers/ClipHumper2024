namespace ClipHunta2;

public abstract class TextFrameTester : FrameTester
{
    protected readonly string _eventName;
    protected abstract string[] Lookups();
    protected virtual string[] AvoidWords()
    {
        return Array.Empty<string>();
    }
    public override bool Test(string text)
    {
        foreach (var lookup in Lookups())
        {
            if (text.IndexOf(lookup, StringComparison.Ordinal) >= 0)
            {
                foreach(var avoidword in AvoidWords())
                {
                    if(text.IndexOf(avoidword,StringComparison.Ordinal) >= 0)
                    {
                        continue;
                    }
                }
                return true;
            }
        }

        return false;
    }

    public TextFrameTester(string eventName) : base(eventName)
    {
        _eventName = eventName;
    }
}