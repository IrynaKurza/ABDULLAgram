using System;
using ABDULLAgram.Support;
using ABDULLAgram.Users;
using NUnit.Framework;

namespace ABDULLAgram.Tests.Support
{
    internal class TestUser : Regular { 
        public TestUser(string name) : base(name, "+" + name.GetHashCode(), true, 1) {} 
    }

    [TestFixture]
    public class FolderTests
    {
        // Clear previous users before every test to prevent phone number collisions
        [SetUp]
        public void Setup()
        {
            Regular.ClearExtent();
        }

        [Test]
        public void CreateFolder_WithValidName_CreatesFolderAndRegistersOnUser()
        {
            var user = new TestUser("Alice");

            var folder = user.CreateFolder("Work");

            Assert.That(folder, Is.Not.Null);
            Assert.That(folder.Name, Is.EqualTo("Work"));
            Assert.That(folder.Owner, Is.EqualTo(user));
            Assert.That(user.Folders, Does.Contain(folder));
        }

        [Test]
        public void CreateFolder_WithEmptyOrWhitespaceName_ThrowsArgumentException()
        {
            var user = new TestUser("Alice");

            Assert.Throws<ArgumentException>(() => user.CreateFolder(""));
            Assert.Throws<ArgumentException>(() => user.CreateFolder("   "));
        }

        [Test]
        public void Set_Name_EmptyOrWhitespace_ThrowsArgumentException()
        {
            var user = new TestUser("Alice");
            var folder = user.CreateFolder("Work");

            Assert.Throws<ArgumentException>(() => folder.Name = "");
            Assert.Throws<ArgumentException>(() => folder.Name = "   ");
        }

        [Test]
        public void Set_Name_Valid_UpdatesValue()
        {
            var user = new TestUser("Alice");
            var folder = user.CreateFolder("Work");

            folder.Name = "Travel";

            Assert.That(folder.Name, Is.EqualTo("Travel"));
        }

        [Test]
        public void DeleteFolder_RemovesFolderFromUser()
        {
            var user = new TestUser("Alice");
            var folder = user.CreateFolder("Work");

            user.DeleteFolder(folder);

            Assert.That(user.Folders, Does.Not.Contain(folder));
        }

        [Test]
        public void DeleteFolder_ForFolderNotOwnedByUser_ThrowsInvalidOperationException()
        {
            var owner = new TestUser("Alice");
            var anotherUser = new TestUser("Bob");
            var folder = owner.CreateFolder("Work");

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