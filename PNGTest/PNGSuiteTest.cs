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
        foreach (var fileName in basicFiles)
        {
            var fileData = File.ReadAllBytes(fileName);
            var png = new PNG();

            using var stuff = png.ReadImage(fileData);
            stuff.Flush();
            var path = Path.Combine("output", Path.ChangeExtension(fileName, ".ppm"));
            using var fs = new FileStream(path, FileMode.Create);
            stuff.Position = 0;
            stuff.CopyTo(fs);
            fs.Flush();
        }
    }

    [TestCase("basn2c08.png")]
    [Test]
    public void basn2c08(string fileName)
    {
        Directory.CreateDirectory("output");

        var fileData = File.ReadAllBytes(Path.Combine("samples", fileName));
        var png = new PNG();

        using var stuff = png.ReadImage(fileData);
        stuff.Flush();
        var path = Path.Combine("output", Path.ChangeExtension(fileName, ".ppm"));
        using var fs = new FileStream(path, FileMode.Create);
        stuff.Position = 0;
        stuff.CopyTo(fs);
        fs.Flush();
    }
}
