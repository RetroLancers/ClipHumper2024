using ClipHunta2;
using ClipHunta2.TaskManagers;
using ClipHunta2.Tasks;
using FFMpegCore;
using OpenCvSharp;
using Serilog;
using Tesseract;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/testing.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

async Task Main(string[] args)
{
    try
    {
//for ffmpeg
        var ffmpeg = System.Environment.GetEnvironmentVariable("FFMPEG_PATH") ??
                     @"C:\ProgramData\chocolatey\lib\ffmpeg\tools\ffmpeg\bin";
        var tmpPath = System.Environment.GetEnvironmentVariable("TMP_PATH") ?? @"C:\tmp";
        GlobalFFOptions.Configure(new FFOptions { BinaryFolder = ffmpeg, TemporaryFilesFolder = tmpPath });


        ImageScannerTaskManager.GetInstance().AddLongTasker();
        EventRouterTaskManager.GetInstance().AddLongTasker();
        TesseractLongTaskManager.GetInstance().AddLongTasker();
        TesseractLongTaskManager.GetInstance().AddLongTasker();
        ImagePrepperTaskManager.GetInstance().AddLongTasker();
        ImagePrepperTaskManager.GetInstance().AddLongTasker();
        ClipTaskManager.GetInstance().AddLongTasker();
        FrameEventHandler.StartHandler();
        var now = DateTime.Now;
        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            var streamCaptureTaskStarterTask =
                new StreamCaptureTaskStarterTask(cancellationTokenSource, "", StreamCaptureType.Clip);
            if (args.Length < 1)
        {
            Log.Information("Usage: ClipHunta2.exe filename");
            return;
        }

        var filename = args[0];
        if (!File.Exists(filename))
        {
            Log.Information("No such file: {FileName}", filename);
            return;
        }

        var streamStatus = streamCaptureTaskStarterTask.Start(filename, cancellationTokenSource);


        while (streamStatus.FinishedCount != streamStatus.FinalFrameCount)
        {
            try
            {
                await Task.Delay(2000, cancellationTokenSource.Token);
            }
            catch (OperationCanceledException e)
            {
                Log.Information("Application was stopped");
                break;
            }

            Log.Information("Image Scanner {ImageScannerStatus}", ImageScannerTaskManager.GetInstance());
            Log.Information("Image Prepped {ImagePrepperStatus}", ImagePrepperTaskManager.GetInstance());
            Log.Information("Tesseract {TesseractStatus}", TesseractLongTaskManager.GetInstance());
            Log.Information("Events {EventRouterStatus}", EventRouterTaskManager.GetInstance());
            Log.Information("Stream Status {StreamOverallStatus}", streamStatus);
        }

        foreach (var frameEventGroup in FrameEventHandler.GetFrameEventGroups())
        {
            Log.Information("Frame Event Group: {FrameEventGroup}", frameEventGroup);
        }

        cancellationTokenSource.Cancel(false);

        var endtime = DateTime.Now;
        var elapse = endtime - now;
        Log.Information("Total elapsed time: {ElapsedTime}", elapse);
        TesseractLongTaskManager.GetInstance().Free();
        } // End of using block for cancellationTokenSource
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "An unhandled exception occurred during application execution.");
    }
    finally
    {
        await Log.CloseAndFlushAsync();
    }
}

// Call the async Main method
await Main(args);