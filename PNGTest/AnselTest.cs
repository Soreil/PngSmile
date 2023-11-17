using PngSmile;

namespace PNGTest
{
    public class AnselTest
    {
        private readonly string fileName = "samples/anseladams.png";
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
            var labels =chunks.Select(c => c.ChunkTypeString()).ToList();
        }
    }
}