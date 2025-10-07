using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using MemDrawer.Infrastructure.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MemDrawer.Infrastructure.Services;

/// <summary>
/// Draws text on an image with configurable options.
/// Uses ImageSharp and SixLabors.Fonts for rendering.
/// </summary>
public static class ImageSharpTextDrawer
{
    private static FontFamily _mainFontFamily;
    private static readonly ConcurrentDictionary<float, Font> FontCache = new();

    // Font size constraints and padding
    private const float MinFontSize = 1f;
    private const float MaxFontSize = 65f;
    private const int Padding = 28;
    private const float PaddingX2 = Padding * 2f;

    // Static constructor to initialize the main font family
    public static void Init()
    {
        var fonts = new FontCollection();
        _mainFontFamily = fonts.Add("data\\impact.ttf");
    }

    [SkipLocalsInit]
    public static async ValueTask DrawTextOnImageAsync(
        ReadOnlyMemory<char> topText,
        ReadOnlyMemory<char> bottomText,
        Stream imageStream,
        Stream outputStream,
        ImageDrawerOptions imageDrawerOptions,
        CancellationToken cancellationToken)
    {
        // Load the image from the input stream
        using var image = await Image.LoadAsync<Rgba32>(imageStream, cancellationToken);
        var width = image.Width;
        var height = image.Height;

        Font? topFont = null;
        Font? bottomFont = null;
        var topMeasured = FontRectangle.Empty;
        var bottomMeasured = FontRectangle.Empty;

        // Measure and find appropriate fonts for top and bottom text
        if (!topText.IsEmpty)
        {
            topMeasured = FindFontAndMeasure(topText, width, out topFont);
        }

        if (!bottomText.IsEmpty)
        {
            bottomMeasured = FindFontAndMeasure(bottomText, width, out bottomFont);
        }

        var topRect = RectangleF.Empty;
        var bottomRect = RectangleF.Empty;
        var topLocation = PointF.Empty;
        var bottomLocation = PointF.Empty;

        // Compute layout rectangles and text positions
        if (topMeasured != FontRectangle.Empty)
        {
            ComputeLayout(topMeasured, width, true, height, out topRect, out topLocation);
        }

        if (bottomMeasured != FontRectangle.Empty)
        {
            ComputeLayout(bottomMeasured, width, false, height, out bottomRect, out bottomLocation);
        }

        // Prepare text strings (uppercase) for drawing
        var topTextStr = topText.IsEmpty ? string.Empty : topText.ToString().ToUpper();
        var bottomTextStr = bottomText.IsEmpty ? string.Empty : bottomText.ToString().ToUpper();

        // Draw background rectangles and text on the image
        var backgroundColor = imageDrawerOptions.BackgroundColor;
        var textColor = imageDrawerOptions.TextColor;

        image.Mutate(ctx =>
        {
            if (bottomFont is not null)
            {
                ctx.Fill(backgroundColor, bottomRect);
                ctx.DrawText(bottomTextStr, bottomFont, textColor, bottomLocation);
            }

            if (topFont is not null)
            {
                ctx.Fill(backgroundColor, topRect);
                ctx.DrawText(topTextStr, topFont, textColor, topLocation);
            }
        });

        await image.SaveAsJpegAsync(outputStream, cancellationToken: cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ComputeLayout(in FontRectangle measured, int width, bool isTop, int imageHeight,
        out RectangleF rect, out PointF point)
    {
        // Calculate rectangle height and positions based on whether it's top or bottom text
        var rectHeight = measured.Height + PaddingX2;
        
        // If top text, position at the top; if bottom text, position at the bottom of the image
        if (isTop)
        {
            // Top text
            rect = new RectangleF(0, 0, width, rectHeight);
            
            // Center the text horizontally and vertically within the rectangle
            point = new PointF(width / 2f - measured.Width / 2f, rect.Height / 2f - measured.Height / 2f);
        }
        else
        {
            rect = new RectangleF(0, imageHeight - rectHeight, width, rectHeight);
            
            // Center the text horizontally and vertically within the rectangle at the bottom
            point = new PointF(width / 2f - measured.Width / 2f,
                imageHeight - rectHeight + rect.Height / 2f - measured.Height / 2f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static FontRectangle FindFontAndMeasure(ReadOnlyMemory<char> text, int width, out Font font)
    {
        // Binary search to find the largest font size that fits within the image width
        // considering padding
        var low = MinFontSize;
        var high = MaxFontSize;
        var chosen = low;
        FontRectangle measured = default;

        // Binary search for optimal font size
        while (low <= high)
        {
            // Use mid-point to avoid overflow
            var mid = (low + high) / 2f;
            
            // Measure the text size at the current mid font size
            measured = Measure(text, mid);

            // Check if the measured width with padding fits within the image width 
            if (measured.Width + PaddingX2 > width)
                // Too big, try smaller size
                high = mid - 1f;
            else
            {
                // Fits, try larger size
                low = mid + 1f;
                chosen = mid;
            }
        }

        // Get the font for the chosen size
        font = GetFont(chosen);
        return measured;
    }

    // Measure the size of the text with the specified font size
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static FontRectangle Measure(ReadOnlyMemory<char> text, float size)
    {
        var font = GetFont(size);
        var opts = new TextOptions(font);
        return TextMeasurer.MeasureSize(text.Span, opts);
    }
    
    // Retrieve or create a Font instance for the specified size, using caching for performance
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Font GetFont(float size)
        => FontCache.GetOrAdd(size, s => _mainFontFamily.CreateFont(s, FontStyle.Regular));
}