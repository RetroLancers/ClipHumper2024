using ClipHunta2.TaskManagers;
using ClipHunta2.Tasks.LongTask;
using OpenCvSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace ClipHunta2.Tasks;

public record ImagePrepperPayload(
    StreamDefinition StreamDefinition,
    byte[] Bytes,
    StreamCaptureType CaptureType,
    StreamCaptureStatus StreamCaptureStatus,
    int FrameNumber,
    int Seconds,
    int Fps
);

public class ImagePrepperTask : LongTask<ImagePrepperPayload>
{
    protected override LongTask<ImagePrepperPayload>? GetTop()
    {
        return ImagePrepperTaskManager.GetInstance().GetTopTasker();
    }

    /// <summary>
    /// Preps and cuts images
    /// </summary>
    /// <param name="payload">The payload containing the stream definition, bytes, capture type, capture status, frame number, seconds, and fps.</param>
    protected override async Task _action(ImagePrepperPayload payload)
    {
    
        using var mem2 = new MemoryStream();
        (Color averageColor, string dominantPrimaryColor) color2;
        (Color averageColor, string dominantPrimaryColor) color;
        (Color averageColor, string dominantPrimaryColor) color3;
        using (var image = Image.Load(new DecoderOptions(), payload.Bytes))
        {
            var cropWidth = image.Width / 8;
            var cropHeight = (image.Height / 16) + 15;
            var cropStarty = image.Height / 2 + 180;
            var cropStartx = image.Width / 2 - 100;
            var cropArea = new Rectangle(cropStartx, cropStarty, cropWidth, cropHeight);
            image.Mutate(x => x.Crop(cropArea).Pad(cropWidth + 20, cropHeight + 20, Color.White));
            await image.SaveAsPngAsync(mem2);
            mem2.Seek(0, SeekOrigin.Begin);
            var imageBuffer = mem2.ToArray();
            color = ImageColorAnalyzer.AnalyzeImageCenter(imageBuffer);
            color2 = ImageColorAnalyzer.AnalyzeImageCenter2(imageBuffer);
            color3 = ImageColorAnalyzer.AnalyzeImageCenter3(imageBuffer);
        }
 
        // mem2 contains the image data from the last SaveAsPngAsync
        // No need to save again, use imageBuffer directly for Mat
        using var ocrInputMat = Mat.FromImageData(imageBuffer, ImreadModes.Unchanged);
 

        ImageScannerTaskManager.GetInstance().GetLongTasker()?
            .PutInQueue((payload.StreamDefinition, [color, color2, color3], ocrInputMat.ToBytes(), payload.CaptureType, payload.StreamCaptureStatus,
                payload.FrameNumber, payload.Seconds, payload.Fps));


        payload.StreamCaptureStatus.IncrementImagesPrepped();
    }

    public void PutInQueue(ImagePrepperPayload payload)
    {
        Put(new LongTaskQueueItem<ImagePrepperPayload>(payload)).Wait();
    }

    public ImagePrepperTask(CancellationTokenSource cts) : base(cts)
    {
    }
}