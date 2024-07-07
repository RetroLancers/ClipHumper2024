using ClipHunta2.TaskManagers;
using ClipHunta2.Tasks.LongTask;
using OpenCvSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace ClipHunta2.Tasks;

public class
    ImagePrepperTask : LongTask<(StreamDefinition streamDefinition, byte[] bytes, StreamCaptureType captureType,
    StreamCaptureStatus streamCaptureStatus, int
    frameNumber, int seconds, int fps
    )>
{
    protected override LongTask<(StreamDefinition streamDefinition, byte[] bytes, StreamCaptureType captureType,
        StreamCaptureStatus streamCaptureStatus, int frameNumber, int seconds, int fps)>? GetTop()
    {
        return ImagePrepperTaskManager.GetInstance().GetTopTasker();
    }

    /// <summary>
    /// Preps and cuts images
    /// </summary>
    /// <param name="value">The tuple containing the stream definition, bytes, capture type, capture status, frame number, seconds, and fps.</param>
    protected override async Task _action(
        (StreamDefinition streamDefinition, byte[] bytes, StreamCaptureType captureType, StreamCaptureStatus
            streamCaptureStatus, int
            frameNumber, int seconds,
            int fps
            ) value)
    {
    
        using var mem2 = new MemoryStream();
        (Color averageColor, string dominantPrimaryColor) color2;
        (Color averageColor, string dominantPrimaryColor) color;
        (Color averageColor, string dominantPrimaryColor) color3;
        using (var image = Image.Load(new DecoderOptions(), value.bytes))
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
    
            mem2.Seek(0, SeekOrigin.Begin);
            await image.SaveAsPngAsync(mem2);
        }

 
        mem2.Seek(0, SeekOrigin.Begin);
        using var bottomLeft = Mat.FromImageData(mem2.ToArray(), ImreadModes.Unchanged);
 

        ImageScannerTaskManager.GetInstance().GetLongTasker()?
            .PutInQueue((value.streamDefinition, [color, color2, color3], bottomLeft.ToBytes(), value.captureType, value.streamCaptureStatus,
                value.frameNumber, value.seconds, value.fps));


        value.streamCaptureStatus.IncrementImagesPrepped();
    }

    public void PutInQueue(
        (StreamDefinition streamDefinition, byte[] bytes, StreamCaptureType captureType, StreamCaptureStatus
            streamCaptureStatus, int
            frameNumber, int seconds, int fps) value)
    {
        Put(new LongTaskQueueItem<(StreamDefinition, byte[], StreamCaptureType, StreamCaptureStatus, int, int, int)>(
            value)).Wait();
    }


    public ImagePrepperTask(CancellationTokenSource cts) : base(cts)
    {
    }
}