using PngSmile;

using System.IO;

namespace PNGTest
{
    public class AnselTest
    {
        private readonly string fileName = "samples/basn2c08.png";
        private PNG p;
        private byte[] file;

        [SetUp]
        public void SetUp()
        {
            p = new PNG();
            file = File.ReadAllBytes(fileName);
        }

        [Test]
        public void TestIsPNG()
        {
            var isPng = PNG.IsPNG(file);

            Assert.That(isPng);
        }

        [Test]
        public void TestDecodeChunk()
        {
            var read = PNG.DecodeChunk(file.AsSpan()[8..], out var chunk);
            Assert.Multiple(() =>
            {
                Assert.That(chunk.Length, Is.EqualTo(0xd));
                Assert.That(read, Is.EqualTo(0xd + PNG.MinimalChunkSize));
            });
            var type = chunk.ChunkTypeString();
            Assert.That(type, Is.EqualTo("IHDR"));
        }

        [Test]
        public void TestDecodeChunkTyped()
        {
            var read = PNG.DecodeChunk(file.AsSpan()[8..], out var chunk);
            Assert.Multiple(() =>
            {
                Assert.That(chunk.Length, Is.EqualTo(0xd));
                Assert.That(read, Is.EqualTo(0xd + PNG.MinimalChunkSize));
            });
            var type = chunk.ChunkTypeString();
            Assert.That(type, Is.EqualTo("IHDR"));

            var ihdr = new IHDR(chunk);
        }

        [Test]
        public void ReadAllChunks()
        {
            var input = file.AsSpan()[8..];

            List<Chunk> chunks = [];
            while (input.Length != 0)
            {
                var read = PNG.DecodeChunk(input, out var chunk);
                Assert.That(read, Is.EqualTo(chunk.Length + PNG.MinimalChunkSize));

                chunks.Add(chunk);
                input = input[read..];
            }

            var labels = chunks.Select(c => c.ChunkTypeString()).ToList();
            labels.ForEach(Console.WriteLine);

            IHDR? ihdr = null;
            GAMA? gama = null;
            CHRM? chrm = null;
            List<IDAT> idatChunks = [];

            foreach (var chunk in chunks)
            {
                switch (chunk.ChunkTypeString())
                {
                    case "IHDR":
                        {
                            ihdr = new IHDR(chunk);
                            Assert.That(idatChunks, Is.Empty);
                        }
                        break;
                    case "gAMA":
                        {
                            gama = new GAMA(chunk);
                            Assert.That(idatChunks, Is.Empty);
                        }
                        break;
                    case "cHRM":
                        {
                            chrm = new CHRM(chunk);
                            Assert.That(idatChunks, Is.Empty);
                        }
                        break;
                    case "IDAT":
                        {
                            Assert.That(ihdr, Is.Not.Null);
                            var idat = new IDAT(chunk, ihdr);
                            idatChunks.Add(idat);
                        }
                        break;
                    case "IEND":
                        {
                            Assert.Multiple(() =>
                            {
                                Assert.That(chunk.Length, Is.EqualTo(0));
                                Assert.That(idatChunks, Is.Not.Empty);
                            });
                        }

                        break;
                    default:
                        throw new NotImplementedException($"Unhandled chunk type:{chunk.ChunkTypeString()}");
                }
            }

            foreach (var idat in idatChunks)
            {
                var dat = new Span<byte>(idat.Stream.Decode());


                using var fs = new FileStream("output.ppm", FileMode.Create);
                using var sw = new StreamWriter(fs);
                sw.Write($"P3\n{ihdr.Width} {ihdr.Height}\n255\n");

                Span<byte> previousSpan = [];
                for (int i = 0; i < ihdr.Height; i++)
                {
                    var filterByte = dat[(int)ihdr.Width * 3 * i + i];

                    var startIndex = (int)ihdr.Width * 3 * i + i + 1;
                    var span = dat.Slice(startIndex, (int)ihdr.Width * 3);

                    var filteredSpan = PNG.Filter(span, previousSpan, filterByte);

                    for (int j = 0; j < ihdr.Width; j++)
                    {
                        sw.Write($"{(int)(filteredSpan[j * 3])} {(int)(filteredSpan[j * 3 + 1])} {(int)(filteredSpan[j * 3 + 2])}\n");
                    }

                    previousSpan = filteredSpan;
                }

                sw.Flush();
            }
        }
    }
}
