using ABDULLAgram.Support;
using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Support
{
    internal class TestUser : Regular { 
        public TestUser(string name) : base(name, "+" + name.GetHashCode(), true, 1) {} 
    }

    // ============================================================
    // COMPOSITION TESTS: User owns Folder
    // Tests strong ownership - folders can't exist without owner
    // ============================================================
    
    [TestFixture]
    public class FolderTests
    {
        [SetUp]
        public void Setup()
        {
            Regular.ClearExtent();
        }

        // TEST: Factory method creates folder and establishes ownership
        [Test]
        public void CreateFolder_WithValidName_CreatesFolderAndRegistersOnUser()
        {
            var user = new TestUser("Alice");

            // Act - User creates folder (composition enforced)
            var folder = user.CreateFolder("Work");

            // Assert - Bidirectional link established
            Assert.That(folder, Is.Not.Null);
            Assert.That(folder.Name, Is.EqualTo("Work"));
            Assert.That(folder.Owner, Is.EqualTo(user)); // Folder knows owner
            Assert.That(user.Folders, Does.Contain(folder)); // User knows folder
        }

        // TEST: Validation on folder creation
        [Test]
        public void CreateFolder_WithEmptyOrWhitespaceName_ThrowsArgumentException()
        {
            var user = new TestUser("Alice");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => user.CreateFolder(""));
            Assert.Throws<ArgumentException>(() => user.CreateFolder("   "));
        }

        // TEST: Can't set empty name after creation
        [Test]
        public void Set_Name_EmptyOrWhitespace_ThrowsArgumentException()
        {
            var user = new TestUser("Alice");
            var folder = user.CreateFolder("Work");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => folder.Name = "");
            Assert.Throws<ArgumentException>(() => folder.Name = "   ");
        }

        // TEST: Can change folder name
        [Test]
        public void Set_Name_Valid_UpdatesValue()
        {
            var user = new TestUser("Alice");
            var folder = user.CreateFolder("Work");

            // Act
            folder.Name = "Travel";

            // Assert
            Assert.That(folder.Name, Is.EqualTo("Travel"));
        }

        // TEST: COMPOSITION - Deleting folder from user's perspective
        [Test]
        public void DeleteFolder_RemovesFolderFromUser()
        {
            var user = new TestUser("Alice");
            var folder = user.CreateFolder("Work");

            // Act
            user.DeleteFolder(folder);

            // Assert - Folder removed from user's collection
            Assert.That(user.Folders, Does.Not.Contain(folder));
        }

        // TEST: Can't delete folder that doesn't belong to user
        [Test]
        public void DeleteFolder_ForFolderNotOwnedByUser_ThrowsInvalidOperationException()
        {
            var owner = new TestUser("Alice");
            var anotherUser = new TestUser("Bob");
            var folder = owner.CreateFolder("Work");

            // Act & Assert - Can't delete someone else's folder
            Assert.Throws<InvalidOperationException>(() => anotherUser.DeleteFolder(folder));
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