using ABDULLAgram.Messages;
using ABDULLAgram.Users;
using ABDULLAgram.Chats;
using ABDULLAgram.Attachments;

namespace ABDULLAgram.Tests.Messages
{
    [TestFixture]
    public class MessageAddRemoveTests
    {
        private class TestUser : Regular { 
            public TestUser(string name) : base(name, "+" + name.GetHashCode(), true, 1) {} 
        }
        private class TestChat : Group { 
            public TestChat() { Name = "Test Group"; } 
        }

        private Sticker CreateOrphanMessage()
        {
            return new Sticker(Sticker.BackgroundTypeEnum.Transparent);
        }

        [SetUp]
        public void Setup()
        {
            Regular.ClearExtent();
        }

        [Test]
        public void UserAddMessage_SetsSender()
        {
            var user = new TestUser("Alice");
            var msg = CreateOrphanMessage();

            user.AddMessage(msg);

            Assert.That(msg.Sender, Is.EqualTo(user));
            Assert.That(user.SentMessages, Contains.Item(msg));
        }

        [Test]
        public void UserRemoveMessage_UnsetsSender()
        {
            var user = new TestUser("Alice");
            var msg = CreateOrphanMessage();
            user.AddMessage(msg);

            user.RemoveMessage(msg);

            Assert.That(msg.Sender, Is.Null);
            Assert.That(user.SentMessages, Does.Not.Contain(msg));
        }

        [Test]
        public void ChatAddMessage_SetsTargetChat()
        {
            var chat = new TestChat();
            var msg = CreateOrphanMessage();

            chat.AddMessage(msg);

            Assert.That(msg.TargetChat, Is.EqualTo(chat));
            Assert.That(chat.History, Contains.Item(msg));
        }

        [Test]
        public void ChatRemoveMessage_UnsetsTargetChat()
        {
            var chat = new TestChat();
            var msg = CreateOrphanMessage();
            chat.AddMessage(msg);

            chat.RemoveMessage(msg);

            Assert.That(msg.TargetChat, Is.Null);
            Assert.That(chat.History, Does.Not.Contain(msg));
        }
    }
}