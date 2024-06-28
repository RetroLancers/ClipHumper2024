using System.Drawing;
using System.Runtime.InteropServices;
using ClipHunta2.Tasks.FrameTesting.OW;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Serilog;
using Tesseract;

namespace ClipHunta2;

public class ImageScannerTask : LongTask<(StreamDefinition streamDefinition,
    byte[] bytes, StreamCaptureType captureType
    , StreamCaptureStatus streamCaptureStatus, int frameNumber, int second,
    int fps)>
{
    protected override LongTask<(StreamDefinition streamDefinition, byte[] bytes, StreamCaptureType captureType,
        StreamCaptureStatus streamCaptureStatus, int frameNumber, int second, int fps)>? GetTop()
    {
        return ImageScannerTaskManager.GetInstance().GetTopTasker();
    }

    protected override async Task _action(
        (StreamDefinition streamDefinition, byte[] bytes, StreamCaptureType
            captureType, StreamCaptureStatus streamCaptureStatus, int frameNumber, int second,
            int fps) value)
    {
        using var pix = Pix.LoadFromMemory(value.bytes);

        var text = await TesseractLongTaskManager.GetInstance().GetLongTasker().GetText(pix);
        if (text?.Trim().Length > 0)
        {
            Bitmap bitmap = PixConverter.ToBitmap(pix);
            Mat mat = bitmap.ToMat();
            Cv2.ImShow($"{value.streamDefinition.StreamerName}", mat);
            Cv2.WaitKey(1);
            Console.WriteLine(text);
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            
            value.streamCaptureStatus.IncrementFinishedCount();
            return;
        }


        EventRouterTaskManager.GetInstance().GetLongTasker()?.Put(
            new LongTaskQueueItem<(StreamDefinition, FrameEvent[], StreamCaptureStatus)>((value.streamDefinition,
                OwFrameTester.GetInstance().TestFrame(text)
                    .Select(s => new FrameEvent(s, value.frameNumber, value.second, value.fps)).ToArray(),
                value.streamCaptureStatus)));

        value.streamCaptureStatus.IncrementImagesScanned();
        text = null;
    }

    public void PutInQueue(
        (StreamDefinition streamDefinition, byte[] bytes, StreamCaptureType captureType, StreamCaptureStatus
            streamCaptureStatus, int frameNumber, int second,
            int fps) value)
    {
        Put(new LongTaskQueueItem<(StreamDefinition, byte[], StreamCaptureType, StreamCaptureStatus, int, int, int)>(
            value)).Wait();
    }

    public ImageScannerTask(CancellationTokenSource cts) : base(cts)
    {
    }
}