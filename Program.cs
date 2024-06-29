using ClipHunta2;
using OpenCvSharp;
using Serilog;
using Tesseract;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/testing.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();


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
FrameEventHandler.OnMultiKill += (args) =>
{
    var group = args.Group;
    if (group.Processed) return;
    group.Processed = true;
    Console.WriteLine(group);

};
FrameEventHandler.StartHandler();
Console.WriteLine("Hello, World!");
var now = DateTime.Now;
var cancellationTokenSource = new CancellationTokenSource();
StreamCaptureTaskStarterTask streamCaptureTaskStarterTask =
    new StreamCaptureTaskStarterTask(cancellationTokenSource, "GodOfBronze5", StreamCaptureType.Clip);
var streamStatus = streamCaptureTaskStarterTask.Start("https://www.twitch.tv/videos/2180035368");


while (streamStatus.FinishedCount != streamStatus.FinalFrameCount)
{
    Task.Delay(2000, cancellationTokenSource.Token).Wait(cancellationTokenSource.Token);
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