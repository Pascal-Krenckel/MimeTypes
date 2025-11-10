using MimeTypes;

namespace MimeTypesTest;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void MimeTypesTest()
    {
        var mp4 = MimeTypes.MimeTypes.GetMimeTypes("test.mp4");
        Assert.That(mp4.Contains("video/mp4"));
        Assert.That(mp4.Count(), Is.EqualTo(1));
       

        var ogg = MimeTypes.MimeTypes.GetMimeTypes("test.ogg");
        Assert.That(ogg.Contains("video/ogg"));
        Assert.That(ogg.Contains("audio/ogg"));

        Assert.That(ogg.Count(), Is.EqualTo(2));
    }
}