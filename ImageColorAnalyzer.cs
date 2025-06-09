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

/// <summary>
/// Represents the result of a color analysis, containing the average color and the dominant primary color.
/// </summary>
using ColorReport = (SixLabors.ImageSharp.Color averageColor, string dominantPrimaryColor);

/// <summary>
/// Provides static methods for analyzing image colors, focusing on specific regions or the entire image.
/// </summary>
public static class ImageColorAnalyzer
{
    /// <summary>
    /// Visualizes a specified square area on an image by drawing a red rectangle around it and returns the image as a byte array.
    /// This method is useful for debugging to see which part of the image is being analyzed.
    /// </summary>
    /// <param name="image">The image to draw on.</param>
    /// <param name="startX">The starting X coordinate of the rectangle.</param>
    /// <param name="startY">The starting Y coordinate of the rectangle.</param>
    /// <param name="size">The width and height of the rectangle.</param>
    /// <returns>A byte array representing the image with the visualized area in PNG format.</returns>
    private static byte[] VisualizeAnalyzedAreaToFile(Image<Rgba32> image, int startX, int startY, int size)
    {
        // Clone the image to avoid modifying the original
        using (Image<Rgba32> visualizedImage = image.Clone(ctx => ctx
                   .Draw(Color.Red, 3, new Rectangle(startX, startY, size, size)))) // Draw a 3px thick red rectangle
        {
            using (MemoryStream output = new MemoryStream())
            {
                visualizedImage.SaveAsPng(output); // Save the modified image to a memory stream as PNG
                return output.ToArray();
            }
        }
    }

    /// <summary>
    /// Creates a clone of the input image with a red rectangle drawn around the area defined by <see cref="SquareSize"/>.
    /// Used for visual debugging of the analysis area for <see cref="AnalyzeImageCenter"/>.
    /// </summary>
    /// <param name="image">The input image.</param>
    /// <returns>A new image with the visualized analysis area.</returns>
    public static Image<Rgba32> VisualizeAnalyzedArea(Image<Rgba32> image)
    {
        var (squareSize, startX, startY) = SquareSize(image);
        return image.Clone(ctx => ctx
            .Draw(Color.Red, 3, new Rectangle(startX, startY, squareSize, squareSize)));
    }

    /// <summary>
    /// Creates a clone of the input image with a red rectangle drawn around the area defined by <see cref="SquareSize2"/>.
    /// Used for visual debugging of the analysis area for <see cref="AnalyzeImageCenter2"/>.
    /// </summary>
    /// <param name="image">The input image.</param>
    /// <returns>A new image with the visualized analysis area.</returns>
    public static Image<Rgba32> VisualizeAnalyzedArea2(Image<Rgba32> image)
    {
        var (squareSize, startX, startY) = SquareSize2(image);
        return image.Clone(ctx => ctx
            .Draw(Color.Red, 3, new Rectangle(startX, startY, squareSize, squareSize)));
    }

    /// <summary>
    /// Creates a clone of the input image with a red rectangle drawn around the area defined by <see cref="SquareSize3"/>.
    /// Used for visual debugging of the analysis area for <see cref="AnalyzeImageCenter3"/>.
    /// </summary>
    /// <param name="image">The input image.</param>
    /// <returns>A new image with the visualized analysis area.</returns>
    public static Image<Rgba32> VisualizeAnalyzedArea3(Image<Rgba32> image)
    {
        var (squareSize, startX, startY) = SquareSize3(image);
        return image.Clone(ctx => ctx
            .Draw(Color.Red, 3, new Rectangle(startX, startY, squareSize, squareSize)));
    }

    /// <summary>
    /// Analyzes the color of a specific region in the given image, defined by <see cref="SquareSize3"/>.
    /// This region is a 4x4 square offset vertically from the center, likely targeting a specific UI element.
    /// </summary>
    /// <param name="imageBuffer">The image data as a byte array.</param>
    /// <returns>A <see cref="ColorReport"/> containing the average color and dominant primary color of the region.</returns>
    public static ColorReport AnalyzeImageCenter3(byte[] imageBuffer)
    {
        using var ms = new MemoryStream(imageBuffer);
        using var image = Image.Load<Rgba32>(ms);
        var (squareSize, startX, startY) = SquareSize3(image); // Defines a 4x4 square, Y-offset +18 from center
        return AnalyzeImageSection(image, (startY, startX, squareSize));
    }

    /// <summary>
    /// Analyzes the color of a specific region in the given image, defined by <see cref="SquareSize2"/>.
    /// This region is a 4x4 square offset vertically from the center, likely targeting a specific UI element.
    /// </summary>
    /// <param name="imageBuffer">The image data as a byte array.</param>
    /// <returns>A <see cref="ColorReport"/> containing the average color and dominant primary color of the region.</returns>
    public static ColorReport AnalyzeImageCenter2(byte[] imageBuffer)
    {
        using var ms = new MemoryStream(imageBuffer);
        using var image = Image.Load<Rgba32>(ms);
        var (squareSize, startX, startY) = SquareSize2(image); // Defines a 4x4 square, Y-offset -10 from center
        return AnalyzeImageSection(image, (startY, startX, squareSize));
    }

    /// <summary>
    /// Analyzes the color of a specific region in the given image, defined by <see cref="SquareSize"/>.
    /// This region is a 4x4 square offset horizontally and vertically from the center, likely targeting a specific UI element.
    /// </summary>
    /// <param name="imageBuffer">The image data as a byte array.</param>
    /// <returns>A <see cref="ColorReport"/> containing the average color and dominant primary color of the region.</returns>
    public static ColorReport AnalyzeImageCenter(byte[] imageBuffer )
    {
        using var ms = new MemoryStream(imageBuffer);
        using var image = Image.Load<Rgba32>(ms);
        var (squareSize, startX, startY) = SquareSize(image); // Defines a 4x4 square, X-offset by -1/4 of width from center, Y-offset -10 from center
        return AnalyzeImageSection(image, (startY, startX, squareSize));
    }

    /// <summary>
    /// Analyzes a specified square section of an image to determine its average color and dominant primary color.
    /// </summary>
    /// <param name="image">The image to analyze.</param>
    /// <param name="coords">A tuple defining the start Y, start X, and size of the square section.</param>
    /// <returns>A <see cref="ColorReport"/> for the specified section.</returns>
    private static ColorReport AnalyzeImageSection(Image<Rgba32> image, (int startY, int startX, int squareSize) coords)
    {
        var totalColor = Vector4.Zero;
        var pixelCount = 0;
        var (startY, startX, squareSize) = coords;

        // Ensure coordinates are within image bounds
        int endY = Math.Min(startY + squareSize, image.Height);
        int endX = Math.Min(startX + squareSize, image.Width);
        startY = Math.Max(0, startY);
        startX = Math.Max(0, startX);

        for (var y = startY; y < endY; y++)
        {
            for (var x = startX; x < endX; x++)
            {
                var pixel = image[x, y];
                totalColor += new Vector4(pixel.R, pixel.G, pixel.B, pixel.A);
                pixelCount++;
            }
        }

        if (pixelCount == 0)
        {
            // Return a default or signal an error if the region was outside the image or of zero size.
            return (Color.Transparent, "None");
        }

        var averageVector = totalColor / pixelCount;
        var averageColor = new Color(new Rgba32(
            (byte)Math.Clamp(averageVector.X, 0, 255), // Clamp values to byte range
            (byte)Math.Clamp(averageVector.Y, 0, 255),
            (byte)Math.Clamp(averageVector.Z, 0, 255),
            (byte)Math.Clamp(averageVector.W, 0, 255)));

        var dominantPrimaryColor = GetDominantPrimaryColor(averageColor);

        // Debugging code for visualizing the analyzed area (commented out)
        // if (averageColor.ToHex().EndsWith("FF")) // Example condition
        // {
        //     var visualizedImage = VisualizeAnalyzedAreaToFile(image, coords.startX, coords.startY, coords.squareSize);
        //     File.WriteAllBytes("c:\\tmp\\visualized_image.png", visualizedImage);
        // }

        return (averageColor, dominantPrimaryColor);
    }

    /// <summary>
    /// Defines a 4x4 square region for color analysis.
    /// The region is offset +18 pixels vertically from the image center.
    /// This specific coordinate suggests it targets a particular UI element in an image of expected dimensions.
    /// </summary>
    /// <param name="image">The image for which to calculate the region.</param>
    /// <returns>A tuple (squareSize, startX, startY).</returns>
    private static (int squareSize, int startX, int startY) SquareSize3( Image<Rgba32> image)
    {
        int squareSize = 4;
        int startX = (image.Width - squareSize) / 2;
        int startY = (image.Height - squareSize) / 2 + 18; // Offset from center
        return (squareSize, startX, startY);
    }

    /// <summary>
    /// Defines a 4x4 square region for color analysis.
    /// The region is offset -10 pixels vertically from the image center.
    /// This specific coordinate suggests it targets a particular UI element in an image of expected dimensions.
    /// </summary>
    /// <param name="image">The image for which to calculate the region.</param>
    /// <returns>A tuple (squareSize, startX, startY).</returns>
    private static (int squareSize, int startX, int startY) SquareSize2( Image<Rgba32> image)
    {
        int squareSize = 4;
        int startX = (image.Width - squareSize) / 2;
        int startY = (image.Height - squareSize) / 2 - 10; // Offset from center
        return (squareSize, startX, startY);
    }

    /// <summary>
    /// Defines a 4x4 square region for color analysis.
    /// The region is offset -1/4 of its width from the horizontal center, and -10 pixels vertically from the image center.
    /// This specific coordinate suggests it targets a particular UI element in an image of expected dimensions.
    /// </summary>
    /// <param name="image">The image for which to calculate the region.</param>
    /// <returns>A tuple (squareSize, startX, startY).</returns>
    private static (int squareSize, int startX, int startY) SquareSize( Image<Rgba32> image)
    {
        int squareSize = 4;
        int startX = (image.Width - squareSize) / 2 - (image.Width - squareSize) / 4; // Offset from center
        int startY = (image.Height - squareSize) / 2 - 10; // Offset from center
        return (squareSize, startX, startY);
    }

    /// <summary>
    /// Analyzes the entire image to determine its average color and dominant primary color.
    /// </summary>
    /// <param name="imageBuffer">The image data as a byte array.</param>
    /// <returns>A <see cref="ColorReport"/> for the entire image.</returns>
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
                totalColor += new Vector4(pixel.R, pixel.G, pixel.B, pixel.A); // Accumulate color values
                pixelCount++;
            }
        }

        if (pixelCount == 0)
        {
            return (Color.Transparent, "None"); // Handle empty or zero-size image
        }

        var averageVector = totalColor / pixelCount;
        var averageColor = new Color(new Rgba32(
            (byte)Math.Clamp(averageVector.X, 0, 255), // Clamp values to byte range
            (byte)Math.Clamp(averageVector.Y, 0, 255),
            (byte)Math.Clamp(averageVector.Z, 0, 255),
            (byte)Math.Clamp(averageVector.W, 0, 255)));

        var dominantPrimaryColor = GetDominantPrimaryColor(averageColor);

        return (averageColor, dominantPrimaryColor);
    }

    /// <summary>
    /// Determines the dominant primary color (Red, Green, or Blue) from a given color.
    /// </summary>
    /// <param name="col">The color to analyze.</param>
    /// <returns>"Red", "Green", "Blue", or "None" if no single primary color is dominant or if the color is achromatic.</returns>
    private static string GetDominantPrimaryColor(Color col)
    {
        var color = col.ToPixel<Rgba32>(); // Convert to Rgba32 for component access
        // Check for achromatic color (or very close to it) to avoid arbitrary dominant color for grays
        if (Math.Abs(color.R - color.G) < 5 && Math.Abs(color.R - color.B) < 5 && Math.Abs(color.G - color.B) < 5) // Tolerance for near grays
        {
            return "None";
        }

        if (color.R > color.G && color.R > color.B)
            return "Red";
        if (color.G > color.R && color.G > color.B) // Ensure G is greater than R as well
            return "Green";
        if (color.B > color.R && color.B > color.G) // Ensure B is greater than R and G
            return "Blue";

        // This case might be reached if two components are equal and largest (e.g., R=200, G=200, B=100 -> Yellowish, not strictly Red or Green)
        // For simplicity, current logic might pick one based on order. More sophisticated logic could return "Mixed" or handle ties.
        return "None"; // Fallback if no single component is strictly dominant (e.g. R=G > B)
    }
}