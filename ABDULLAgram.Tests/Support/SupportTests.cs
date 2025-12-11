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

        // TEST: COMPOSITION - User deletion deletes all folders
        [Test]
        public void DeleteUser_DeletesAllOwnedFolders()
        {
            var user = new TestUser("Alice");
            var folder1 = user.CreateFolder("Work");
            var folder2 = user.CreateFolder("Personal");

            var initialFolderCount = Folder.GetAll().Count;

            // Act - Delete user (composition)
            user.DeleteUser();

            // Assert - All folders removed from extent
            Assert.That(Folder.GetAll().Count, Is.LessThan(initialFolderCount));
            Assert.That(Folder.GetAll(), Does.Not.Contain(folder1));
            Assert.That(Folder.GetAll(), Does.Not.Contain(folder2));
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