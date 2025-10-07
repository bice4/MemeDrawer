using System.Runtime.CompilerServices;

namespace MemDrawer.Infrastructure.Helpers;

public static class ColorUtils
{
    /// <summary>
    /// Converts a hex color string to its RGB components.
    /// </summary>
    /// <param name="hex">Hex color string (e.g. "#RRGGBB" or "RRGGBB")</param>
    /// <returns>Tuple of (R, G, B) components</returns>
    /// <exception cref="ArgumentException">Thrown if the hex string is invalid</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (byte r, byte g, byte b) HexToRgb(ReadOnlySpan<char> hex)
    {
        // Remove leading '#' if present
        if (hex[0] == '#')
            hex = hex[1..];
        
        // Validate length
        if (hex.Length != 6)
            throw new ArgumentException("Invalid hex color");

        // Parse R, G, B components
        var r = (byte)((HexToByte(hex[0]) << 4) | HexToByte(hex[1]));
        var g = (byte)((HexToByte(hex[2]) << 4) | HexToByte(hex[3]));
        var b = (byte)((HexToByte(hex[4]) << 4) | HexToByte(hex[5]));

        return (r, g, b);

        // Convert a single hex character to its byte value
        static byte HexToByte(char c)
        {
            // Using uint casts to avoid branching for invalid characters
            return (byte)(
                (uint)(c - '0') <= 9 ? c - '0' :
                (uint)(c - 'A') <= 5 ? c - 'A' + 10 :
                (uint)(c - 'a') <= 5 ? c - 'a' + 10 :
                throw new ArgumentException("Invalid hex char"));
        }
    }
}