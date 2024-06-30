using ClipHunta2;
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

#if TrainingData
TrainingDataTaskManager.GetInstance().AddLongTasker();
#endif
ImageScannerTaskManager.GetInstance().AddLongTasker();
// ImageScannerTaskManager.GetInstance().AddLongTasker();
// ImageScannerTaskManager.GetInstance().AddLongTasker();
// ImageScannerTaskManager.GetInstance().AddLongTasker();
EventRouterTaskManager.GetInstance().AddLongTasker();
TesseractLongTaskManager.GetInstance().AddLongTasker();
TesseractLongTaskManager.GetInstance().AddLongTasker();
ImagePrepperTaskManager.GetInstance().AddLongTasker();
ImagePrepperTaskManager.GetInstance().AddLongTasker();
ClipTaskManager.GetInstance().AddLongTasker();
FrameEventHandler.StartHandler();
Console.WriteLine("Hello, World!");
var now = DateTime.Now;
var cancellationTokenSource = new CancellationTokenSource();
StreamCaptureTaskStarterTask streamCaptureTaskStarterTask =
    new StreamCaptureTaskStarterTask(cancellationTokenSource, "GodOfBronze5", StreamCaptureType.Clip);
var streamStatus = streamCaptureTaskStarterTask.Start(@"c:\twitchvods\stream_2024_6_29.mkv", cancellationTokenSource);


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