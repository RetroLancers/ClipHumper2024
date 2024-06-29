using OpenCvSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using Tesseract;
using Size = OpenCvSharp.Size;

namespace ClipHunta2;

public class
    ImagePrepperTask : LongTask<(StreamDefinition streamDefinition, byte[] bytes, StreamCaptureType captureType,
    StreamCaptureStatus streamCaptureStatus, int
    frameNumber, int seconds, int fps
    )>
{
    private readonly PngDecoder _pngDecoder;

    protected override LongTask<(StreamDefinition streamDefinition, byte[] bytes, StreamCaptureType captureType,
        StreamCaptureStatus streamCaptureStatus, int frameNumber, int seconds, int fps)>? GetTop()
    {
        return ImagePrepperTaskManager.GetInstance().GetTopTasker();
    }


    protected override async Task _action(
        (StreamDefinition streamDefinition, byte[] bytes, StreamCaptureType captureType, StreamCaptureStatus
            streamCaptureStatus, int
            frameNumber, int seconds,
            int fps
            ) value)
    {
        using var mem = new MemoryStream();

        using (var image = Image.Load(new DecoderOptions()
               {
               }, value.bytes))
        {
            int cropWidth = image.Width / 4;
            int cropHeight = image.Height / 4;

            var cropArea = new Rectangle(image.Width - cropWidth, 0, cropWidth, cropHeight);
            image.Mutate(x => x.Crop(cropArea).Pad(cropWidth + 20, cropHeight + 20, Color.White));


            await image.SaveAsPngAsync(mem);
        }

        mem.Seek(0, SeekOrigin.Begin);
        using var matOut = Mat.FromImageData(mem.ToArray(), ImreadModes.Unchanged);

        ImageScannerTaskManager.GetInstance().GetLongTasker()?
            .PutInQueue((value.streamDefinition, matOut.ToBytes(), value.captureType, value.streamCaptureStatus,
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
        this._pngDecoder = PngDecoder.Instance;
    }
}