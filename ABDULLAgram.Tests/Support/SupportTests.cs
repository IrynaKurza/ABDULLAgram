using ABDULLAgram.Support;

namespace ABDULLAgram.Tests.Support
{
    [TestFixture]
    public class FolderTests
    {
        [Test]
        public void Set_Name_Empty_ThrowsArgumentException()
        {
            var folder = new Folder();
            Assert.Throws<ArgumentException>(() => folder.Name = "");
            Assert.Throws<ArgumentException>(() => folder.Name = "   ");
        }

        [Test]
        public void Set_Name_Valid_SetsValue()
        {
            var folder = new Folder();
            folder.Name = "Work";
            Assert.That(folder.Name, Is.EqualTo("Work"));
        }
    }

    [TestFixture]
    public class StickerpackTests
    {
        [Test]
        public void Set_Name_Empty_ThrowsArgumentException()
        {
            var pack = new Stickerpack();
            Assert.Throws<ArgumentException>(() => pack.Name = "");
            Assert.Throws<ArgumentException>(() => pack.Name = "   ");
        }

        [Test]
        public void Set_Name_Valid_SetsValue()
        {
            var pack = new Stickerpack();
            pack.Name = "Funny Cats";
            Assert.That(pack.Name, Is.EqualTo("Funny Cats"));
        }
    }
}