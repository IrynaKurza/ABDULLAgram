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
            Regular.ClearExtent();
            Premium.ClearExtent();
        }

        [SetUp]
        public void Setup()
        {
            Sent.ClearExtent();
            Regular.ClearExtent();
        }

        [SetUp]
        public void Setup()
        {
            User.ClearExtent();
            Sent.ClearExtent(); // only if Sent has extent and is used in these tests
        }
        
        [Test]
        public void MarkAsRead_SetsReverseConnection()
        {
            var sender = new TestUser("Sender1"); // Unique name
            var reader = new TestUser("Reader1"); // Unique name
            var chat = new Group { Name = "Chat" };

            sender.JoinChat(chat);
            reader.JoinChat(chat);

            // Create Sent component (User/Chat are now part of Message, not Sent)
            var msg = new Sent(
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
            var sender = new TestUser("Sender2"); // Unique name
            var reader = new TestUser("Reader2"); // Unique name
            var chat = new Group { Name = "Chat" };

            sender.JoinChat(chat);
            reader.JoinChat(chat);

            var msg = new Sent(
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