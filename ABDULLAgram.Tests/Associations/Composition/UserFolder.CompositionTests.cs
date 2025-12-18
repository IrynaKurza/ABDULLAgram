using ABDULLAgram.Support;
using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Associations.Composition
{
    [TestFixture]
    public class CompositionTests
    {

        private class TestUser : Regular
        {
            public TestUser(string name) : base(name, "+" + name.GetHashCode(), true, 1)
            {
            }
        }

        [SetUp]
        public void Setup()
        {
            Regular.ClearExtent();
        }
        
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
    }
}