namespace ClipHunta2.StreamLink;

using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using Serilog;
using System;

public class TwitchDlRunner
{
    public static TwitchDl.ClipLookupResult? LookUpStream(string streamUrl)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"-m twitchdl {streamUrl} --json",
                    UseShellExecute = false, RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.Start();

            StringBuilder ssb = new();
            while (!process.StandardOutput.EndOfStream)
            {
                var line = process.StandardOutput.ReadLine();

                ssb.AppendLine(line);
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

            return JsonConvert.DeserializeObject<TwitchDl.ClipLookupResult>(ssb.ToString());
        }
        catch (Exception e)
        {
            Log.Logger.Error("Error starting streamlink: {Message} {Stack}", e.Message, e.StackTrace);
        }

        return null;
    }
}