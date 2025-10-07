using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using MemDrawer.Infrastructure.Models;
using SkiaSharp;

namespace MemDrawer.Infrastructure.Services;

/// <summary>
/// Draws text on an image with configurable options.
/// Uses SkiaSharp for rendering. (more performant than ImageSharp)
/// A lot of codebase is obsolete and should be removed/refactored.
/// </summary>
public static class SkiaTextDrawer
{
    // Typeface for the Impact font
    private static readonly SKTypeface Typeface;
    
    // Cache for SKPaint objects based on font size
    private static readonly ConcurrentDictionary<float, (SKPaint fill, SKPaint outline)> FontCache = new();

    // Font size constraints and padding
    private const int DefaultPadding = 28;
    private const float MinFontSize = 10f;
    private const float MaxFontSize = 100f;

    // Static constructor to initialize the typeface
    static SkiaTextDrawer()
    {
        Typeface = SKTypeface.FromFile("data\\impact.ttf") ?? SKTypeface.Default;
    }

    public static async Task DrawTextOnImageAsync(
        Stream inputStream,
        Stream outputStream,
        ReadOnlyMemory<char> topText,
        ReadOnlyMemory<char> bottomText,
        ImageDrawerOptions options,
        CancellationToken cancellationToken = default)
    {
        // Load the image from the input stream 
        using var bitmap = SKBitmap.Decode(inputStream);
        
        // Create a canvas to draw on the bitmap
        using var canvas = new SKCanvas(bitmap);

        var width = bitmap.Width;
        var height = bitmap.Height;

        // Draw background rectangles for text areas if needed
        var backgroundPaint = new SKPaint
        {
            Color = options.BackgroundSkColor,
            Style = SKPaintStyle.Fill
        };

        // Draw top text if provided
        if (!topText.IsEmpty)
        {
            // Find the best font size that fits within the image width with padding
            var (fill, outline) = FindBestFontForSpan(topText.Span, width);
            var topHeight = fill.TextSize + DefaultPadding * 2;

            // Draw background rectangle for top text
            canvas.DrawRect(new SKRect(0, 0, width, topHeight), backgroundPaint);
            
            // Draw the centered top text
            DrawCenteredText(canvas, topText.Span.Trim(), fill, outline, width, topHeight / 2 + fill.TextSize / 3, options);
        }

        // Draw bottom text if provided
        if (!bottomText.IsEmpty)
        {
            // Find the best font size that fits within the image width with padding
            var (fill, outline) = FindBestFontForSpan(bottomText.Span, width);
            var rectHeight = fill.TextSize + DefaultPadding * 2;
            var rectTop = height - rectHeight;

            // Draw background rectangle for bottom text
            canvas.DrawRect(new SKRect(0, rectTop, width, height), backgroundPaint);
            DrawCenteredText(canvas, bottomText.Span.Trim(), fill, outline, width,
                rectTop + rectHeight / 2 + fill.TextSize / 3, options);
        }

        // Encode the modified bitmap to JPEG and write to the output stream
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
        
        await data.AsStream().CopyToAsync(outputStream, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DrawCenteredText(SKCanvas canvas, ReadOnlySpan<char> text, SKPaint fill, SKPaint outline,
        float width, float y, in ImageDrawerOptions options)
    {
        // Convert text to uppercase using a stack-allocated buffer
        Span<char> buffer = stackalloc char[text.Length];
        text.ToUpperInvariant(buffer);

        // Measure text width and calculate x position for centering
        var textWidth = fill.MeasureText(buffer);
        var x = (width - textWidth) / 2f;

        // Create string from span for drawing
        var str = string.Create(buffer.Length, buffer, (span, src) => src.CopyTo(span));

        // Set paint colors based on options
        fill.Color = options.TextSkColor;

        // Draw outline if enabled
        if (options.WithOutline)
            canvas.DrawText(str, x, y, outline);
        canvas.DrawText(str, x, y, fill);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (SKPaint fill, SKPaint outline) FindBestFontForSpan(ReadOnlySpan<char> text, int maxWidth)
    {
        // Binary search to find the largest font size that fits within the image width
        float min = MinFontSize, max = MaxFontSize, chosen = min;

        // Use a stack-allocated buffer to avoid heap allocations
        Span<char> buffer = stackalloc char[text.Length];
        text.CopyTo(buffer);

        // Binary search for the best fitting font size
        while (min <= max)
        {
            // Use mid-point font size for testing
            var mid = (min + max) / 2f;
            var (fill, _) = GetOrCreatePaints(mid);
            var w = fill.MeasureText(buffer);

            // Check if the measured width with padding fits within the max width
            if (w + DefaultPadding * 2 > maxWidth)
                max = mid - 1;
            else
            {
                // Fits, try larger size
                chosen = mid;
                min = mid + 1;
            }
        }

        return GetOrCreatePaints(chosen);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (SKPaint fill, SKPaint outline) GetOrCreatePaints(float size)
        => FontCache.GetOrAdd(size, s =>
        {
            var outline = new SKPaint
            {
                Typeface = Typeface,
                Color = SKColors.Black,
                TextSize = s,
                IsAntialias = true,
                TextAlign = SKTextAlign.Left,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = Math.Max(2f, s / 12f)
            };

            var fill = new SKPaint
            {
                Typeface = Typeface,
                Color = SKColors.White,
                TextSize = s,
                IsAntialias = true,
                TextAlign = SKTextAlign.Left,
                Style = SKPaintStyle.Fill
            };

            return (fill, outline);
        });
}