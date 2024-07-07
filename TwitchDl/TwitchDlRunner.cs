using System.Diagnostics;
using System.Text;
using Serilog;

namespace ClipHunta2.TwitchDl;

public class TwitchDlRunner
{
    public static string? LookUpStream(string streamUrl)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"C:\python310\python3.exe",
                    Arguments = $"TwitchDl/clip.py {streamUrl} ",
                    UseShellExecute = false, RedirectStandardOutput = true,
                    CreateNoWindow = true, RedirectStandardError = true,
                }
            };

            process.Start();
            StringBuilder ssbError = new();
            StringBuilder ssb = new();
            while (!process.StandardOutput.EndOfStream)
            {
                var line = process.StandardOutput.ReadLine();
                if (line == "_start_")
                {
                    ssb.AppendLine(process.StandardOutput.ReadLine());
                }
            }

            while (!process.StandardError.EndOfStream)
            {
                ssbError.AppendLine(process.StandardError.ReadLine());
            }

            process.WaitForExit(30000);

            if (process.HasExited)
            {
                Log.Logger.Information("Process exited for {StreamUrl}", streamUrl);
            }
            else
            {
                Log.Logger.Information("Process Failed exit for {StreamUrl}", streamUrl);
            }

            return ssb.ToString().Trim();
        }
        catch (Exception e)
        {
            Log.Logger.Error("Error starting streamlink: {Message} {Stack}", e.Message, e.StackTrace);
        }

        return null;
    }
}