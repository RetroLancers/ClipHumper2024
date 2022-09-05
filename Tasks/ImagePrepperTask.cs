using OpenCvSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using Tesseract;

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

        using (var image = Image.Load(value.bytes, _pngDecoder))
        {
            var imageWidth = image.Width;
            var imageHeight = image.Height;
            image.Mutate(img =>
            {
                img.Crop(new Rectangle(imageWidth / 4 + imageWidth/8, imageHeight / 2 +imageHeight  /8, imageWidth / 2, imageHeight / 8));

            });
            await image.SaveAsPngAsync(mem);
        }

        mem.Seek(0, SeekOrigin.Begin);
        using var matOut = Mat.FromImageData(mem.ToArray(), ImreadModes.Grayscale);
        using var imgOut = matOut.EmptyClone();
        using var gausout = matOut.EmptyClone();
        using var threshout = matOut.EmptyClone();
 
        Cv2.GaussianBlur(matOut, gausout, new OpenCvSharp.Size(9, 9), -1);
     //  
        Cv2.AdaptiveThreshold(gausout, threshout,  255, AdaptiveThresholdTypes.GaussianC,ThresholdTypes.Binary,5,0);
        Cv2.Threshold(threshout, imgOut, 0, 255, ThresholdTypes.Otsu);

        ImageScannerTaskManager.GetInstance().GetLongTasker()?
        .PutInQueue((value.streamDefinition, imgOut.ToBytes(), value.captureType, value.streamCaptureStatus,
            value.frameNumber, value.seconds,
            value.fps));

        //Cv2.ImShow(value.streamDefinition.StreamerName, imgOut);
        //Cv2.WaitKey(1);
        

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
        this._pngDecoder = new PngDecoder();
    }
}