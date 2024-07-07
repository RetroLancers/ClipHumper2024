using System.Collections;
using System.Text;
using FuzzySharp;

namespace ClipHunta2.Tasks;

public class FrameEventGroup : IEnumerable<FrameEvent>
{
    public readonly List<FrameEvent> Events = [];
    public bool Processed { get; set; }


    public IEnumerator<FrameEvent> GetEnumerator()
    {
        return Events.GetEnumerator();
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("Kills at -> ");
        Monitor.Enter(Events);
        try
        {
            foreach (var ev in Events.OrderBy(a => a.Second))
            {
                sb.Append($"{ev.Second} ({ev.Target}), ");
            }
        }
        finally
        {
            Monitor.Exit(Events);
        }

        return sb.ToString().Trim().Trim(',');
    }

    private static bool IsSimilarStringPresent(IEnumerable<string> list, string input)
    {
        const int similarityTolerance = 60; // adjust this as needed

        return list.Select(item => Fuzz.PartialRatio(input, item)).All(partialRatio => partialRatio <= similarityTolerance);
    }

    public void Add(FrameEvent lastEvent)
    {
        Monitor.Enter(Events);
        try
        {
            var names = Events.Where(a => a.Target != null).Select(a => a.Target!.ToLower()).ToArray();
            var passesFuzz = true;
            if (!string.IsNullOrEmpty(lastEvent.Target))
            {
                passesFuzz = IsSimilarStringPresent(names!, lastEvent.Target.ToLower());
            }

            switch (Events.Count)
            {
                case > 0 when !passesFuzz:

                    return;
                default:

                    Events.Add(lastEvent);
                    break;
            }
        }
        finally
        {
            Monitor.Exit(Events);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Events.GetEnumerator();
    }
}