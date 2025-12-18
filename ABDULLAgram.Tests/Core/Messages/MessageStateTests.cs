using ABDULLAgram.Attachments;
using ABDULLAgram.Chats;
using ABDULLAgram.Messages;
using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Core.Messages
{
    [TestFixture]
    public class MessageStateTests
    {
        private class TestUser : Regular
        {
            public TestUser(string name) : base(name, "+" + name.GetHashCode(), true, 1) { }
        }
        private class TestChat : Group { public TestChat() { Name = "G"; } }

        [SetUp]
        public void Setup()
        {
            Regular.ClearExtent();
            Draft.ClearExtent();
            Sent.ClearExtent();
            Text.ClearExtent();
        }

        [Test]
        public void NewMessage_IsCreatedAsDraft()
        {
            var user = new TestUser("Alice");
            var chat = new TestChat();
            chat.AddMember(user);

            // Act - Create a Text message (inherits Message)
            var msg = new Text(user, chat, "Hello", false);

            // Assert
            Assert.That(msg.IsDraft, Is.True);
            Assert.That(msg.IsSent, Is.False);
            Assert.That(msg.Draft, Is.Not.Null);
            Assert.That(msg.Sent, Is.Null);
            
            // Check extent - Draft should be registered
            Assert.That(Draft.GetAll().Count, Is.EqualTo(1));
            Assert.That(Draft.GetAll().Contains(msg.Draft), Is.True);
        }

        [Test]
        public void Draft_SendMessage_TransitionsToSent()
        {
            var user = new TestUser("Alice");
            var chat = new TestChat();
            chat.AddMember(user);
            var msg = new Text(user, chat, "Hello", false);
            var initialDraft = msg.Draft;

            // Act - Send the message (Draft -> Sent)
            initialDraft!.SendMessage(msg);

            // Assert
            Assert.That(msg.IsDraft, Is.False);
            Assert.That(msg.IsSent, Is.True);
            Assert.That(msg.Draft, Is.Null);
            Assert.That(msg.Sent, Is.Not.Null);

            // Check extents
            Assert.That(Draft.GetAll(), Does.Not.Contain(initialDraft)); // Draft deleted from extent
            Assert.That(Sent.GetAll(), Contains.Item(msg.Sent)); // Sent added to extent
        }

        [Test]
        public void Draft_SendMessage_WrongMessage_ThrowsException()
        {
            var user = new TestUser("Alice");
            var chat = new TestChat();
            chat.AddMember(user);
            
            var msg1 = new Text(user, chat, "Msg1", false);
            var msg2 = new Text(user, chat, "Msg2", false);

            // Act & Assert - Try to send msg1 using msg2's draft
            Assert.Throws<InvalidOperationException>(() => msg2.Draft!.SendMessage(msg1));
        }

        [Test]
        public void Message_Delete_RemovesCompositionParts()
        {
            var user = new TestUser("Alice");
            var chat = new TestChat();
            chat.AddMember(user);
            
            var msg = new Text(user, chat, "Hello", false);
            var draft = msg.Draft;

            // Act
            msg.Delete();

            // Assert
            Assert.That(Draft.GetAll(), Does.Not.Contain(draft));
        }

        [Test]
        public void Sent_Message_Delete_RemovesSentPart()
        {
            var user = new TestUser("Alice");
            var chat = new TestChat();
            chat.AddMember(user);
            
            var msg = new Text(user, chat, "Hello", false);
            msg.Draft!.SendMessage(msg); // Convert to Sent
            var sent = msg.Sent;

            // Act
            msg.Delete();

            // Assert
            Assert.That(Sent.GetAll(), Does.Not.Contain(sent));
        }
    }
}