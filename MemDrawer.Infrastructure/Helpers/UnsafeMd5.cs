using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace MemDrawer.Infrastructure.Helpers;

public static class UnsafeMd5
{
    /// <summary>
    /// Computes the MD5 hash of a stream and returns it as a hexadecimal string.
    /// This implementation uses unsafe code and stackalloc for performance optimization.
    /// </summary>
    /// <param name="stream">The input stream to compute the MD5 hash for.</param>
    /// <returns>The MD5 hash as a hexadecimal string.</returns>
    public static unsafe string Compute(Stream stream)
    {
        // Reset stream position to the beginning if possible
        if (stream.CanSeek)
            stream.Position = 0;

        using var md5 = MD5.Create();

        // Buffer for reading stream data
        const int bufferSize = 8192;
        Span<byte> buffer = stackalloc byte[bufferSize];

        // Read the stream in chunks and update the MD5 hash incrementally
        int bytesRead;
        while ((bytesRead = stream.Read(buffer)) > 0)
        {
            fixed (byte* ptr = buffer)
            {
                md5.TransformBlock(buffer[..bytesRead].ToArray(), 0, bytesRead, null, 0);
            }
        }

        md5.TransformFinalBlock([], 0, 0);

        // Copy the hash to a stack-allocated span
        Span<byte> hash = stackalloc byte[md5.Hash!.Length];
        md5.Hash.CopyTo(hash);

        // Convert hash bytes to hex string representation using stackalloc
        Span<char> result = stackalloc char[hash.Length * 2];
        for (var i = 0; i < hash.Length; i++)
        {
            var b = hash[i];
            result[i * 2] = GetHexChar(b >> 4);
            result[i * 2 + 1] = GetHexChar(b & 0xF);
        }

        return new string(result);
    }

    // Convert a 4-bit value to its hexadecimal character representation
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static char GetHexChar(int val)
        => (char)(val < 10 ? '0' + val : 'a' + (val - 10));
}