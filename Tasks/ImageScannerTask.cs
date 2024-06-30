using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ClipHunta2.Tasks.FrameTesting.OW;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Serilog;
using Tesseract;

namespace ClipHunta2;
using ColorReport = (SixLabors.ImageSharp.Color averageColor, string dominantPrimaryColor);
public class ImageScannerTask : LongTask<(StreamDefinition streamDefinition,
    ColorReport[] dominantColor, byte[] portraitbytes, StreamCaptureType captureType
    , StreamCaptureStatus streamCaptureStatus, int frameNumber, int second,
    int fps)>
{
    protected override LongTask<(StreamDefinition streamDefinition,ColorReport[] dominantColor, byte[] portraitbytes, StreamCaptureType captureType,
        StreamCaptureStatus streamCaptureStatus, int frameNumber, int second, int fps)>? GetTop()
    {
        return ImageScannerTaskManager.GetInstance().GetTopTasker();
    }


    protected override async Task _action(
        (StreamDefinition streamDefinition, ColorReport[] dominantColor, byte[] portraitbytes, StreamCaptureType
            captureType, StreamCaptureStatus streamCaptureStatus, int frameNumber, int second,
            int fps) value)
    {
        if (value.dominantColor[0].dominantPrimaryColor != "Red")
        {
         //   value.streamCaptureStatus.IncrementImagesScanned();
      //      value.streamCaptureStatus.IncrementFinishedCount();
       //     return  ;
        }

        //  using var pix = Pix.LoadFromMemory(value.bytes);
        using var pixPortrait = Pix.LoadFromMemory(value.portraitbytes);
       
        var tesseractTask = TesseractLongTaskManager.GetInstance().GetLongTasker();
       // var text =   await tesseractTask?.GetText(pix);
       
       //Console.WriteLine(value.dominantColor);
        var portraitText = await tesseractTask?.GetText(pixPortrait);
        // Bitmap bitmap = PixConverter.ToBitmap(pixPortrait);
        // using Mat mat = bitmap.ToMat();
        // Cv2.ImShow($"{value.streamDefinition.StreamerName}", mat);
        // Cv2.WaitKey(1);
        // if (string.IsNullOrWhiteSpace(text))
        // {
        //     value.streamCaptureStatus.IncrementFinishedCount();
        //     return;
        // }


        EventRouterTaskManager.GetInstance().GetLongTasker()?.Put(
            new LongTaskQueueItem<(StreamDefinition, string? text,string? portraitText,ColorReport[] dominantColor, int frameNumber, int second,
                int fps, StreamCaptureStatus)>
            ((value.streamDefinition, "",portraitText,value.dominantColor, value.frameNumber, value.second,
                value.fps, value.streamCaptureStatus)));

        value.streamCaptureStatus.IncrementImagesScanned();
       // text = null;
    }

    public void PutInQueue(
        (StreamDefinition streamDefinition,ColorReport[] dominantColor, byte[] portraitbytes, StreamCaptureType captureType, StreamCaptureStatus
            streamCaptureStatus, int frameNumber, int second,
            int fps) value)
    {
        Put(new LongTaskQueueItem<(StreamDefinition,ColorReport[], byte[]  , StreamCaptureType, StreamCaptureStatus, int, int, int)>(
            value)).Wait();
    }

    public ImageScannerTask(CancellationTokenSource cts) : base(cts)
    {
    }
}