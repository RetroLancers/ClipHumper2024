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


//for ffmpeg
GlobalFFOptions.Configure(new FFOptions { BinaryFolder = "C:\\ProgramData\\chocolatey\\lib\\ffmpeg\\tools\\ffmpeg\\bin", TemporaryFilesFolder = "c:\\tmp" });


ImageScannerTaskManager.GetInstance().AddLongTasker();
EventRouterTaskManager.GetInstance().AddLongTasker();
TesseractLongTaskManager.GetInstance().AddLongTasker();
TesseractLongTaskManager.GetInstance().AddLongTasker();
ImagePrepperTaskManager.GetInstance().AddLongTasker();
ImagePrepperTaskManager.GetInstance().AddLongTasker();
ClipTaskManager.GetInstance().AddLongTasker();
FrameEventHandler.StartHandler(); 
var now = DateTime.Now;
var cancellationTokenSource = new CancellationTokenSource();
var streamCaptureTaskStarterTask =
    new StreamCaptureTaskStarterTask(cancellationTokenSource, "", StreamCaptureType.Clip);
if (args.Length < 1)
{
    Console.WriteLine("Usage: ClipHunta2.exe filename");
    return;
}

var filename = args[0];
if (!File.Exists(filename))
{
    Console.WriteLine("No such file");
    return;
}
var streamStatus = streamCaptureTaskStarterTask.Start(filename, cancellationTokenSource);


while (streamStatus.FinishedCount != streamStatus.FinalFrameCount)
{
    try
    {
        Task.Delay(2000, cancellationTokenSource.Token).Wait(cancellationTokenSource.Token);
    }
    catch (OperationCanceledException e)
    {
        Console.WriteLine("Application was stopped");
        break;
    }

    Console.WriteLine("Image Scanner " + ImageScannerTaskManager.GetInstance());
    Console.WriteLine("Image Prepped" + ImagePrepperTaskManager.GetInstance());
    Console.WriteLine("Tesseract    " + TesseractLongTaskManager.GetInstance());
    Console.WriteLine("Events    " + EventRouterTaskManager.GetInstance());
    Console.WriteLine(streamStatus);
}

foreach (var frameEventGroup in FrameEventHandler.GetFrameEventGroups())
{
    Console.WriteLine(frameEventGroup);
}

cancellationTokenSource.Cancel(false);

var endtime = DateTime.Now;
var elapse = endtime - now;
Console.WriteLine(elapse);
TesseractLongTaskManager.GetInstance().Free();