using FFMpegCore;

namespace ClipHunta2;

public class ClipTask : LongTask<(string inFile,string outFile,int start, int end)>
{
    public ClipTask(CancellationTokenSource cts) : base(cts)
    {
    }

    protected override async Task _action((string inFile,string outFile,int start, int end) value)
    {
        var (inFile, outFile, start, end) = value;
        try
        {
            await FFMpeg.SubVideoAsync(inFile,
                outFile,
                TimeSpan.FromSeconds(start),
                TimeSpan.FromSeconds(end)
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            
        }
      
        
    }
}