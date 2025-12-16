using ABDULLAgram.Users;
using ABDULLAgram.Support;

namespace ABDULLAgram.Tests.Support
{
    [TestFixture]
    public class StickerpackTests
    {
        private class TestUser : Regular { 
            public TestUser(string name) : base(name, "+" + name.GetHashCode(), true, 1) {} 
        }

        [SetUp]
        public void Setup()
        {
            Regular.ClearExtent();
            Folder.ClearExtent();
        }
        

        // TEST: Folder.Delete() removes from extent
        [Test]
        public void DeleteFolder_RemovesFromExtent()
        {
            var user = new TestUser("Bob");
            var folder = user.CreateFolder("Work");

            // Act
            folder.Delete();

            // Assert - Removed from extent
            Assert.That(Folder.GetAll(), Does.Not.Contain(folder));
            Assert.That(user.Folders, Does.Not.Contain(folder));
        }
    }
}