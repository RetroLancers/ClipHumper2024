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
ImageScannerTaskManager.GetInstance().AddLongTasker();
ImageScannerTaskManager.GetInstance().AddLongTasker();
ImageScannerTaskManager.GetInstance().AddLongTasker();
EventRouterTaskManager.GetInstance().AddLongTasker();
TesseractLongTaskManager.GetInstance().AddLongTasker();
TesseractLongTaskManager.GetInstance().AddLongTasker();
ImagePrepperTaskManager.GetInstance().AddLongTasker();
ImagePrepperTaskManager.GetInstance().AddLongTasker();

Console.WriteLine("Hello, World!");
var now = DateTime.Now;
var cancellationTokenSource = new CancellationTokenSource();
StreamCaptureTaskStarterTask streamCaptureTaskStarterTask =
    new StreamCaptureTaskStarterTask(cancellationTokenSource, "GodOfBronze5", StreamCaptureType.Clip);
var streamStatus = streamCaptureTaskStarterTask.Start("https://www.twitch.tv/videos/2180035368");



while (streamStatus.FinishedCount != streamStatus.FinalFrameCount)
{
    Task.Delay(2000, cancellationTokenSource.Token).Wait(cancellationTokenSource.Token);
    // Console.WriteLine(streamStatus);
    // Console.WriteLine("Image Scanner " + ImageScannerTaskManager.GetInstance());
    // Console.WriteLine("Image Prepped" + ImagePrepperTaskManager.GetInstance());
    //Console.WriteLine("Tesseract    " + TesseractLongTaskManager.GetInstance());
}

cancellationTokenSource.Cancel(false);


var items = EventRouterTask.EventsrecvReverent.OrderBy(A => A.frameEvent.Second).GroupBy(a => a.frameEvent.EventName).ToDictionary(a => a.Key);


foreach ( string eventName in items.Keys)
{
    List<int> removeIndex = new();
    var evented = items[eventName].GroupBy(a => a.frameEvent.Second).Select(a => a.First()).ToList();
    var removing = false;

    var removeEnd = 0;
    for (int i = 0; i < evented.Count; i++)
    {
        (StreamDefinition streamDefinition, FrameEvent frameEvent) value = evented[i];
        if (removing)
        {
            if(removeEnd < value.frameEvent.Second)
            {
                removing = false;
            }
            else
            {
                removeIndex.Add(i);
            }

        }

        if(value.frameEvent.EventName == "elimed")
        {
            removing = true;

            removeEnd = value.frameEvent.Second + 8;
        }
    }
    foreach(var index in removeIndex.OrderByDescending(a => a))
    {
        evented.RemoveAt(index);
    }
    
    foreach (var value in evented) {

        Console.WriteLine($"{value.frameEvent.EventName} {value.frameEvent.Second} {value.streamDefinition.StreamerName}");
    }
    //Console.WriteLine(valueTuple.frameEvents.Select(a => a.ToString()).ToArray());
}

var endtime = DateTime.Now;
var elapse = endtime - now;
Console.WriteLine(elapse);
TesseractLongTaskManager.GetInstance().Free();