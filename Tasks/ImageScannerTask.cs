using ClipHunta2.TaskManagers;
using ClipHunta2.Tasks.LongTask;
using Tesseract;

namespace ClipHunta2.Tasks;

using ColorReport = (SixLabors.ImageSharp.Color averageColor, string dominantPrimaryColor);

public record ImageScannerPayload(
    StreamDefinition StreamDefinition,
    ColorReport[] DominantColors,
    byte[] PortraitBytes,
    StreamCaptureType CaptureType,
    StreamCaptureStatus StreamCaptureStatus,
    int FrameNumber,
    int Second,
    int Fps
);

public record EventRouterPayload(
    StreamDefinition StreamDefinition,
    string? Text,
    string? PortraitText,
    ColorReport[] DominantColors,
    int FrameNumber,
    int Second,
    int Fps,
    StreamCaptureStatus StreamCaptureStatus
);

public class ImageScannerTask(CancellationTokenSource cts) : LongTask<ImageScannerPayload>(cts)
{
    protected override LongTask<ImageScannerPayload>? GetTop()
    {
        return ImageScannerTaskManager.GetInstance().GetTopTasker();
    }


    /// <summary>
    /// Scans the prepped images for text
    /// </summary>
    /// <param name="payload">The payload containing the stream definition, dominant colors, portrait bytes,
    /// capture type, stream capture status, frame number, second, and FPS.</param>
    protected override async Task _action(ImageScannerPayload payload)
    {
        


        using var pixPortrait = Pix.LoadFromMemory(payload.PortraitBytes);

        var tesseractTask = TesseractLongTaskManager.GetInstance().GetLongTasker();

        var portraitText = await tesseractTask!.GetText(pixPortrait);

        var eventRouterPayload = new EventRouterPayload(
            payload.StreamDefinition,
            "", // Assuming Text is intentionally empty here
            portraitText,
            payload.DominantColors,
            payload.FrameNumber,
            payload.Second,
            payload.Fps,
            payload.StreamCaptureStatus
        );

        EventRouterTaskManager.GetInstance().GetLongTasker()?.Put(
            new LongTaskQueueItem<EventRouterPayload>(eventRouterPayload));

        payload.StreamCaptureStatus.IncrementImagesScanned();
    }

    public void PutInQueue(ImageScannerPayload payload)
    {
        Put(new LongTaskQueueItem<ImageScannerPayload>(payload)).Wait();
    }
}