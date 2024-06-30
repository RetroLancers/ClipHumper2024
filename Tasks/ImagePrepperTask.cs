using OpenCvSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using Tesseract;
using Point = System.Drawing.Point;
using Size = OpenCvSharp.Size;

namespace ClipHunta2;

using ColorReport = (SixLabors.ImageSharp.Color averageColor, string dominantPrimaryColor);

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
        //       using var mem = new MemoryStream();
        using var mem2 = new MemoryStream();
        (Color averageColor, string dominantPrimaryColor) color2;
        (Color averageColor, string dominantPrimaryColor) color;
        (Color averageColor, string dominantPrimaryColor) color3;
        using (var image = Image.Load(new DecoderOptions()
               {
               }, value.bytes))
        {
            int cropWidth = image.Width / 8;
            int cropHeight = (image.Height / 16) + 15;
            int cropStarty = image.Height / 2 + 180;
            int cropStartx = image.Width / 2 - 100;
            var cropArea = new Rectangle(cropStartx, cropStarty, cropWidth, cropHeight);
            image.Mutate(x => x.Crop(cropArea).Pad(cropWidth + 20, cropHeight + 20, Color.White));
            await image.SaveAsPngAsync(mem2);
            mem2.Seek(0, SeekOrigin.Begin);
            var imageBuffer = mem2.ToArray();
            color = ImageColorAnalyzer.AnalyzeImageCenter(imageBuffer);
            color2 = ImageColorAnalyzer.AnalyzeImageCenter2(imageBuffer);
            color3 = ImageColorAnalyzer.AnalyzeImageCenter3(imageBuffer);
            // using var cloneAs = image.CloneAs<Rgba32>();
            // using var image2 = ImageColorAnalyzer.VisualizeAnalyzedArea(cloneAs);
            // using var image3 = ImageColorAnalyzer.VisualizeAnalyzedArea2(image2);
            // using var image4 = ImageColorAnalyzer.VisualizeAnalyzedArea3(image3);
            mem2.Seek(0, SeekOrigin.Begin);
            await image.SaveAsPngAsync(mem2);
        }


        //Console.WriteLine($"Dominate Color: {color}");
        mem2.Seek(0, SeekOrigin.Begin);
        using var bottomLeft = Mat.FromImageData(mem2.ToArray(), ImreadModes.Unchanged);
        // mem.Seek(0, SeekOrigin.Begin);
        //  using var matOut = Mat.FromImageData(mem.ToArray(), ImreadModes.Unchanged);

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
        this._pngDecoder = PngDecoder.Instance;
    }
}