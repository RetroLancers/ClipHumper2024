using OpenCvSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Tesseract;

namespace ClipHunta2;

public class ImagePrepperTask : LongTask<Tuple<string, byte[]>>
{
    protected override Task  _action(Tuple<string, byte[]> value)
    {
        using var outputstream = new MemoryStream();

        using (var image = Image.Load(value.Item2))
        {
            image.Mutate(img =>
            {
                img.Crop(new Rectangle(image.Width / 4, image.Height / 2, image.Width / 2, image.Height / 2));
            });
            image.SaveAsPng(outputstream);
        }

        outputstream.Seek(0, SeekOrigin.Begin);
        using var matimg = Mat.FromImageData(outputstream.ToArray(), ImreadModes.Grayscale);
        using Mat img_out = matimg.EmptyClone();
        Cv2.Threshold(matimg, img_out, 0, 255, ThresholdTypes.BinaryInv | ThresholdTypes.Otsu);
        ImageScannerTaskManager.GetInstance().GetLongTasker()
            .PutInQueue(new Tuple<string, byte[]>(value.Item1,  img_out.ToBytes()));
        return null;
    }

    public void PutInQueue(Tuple<string, byte[]> value)
    {
        Put(new LongTaskQueueItem<Tuple<string, byte[]>>(value)).Wait();
    }


    public ImagePrepperTask(CancellationTokenSource cts) : base(cts)
    {
    }
}