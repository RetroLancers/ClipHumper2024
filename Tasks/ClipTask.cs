using ClipHunta2.Tasks.LongTask;
using FFMpegCore;

namespace ClipHunta2.Tasks;

public class ClipTask : LongTask<(string inFile,string outFile,int start, int end)>
{
    public ClipTask(CancellationTokenSource cts) : base(cts)
    {
    }

    /// Clips (fast) mkv with ffmppeg
    /// @param value The input value for the action.
    /// - inFile: The input file path.
    /// - outFile: The output file path.
    /// - start: The start time.
    /// - end: The end time.
    /// @returns A Task representing the asynchronous operation.
    /// /
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