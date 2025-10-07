using BenchmarkDotNet.Attributes;
using MemDrawer.Infrastructure.Models;
using MemDrawer.Infrastructure.Services;

namespace MemDrawer.Benchmarks;

[MemoryDiagnoser]
public class ImageDrawBenchmark
{
    private const string ImagePath = "kevinhart2.jpg";

    private readonly List<string> _topTextExamples = new()
    {
        "When you realize",
        "That moment when",
        "POV:",
        "Me trying to explain",
        "When the code works but",
        "That awkward moment when",
        "When you finally understand",
        "Trying to debug like",
        "When the WiFi is down and",
        "When you see your code in production and"
    };

    private readonly List<string> _bottomTextExamples = new()
    {
        "you left your keys inside",
        "you forgot your password",
        "you walk into a room and forget why",
        "your code doesn't work",
        "you realize it's Monday again",
        "you find a bug in your own code",
        "you have to explain your code to a non-techie",
        "you try to fix one bug and create three more",
        "your internet is slower than a snail",
        "it actually works without any errors"
    };

    private readonly Dictionary<string, string> _pairs = new();

    [GlobalSetup]
    public void Setup()
    {
        ImageSharpTextDrawer.Init();
        
        // Fill pairs to avoid measuring random generation time
        var random = new Random();
        for (int i = 0; i < 150; i++)
        {
            var topText = _topTextExamples[random.Next(_topTextExamples.Count)];
            var bottomText = _bottomTextExamples[random.Next(_bottomTextExamples.Count)];
            _pairs[topText] = bottomText;
        }
        
    }

    [Benchmark]
    public async Task DrawWithImageSharp()
    {
        foreach (var pair in _pairs)
        {
            await using var imageStream = File.OpenRead(ImagePath);
            await using var outputStream = new MemoryStream();
            await ImageSharpTextDrawer.DrawTextOnImageAsync(
                pair.Key.AsMemory(),
                pair.Value.AsMemory(),
                imageStream,
                outputStream,
                new ImageDrawerOptions(),
                CancellationToken.None);
        }
    }

    [Benchmark]
    public async Task DrawWithSkiaSharp()
    {
        foreach (var pair in _pairs)
        {
            await using var imageStream = File.OpenRead(ImagePath);
            await using var outputStream = new MemoryStream();
            await SkiaTextDrawer.DrawTextOnImageAsync(
                imageStream,
                outputStream,
                pair.Key.AsMemory(),
                pair.Value.AsMemory(),
                new ImageDrawerOptions(),
                CancellationToken.None);
        }
    }

}