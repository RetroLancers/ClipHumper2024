using System.Drawing;
using SixLabors.ImageSharp.Processing;

namespace ClipHunta2;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Numerics;
using ColorReport = (SixLabors.ImageSharp.Color averageColor, string dominantPrimaryColor);

public static class ImageColorAnalyzer
{
    private static byte[] VisualizeAnalyzedAreaToFile(Image<Rgba32> image, int startX, int startY, int size)
    {
        using (Image<Rgba32> visualizedImage = image.Clone(ctx => ctx
                   .Draw(Color.Red, 3, new Rectangle(startX, startY, size, size))))
        {
            using (MemoryStream output = new MemoryStream())
            {
                visualizedImage.Save(output, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
                return output.ToArray();
            }
        }
    }

    public static Image<Rgba32> VisualizeAnalyzedArea(Image<Rgba32> image)
    {
        var (squareSize, startX, startY) = SquareSize( image);
        return image.Clone(ctx => ctx
            .Draw(Color.Red, 3, new Rectangle(startX, startY, squareSize, squareSize)));
    }

    public static Image<Rgba32> VisualizeAnalyzedArea2(Image<Rgba32> image)
    {
        var (squareSize, startX, startY) = SquareSize2( image);
        return image.Clone(ctx => ctx
            .Draw(Color.Red, 3, new Rectangle(startX, startY, squareSize, squareSize)));
    }
    public static Image<Rgba32> VisualizeAnalyzedArea3(Image<Rgba32> image)
    {
        var (squareSize, startX, startY) = SquareSize3( image);
        return image.Clone(ctx => ctx
            .Draw(Color.Red, 3, new Rectangle(startX, startY, squareSize, squareSize)));
    }

    public static ColorReport AnalyzeImageCenter3(byte[] imageBuffer)
    {
        using var ms = new MemoryStream(imageBuffer);
        using var image = Image.Load<Rgba32>(ms);
        // Calculate the dimensions of the center square
        var (squareSize, startX, startY) = SquareSize3( image);

        var totalColor = Vector4.Zero;
        var pixelCount = 0;

        for (var y = startY; y < startY + squareSize; y++)
        {
            for (var x = startX; x < startX + squareSize; x++)
            {
                var pixel = image[x, y];
                totalColor += new Vector4(pixel.R, pixel.G, pixel.B, pixel.A);
                pixelCount++;
            }
        }

        var averageVector = totalColor / pixelCount;
        var averageColor = new Color(new Rgba32(
            (byte)averageVector.X,
            (byte)averageVector.Y,
            (byte)averageVector.Z,
            (byte)averageVector.W));

        var dominantPrimaryColor = GetDominantPrimaryColor(averageColor);
        // if (averageColor.ToHex().EndsWith("FF"))
        // {
        //     var visualizedImage = VisualizeAnalyzedAreaToFile(image, startX, startY, squareSize);
        //     File.WriteAllBytes("c:\\tmp\\visualized_image.png", visualizedImage);
        // }

        return (averageColor, dominantPrimaryColor);
    }
    public static ColorReport AnalyzeImageCenter2(byte[] imageBuffer)
    {
        using var ms = new MemoryStream(imageBuffer);
        using var image = Image.Load<Rgba32>(ms);
        // Calculate the dimensions of the center square
        var (squareSize, startX, startY) = SquareSize2( image);

        var totalColor = Vector4.Zero;
        var pixelCount = 0;

        for (var y = startY; y < startY + squareSize; y++)
        {
            for (var x = startX; x < startX + squareSize; x++)
            {
                var pixel = image[x, y];
                totalColor += new Vector4(pixel.R, pixel.G, pixel.B, pixel.A);
                pixelCount++;
            }
        }

        var averageVector = totalColor / pixelCount;
        var averageColor = new Color(new Rgba32(
            (byte)averageVector.X,
            (byte)averageVector.Y,
            (byte)averageVector.Z,
            (byte)averageVector.W));

        var dominantPrimaryColor = GetDominantPrimaryColor(averageColor);
        // if (averageColor.ToHex().EndsWith("FF"))
        // {
        //     var visualizedImage = VisualizeAnalyzedAreaToFile(image, startX, startY, squareSize);
        //     File.WriteAllBytes("c:\\tmp\\visualized_image.png", visualizedImage);
        // }

        return (averageColor, dominantPrimaryColor);
    }

    public static ColorReport AnalyzeImageCenter(byte[] imageBuffer )
    {
        using var ms = new MemoryStream(imageBuffer);
        using var image = Image.Load<Rgba32>(ms);
        // Calculate the dimensions of the center square
        var (squareSize, startX, startY) = SquareSize( image);

        return AnalyzeImageSection(image, (startY, startX, squareSize));
    }

    private static ColorReport AnalyzeImageSection(Image<Rgba32> image, (int startY, int startX, int squareSize) coords)
    {
        var totalColor = Vector4.Zero;
        var pixelCount = 0;
        var (startY, startX, squareSize) = coords;
        for (var y = startY; y < startY + squareSize; y++)
        {
            for (var x = startX; x < startX + squareSize; x++)
            {
                var pixel = image[x, y];
                totalColor += new Vector4(pixel.R, pixel.G, pixel.B, pixel.A);
                pixelCount++;
            }
        }

        var averageVector = totalColor / pixelCount;
        var averageColor = new Color(new Rgba32(
            (byte)averageVector.X,
            (byte)averageVector.Y,
            (byte)averageVector.Z,
            (byte)averageVector.W));

        var dominantPrimaryColor = GetDominantPrimaryColor(averageColor);
        // if (averageColor.ToHex().EndsWith("FF"))
        // {
        //     var visualizedImage = VisualizeAnalyzedAreaToFile(image, startX, startY, squareSize);
        //     File.WriteAllBytes("c:\\tmp\\visualized_image.png", visualizedImage);
        // }

        return (averageColor, dominantPrimaryColor);
    }
    private static (int squareSize, int startX, int startY) SquareSize3( Image<Rgba32> image)
    {
        int squareSize = 4; // (int)(Math.Min(image.Width, image.Height) * centerPercentage);
        int startX = (image.Width - squareSize) / 2;
        int startY = (image.Height - squareSize) / 2 + 18;
        return (squareSize, startX, startY);
    }
    private static (int squareSize, int startX, int startY) SquareSize2( Image<Rgba32> image)
    {
        int squareSize = 4; // (int)(Math.Min(image.Width, image.Height) * centerPercentage);
        int startX = (image.Width - squareSize) / 2;
        int startY = (image.Height - squareSize) / 2 - 10;
        return (squareSize, startX, startY);
    }

    private static (int squareSize, int startX, int startY) SquareSize( Image<Rgba32> image)
    {
        int squareSize = 4; // (int)(Math.Min(image.Width, image.Height) * centerPercentage);
        int startX = (image.Width - squareSize) / 2 - (image.Width - squareSize) / 4;
        int startY = (image.Height - squareSize) / 2 - 10;
        return (squareSize, startX, startY);
    }

    public static ColorReport AnalyzeImage(byte[] imageBuffer)
    {
        using var ms = new MemoryStream(imageBuffer);
        using var image = Image.Load<Rgba32>(ms);
        var totalColor = Vector4.Zero;
        var pixelCount = 0;

        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                var pixel = image[x, y];
                totalColor += new Vector4(pixel.R, pixel.G, pixel.B, pixel.A);
                pixelCount++;
            }
        }

        var averageVector = totalColor / pixelCount;
        var averageColor = new Color(new Rgba32(
            (byte)averageVector.X,
            (byte)averageVector.Y,
            (byte)averageVector.Z,
            (byte)averageVector.W));

        var dominantPrimaryColor = GetDominantPrimaryColor(averageColor);

        return (averageColor, dominantPrimaryColor);
    }

    private static string GetDominantPrimaryColor(Color col)
    {
        var color = col.ToPixel<Rgba32>();
        if (color.R > color.G && color.R > color.B)
            return "Red";
        if (color.G > color.R && color.G > color.B)
            return "Green";
        if (color.B > color.R && color.B > color.G)
            return "Blue";
        return "None";
    }
}