using MemDrawer.Infrastructure.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;

namespace MemDrawer.Infrastructure.Models;

/// <summary>
/// Settings for drawing text on an image
/// </summary>
public struct ImageDrawerOptions
{
    // Background alpha value (0-255) for the text background color
    private const byte BackgroundAlpha = 120;

    // Default constructor with preset values
    public ImageDrawerOptions()
    {
        BackgroundColor = new Rgba32(0, 0, 0, BackgroundAlpha);
        TextColor = Color.White;
        WithOutline = false;
    }

    // Constructor allowing custom text and background colors, outline option, and alpha value
    public ImageDrawerOptions(string textColorHex, string backgroundColorHex, bool withOutline, byte alpha = BackgroundAlpha)
    {
        // Convert hex colors to RGB and create color objects
        var (bgR, bgG, bgB) = ColorUtils.HexToRgb(backgroundColorHex);
        BackgroundColor = new Rgba32(bgR, bgG, bgB, alpha);
        var (textR, textG, textB) = ColorUtils.HexToRgb(textColorHex);
        TextColor = Color.FromRgb(textR, textG, textB);
        TextSkColor = new SKColor(textR, textG, textB);
        WithOutline = withOutline;
    }

    public Rgba32 BackgroundColor { get; }
    public Color TextColor { get; private set; }

    public SKColor BackgroundSkColor => new(BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, BackgroundColor.A);
    public SKColor TextSkColor { get; private set; }
    
    public bool WithOutline { get; private set; }
}