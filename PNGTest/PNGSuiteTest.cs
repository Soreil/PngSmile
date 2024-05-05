using PngSmile;

namespace PNGTest;

public class PNGSuiteTest
{
    [TestCase("basi")]
    [Test]
    public void WriteOutBasicPPM(string s)
    {
        var basicFiles = Directory.EnumerateFiles("samples").Where(x => Path.GetFileName(x).StartsWith(s));
        Directory.CreateDirectory("output");
        foreach (var file in basicFiles)
        {
            var fileData = File.ReadAllBytes(file);
            var png = new PNG();

            var stuff = png.Decode(fileData);
            using var fs = new FileStream(Path.Combine("output", file), FileMode.Create, FileAccess.Write);
            stuff.CopyTo(fs);
        }
    }
}
