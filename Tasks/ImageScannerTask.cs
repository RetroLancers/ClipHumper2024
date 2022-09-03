using Serilog;
using Tesseract;

namespace ClipHunta2;

public class ImageScannerTask : LongTask<Tuple<string, byte[]>>
{
    protected override async Task _action(Tuple<string, byte[]> value)
    {
        var tesseractTask = TesseractLongTaskManager.GetInstance().GetLongTasker();
        using var pix = Pix.LoadFromMemory(value.Item2);
        var text = await tesseractTask.PutAndGet(pix);
        if (text == null)
        {
            Log.Logger.Debug("Got empty text");
            return;
        }

        var events = new List<string>();
        foreach (var key in look_ups.Keys)
        {
            for (var index = 0; index < look_ups[key].Length; index++)
            {
                var textLookup = look_ups[key][index];
                if (text.IndexOf(textLookup, StringComparison.Ordinal) < 0) continue;
                events.Add(key);
                break;
            }
        }

        var retval = events.ToArray();
        events.Clear();
        events = null;
        // EventEmitterTaskManager.GetInstance().GetLongTasker().Put(
        //     new LongTaskQueueItem<Tuple<string, string[]>>(new Tuple<string, string[]>(value.Item1, retval)));
        return;
    }

    public void PutInQueue(Tuple<string, byte[]> value)
    {
        Put(new LongTaskQueueItem<Tuple<string, byte[]>>(value)).Wait();
    }

    public ImageScannerTask(CancellationTokenSource cts) : base(cts)
    {
    }

    static Dictionary<string, string[]> look_ups = new Dictionary<string, string[]>()
    {
        { "elim", new string[] { "ELIMINATED", "NATED", "MNATED", "TED" } }
    };
}