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
            var p = new PngSmile.PNG();
            Assert.That(p, Is.Not.Null);
            Assert.That(p.FileSignature, Has.Length.EqualTo(8));
        }
    }
}