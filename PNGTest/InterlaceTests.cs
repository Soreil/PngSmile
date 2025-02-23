using PngSmile;

namespace PNGTest;

public class InterlaceTests
{
    [Test]
    public void Generate8x8()
    {
        var generator = InterlacedIndexGenerator.GetInterLacedIndex(8, 8);
        var gen = generator.ToList();

        foreach (var item in gen)
        {
            Console.WriteLine(item);
        }
    }
}
