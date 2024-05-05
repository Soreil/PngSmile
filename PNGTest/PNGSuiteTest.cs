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

            var stuff = png.ReadImage(fileData);
            using var fs = new FileStream(Path.Combine("output", file), FileMode.Create, FileAccess.Write);
            stuff.CopyTo(fs);
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
