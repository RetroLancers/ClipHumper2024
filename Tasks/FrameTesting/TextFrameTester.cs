namespace ClipHunta2.Tasks.FrameTesting;

public abstract class TextFrameTester : FrameTester
{
    protected readonly string EventName;
    protected abstract string[] Lookups();
    protected virtual string[] AvoidWords()
    {
        return [];
    }
    public override bool Test(string text)
    {
        foreach (var lookup in Lookups())
        {
            if (text.IndexOf(lookup, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                bool avoidWordFound = false;
                foreach (var avoidword in AvoidWords())
                {
                    if (text.IndexOf(avoidword, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        avoidWordFound = true;
                        break;
                    }
                }

                if (!avoidWordFound)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public TextFrameTester(string eventName) : base(eventName)
    {
        EventName = eventName;
    }
}