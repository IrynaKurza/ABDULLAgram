using ABDULLAgram.Chats;
using ABDULLAgram.Support;
using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Associations.Aggregation
{
    [TestFixture]
    public class FolderChatAggregationTests
    {
        private class TestUser : User
        {
            public TestUser(string name)
                : base(name, "+" + name.GetHashCode(), true)
            {
                InitializeAsRegular(1);
            }
        }

        [SetUp]
        public void Setup()
        {
            Folder.ClearExtent();
            User.ClearExtent();
        }

        [Test]
        public void AddChat_ToFolder_CreatesReverseLink()
        {
            var user = new TestUser("Alice");
            var folder = user.CreateFolder("Work");
            var chat = new Chat(ChatType.Group) { Name = "General" };

            folder.AddChat(chat);

            Assert.That(folder.Chats, Contains.Item(chat));
            Assert.That(chat.Folders, Contains.Item(folder));
        }

        [Test]
        public void RemoveChat_FromFolder_RemovesReverseLink()
        {
            var user = new TestUser("Alice");
            var folder = user.CreateFolder("Work");
            var chat = new Chat(ChatType.Group) { Name = "General" };

            folder.AddChat(chat);
            folder.RemoveChat(chat);

            Assert.That(folder.Chats, Does.Not.Contain(chat));
            Assert.That(chat.Folders, Does.Not.Contain(folder));
        }

        [Test]
        public void Folder_CannotContainMoreThan100Chats()
        {
            var user = new TestUser("Alice");
            var folder = user.CreateFolder("BigFolder");

            for (int i = 0; i < 100; i++)
            {
                folder.AddChat(new Chat(ChatType.Group) { Name = "Chat " + i });
            }

            Assert.Throws<InvalidOperationException>(() =>
                folder.AddChat(new Chat(ChatType.Group) { Name = "Overflow" }));
        }

        [Test]
        public void DeletingFolder_DoesNotDeleteChat()
        {
            var user = new TestUser("Alice");
            var folder = user.CreateFolder("Work");
            var chat = new Chat(ChatType.Group) { Name = "General" };

            folder.AddChat(chat);
            folder.Delete();

            Assert.That(chat.Folders, Does.Not.Contain(folder));
        }
    }
}
