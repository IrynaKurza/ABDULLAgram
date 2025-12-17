using ABDULLAgram.Attachments;
using ABDULLAgram.Chats;
using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Core.Messages
{
    [TestFixture]
    public class MessageAddRemoveTests
    {
        private class TestUser : Regular { 
            public TestUser(string name) : base(name, "+" + name.GetHashCode(), true, 1) {} 
        }
        private class TestChat : Chat { 
            public TestChat() : base(ChatType.Group) { Name = "Test Group"; } 
        }

        // Counter to ensure unique emoji codes across tests
        private int _emojiCounter = 0;

        // Helper: Create a sticker with unique emojiCode
        private Sticker CreateOrphanMessage()
        {
            var emoji = "🎨" + _emojiCounter;
            _emojiCounter++;
            return new Sticker(emoji, Sticker.BackgroundTypeEnum.Transparent);
        }

        [SetUp]
        public void Setup()
        {
            Regular.ClearExtent();
            Sticker.ClearExtent();
            _emojiCounter = 0;
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