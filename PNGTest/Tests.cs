using PngSmile;

namespace PNGTest
{

    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestHeader()
        {
            Assert.That(PNG.FileSignature, Has.Length.EqualTo(8));
        }

        [Test]
        public void TestIENDCrc()
        {
            uint ExpectedCrc = 0xae426082U;
            var crcEncoder = new CRC32();
            byte[] block = [(byte)'I', (byte)'E', (byte)'N', (byte)'D'];
            var gotCrc = crcEncoder.Crc(block.AsSpan());

            Assert.That(gotCrc, Is.EqualTo(ExpectedCrc));
        }

        [Test]
        public void TestIHDRCrc()
        {
            var length = 0x0D;
            byte[] block = [0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x0C, 0x30, 0x00, 0x00, 0x0F, 0x50, 0x08, 0x02, 0x00, 0x00, 0x00];
            uint ExpectedCrc = 0x2CC683A4U;
            var crcEncoder = new CRC32();
            var gotCrc = crcEncoder.Crc(block.AsSpan());
            Assert.Multiple(() =>
            {
                Assert.That(gotCrc, Is.EqualTo(ExpectedCrc));

                Assert.That(block, Has.Length.EqualTo(length + 4));
            });
        }
    }
}