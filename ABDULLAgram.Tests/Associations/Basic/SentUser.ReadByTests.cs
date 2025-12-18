using ABDULLAgram.Chats;
using ABDULLAgram.Messages;
using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Associations.Basic
{
    [TestFixture]
    public class SentReadByAssociationTests
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
            User.ClearExtent();
            Sent.ClearExtent();
        }
        
        [Test]
        public void MarkAsRead_SetsReverseConnection()
        {
            var sender = new TestUser("Sender1");
            var reader = new TestUser("Reader1");
            var chat = new Chat(ChatType.Group) { Name = "Chat" };

            sender.JoinChat(chat);
            reader.JoinChat(chat);

            var msg = new Sent(
                sender,
                chat,
                DateTime.Now.AddMinutes(-5),
                DateTime.Now.AddMinutes(-4),
                null,
                null
            );

            msg.MarkAsRead(reader);

            Assert.That(msg.ReadByUsers, Contains.Item(reader));
            Assert.That(reader.ReadMessages, Contains.Item(msg));
        }
        

        [Test]
        public void MarkAsRead_Twice_DoesNotDuplicate()
        {
            var sender = new TestUser("Sender2");
            var reader = new TestUser("Reader2");
            var chat = new Chat(ChatType.Group) { Name = "Chat" };

            sender.JoinChat(chat);
            reader.JoinChat(chat);

            var msg = new Sent(
                sender,
                chat,
                DateTime.Now.AddMinutes(-5),
                DateTime.Now.AddMinutes(-4),
                null,
                null
            );

            msg.MarkAsRead(reader);
            msg.MarkAsRead(reader);

            Assert.That(msg.ReadByUsers.Count, Is.EqualTo(1));
            Assert.That(reader.ReadMessages.Count, Is.EqualTo(1));
        }
    }
}