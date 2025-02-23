using PngSmile;

namespace PNGTest;

public class PNGSuiteTests
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
            var path = Path.Combine("output", Path.ChangeExtension(Path.GetFileName(fileName), ".ppm"));
            using var fs = new FileStream(path, FileMode.Create);
            stuff.Position = 0;
            stuff.CopyTo(fs);
            fs.Flush();
        }
    }

    [TestCase("basi3p02.png")]
    [Test]
    public void basi3p02(string fileName)
    {
        Directory.CreateDirectory("output");

        var fileData = File.ReadAllBytes(Path.Combine("samples", fileName));
        var png = new PNG();

        using var stuff = png.ReadImage(fileData);
        stuff.Flush();
        var path = Path.Combine("output", Path.ChangeExtension(fileName, ".ppm"));
        Console.WriteLine(Path.Combine(Directory.GetCurrentDirectory(), path));
        using var fs = new FileStream(path, FileMode.Create);
        stuff.Position = 0;
        stuff.CopyTo(fs);
        fs.Flush();
    }

    [TestCase("basn0g08.png")]
    [Test]
    public void basn0g08(string fileName)
    {
        Directory.CreateDirectory("output");

        var fileData = File.ReadAllBytes(Path.Combine("samples", fileName));
        var png = new PNG();

        using var stuff = png.ReadImage(fileData);
        stuff.Flush();
        var path = Path.Combine("output", Path.ChangeExtension(fileName, ".ppm"));
        Console.WriteLine(Path.Combine(Directory.GetCurrentDirectory(), path));
        using var fs = new FileStream(path, FileMode.Create);
        stuff.Position = 0;
        stuff.CopyTo(fs);
        fs.Flush();
    }

    [TestCase("basn0g01.png")]
    [Test]
    public void basn0g01(string fileName)
    {
        Directory.CreateDirectory("output");

        var fileData = File.ReadAllBytes(Path.Combine("samples", fileName));
        var png = new PNG();

        using var stuff = png.ReadImage(fileData);
        stuff.Flush();
        var path = Path.Combine("output", Path.ChangeExtension(fileName, ".ppm"));
        Console.WriteLine(Path.Combine(Directory.GetCurrentDirectory(), path));
        using var fs = new FileStream(path, FileMode.Create);
        stuff.Position = 0;
        stuff.CopyTo(fs);
        fs.Flush();
    }

    [TestCase("basn0g04.png")]
    [Test]
    public void basn0g04(string fileName)
    {
        Directory.CreateDirectory("output");

        var fileData = File.ReadAllBytes(Path.Combine("samples", fileName));
        var png = new PNG();

        using var stuff = png.ReadImage(fileData);
        stuff.Flush();
        var path = Path.Combine("output", Path.ChangeExtension(fileName, ".ppm"));
        Console.WriteLine(Path.Combine(Directory.GetCurrentDirectory(), path));
        using var fs = new FileStream(path, FileMode.Create);
        stuff.Position = 0;
        stuff.CopyTo(fs);
        fs.Flush();
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
        Console.WriteLine(Path.Combine(Directory.GetCurrentDirectory(), path));
        using var fs = new FileStream(path, FileMode.Create);
        stuff.Position = 0;
        stuff.CopyTo(fs);
        fs.Flush();
    }

    [TestCase("basn6a08.png")]
    [Test]
    public void basn6a08(string fileName)
    {
        Directory.CreateDirectory("output");

        var fileData = File.ReadAllBytes(Path.Combine("samples", fileName));
        var png = new PNG();

        using var stuff = png.ReadImage(fileData);
        stuff.Flush();
        var path = Path.Combine("output", Path.ChangeExtension(fileName, ".ppm"));
        Console.WriteLine(Path.Combine(Directory.GetCurrentDirectory(), path));
        using var fs = new FileStream(path, FileMode.Create);
        stuff.Position = 0;
        stuff.CopyTo(fs);
        fs.Flush();
    }
}
