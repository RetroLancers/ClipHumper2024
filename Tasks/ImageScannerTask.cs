using ClipHunta2.TaskManagers;
using ClipHunta2.Tasks.LongTask;
using Tesseract;

namespace ClipHunta2.Tasks;

using ColorReport = (SixLabors.ImageSharp.Color averageColor, string dominantPrimaryColor);

public class ImageScannerTask(CancellationTokenSource cts) : LongTask<(StreamDefinition streamDefinition,
    ColorReport[] dominantColor, byte[] portraitbytes, StreamCaptureType captureType
    , StreamCaptureStatus streamCaptureStatus, int frameNumber, int second,
    int fps)>(cts)
{
    protected override LongTask<(StreamDefinition streamDefinition, ColorReport[] dominantColor, byte[] portraitbytes, StreamCaptureType captureType,
        StreamCaptureStatus streamCaptureStatus, int frameNumber, int second, int fps)>? GetTop()
    {
        return ImageScannerTaskManager.GetInstance().GetTopTasker();
    }


    /// <summary>
    /// Scans the prepped images for text
    /// </summary>
    /// <param name="value">The tuple containing the stream definition, dominant colors, portrait bytes,
    /// capture type, stream capture status, frame number, second, and FPS.</param>
    protected override async Task _action(
        (StreamDefinition streamDefinition, ColorReport[] dominantColor, byte[] portraitbytes, StreamCaptureType
            captureType, StreamCaptureStatus streamCaptureStatus, int frameNumber, int second,
            int fps) value)
    {
        


        using var pixPortrait = Pix.LoadFromMemory(value.portraitbytes);

        var tesseractTask = TesseractLongTaskManager.GetInstance().GetLongTasker();

        var portraitText = await tesseractTask!.GetText(pixPortrait);


        EventRouterTaskManager.GetInstance().GetLongTasker()?.Put(
            new LongTaskQueueItem<(StreamDefinition, string? text, string? portraitText, ColorReport[] dominantColor, int frameNumber, int second,
                int fps, StreamCaptureStatus)>
            ((value.streamDefinition, "", portraitText, value.dominantColor, value.frameNumber, value.second,
                value.fps, value.streamCaptureStatus)));

        value.streamCaptureStatus.IncrementImagesScanned();
    }

    public void PutInQueue(
        (StreamDefinition streamDefinition, ColorReport[] dominantColor, byte[] portraitbytes, StreamCaptureType captureType, StreamCaptureStatus
            streamCaptureStatus, int frameNumber, int second,
            int fps) value)
    {
        Put(new LongTaskQueueItem<(StreamDefinition, ColorReport[], byte[], StreamCaptureType, StreamCaptureStatus, int, int, int)>(
            value)).Wait();
    }
}