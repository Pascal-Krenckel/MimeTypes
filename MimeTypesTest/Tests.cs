namespace MimeTypesTest;

[TestClass]
public sealed class Tests
{
    [TestMethod]
    public void MimeTypesTest()
    {
        var mp4 = MimeTypes.MimeTypes.GetMimeTypes("test.mp4");
        Assert.IsTrue(mp4.Contains("video/mp4"));
        Assert.AreEqual(1, mp4.Count());


        var ogg = MimeTypes.MimeTypes.GetMimeTypes("test.ogg");
        Assert.IsTrue(ogg.Contains("video/ogg"));
        Assert.IsTrue(ogg.Contains("audio/ogg"));

        Assert.AreEqual(2, ogg.Count());
    }
}
