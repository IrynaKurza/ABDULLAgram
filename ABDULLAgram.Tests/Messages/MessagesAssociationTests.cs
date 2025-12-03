using ABDULLAgram.Messages;
using ABDULLAgram.Users;
using ABDULLAgram.Chats;
using ABDULLAgram.Attachments;

namespace ABDULLAgram.Tests.Messages
{
    [TestFixture]
    public class MessageAssociationTests
    {
        private class TestUser : Regular { 
            public TestUser(string name) : base(name, "+" + name.GetHashCode(), true, 1) {} 
        }
        private class TestChat : Group { 
            public TestChat() { Name = "Test Group"; Description = "Desc"; } 
        }
        private class TestMessage : ABDULLAgram.Messages.Message {
            public TestMessage(User u, Chat c) : base(u, c) { }
        }

        private User _user1;
        private User _user2;
        private Chat _chat1;
        private Chat _chat2;

        [SetUp]
        public void Setup()
        {
            Regular.ClearExtent();
            _user1 = new TestUser("Alice");
            _user2 = new TestUser("Bob");
            _chat1 = new TestChat();
            _chat2 = new TestChat();
        }

        [Test]
        public void Constructor_Establishes_DoubleLinks()
        {
            // Act
            var msg = new TestMessage(_user1, _chat1);

            Assert.That(msg.Sender, Is.EqualTo(_user1));
            Assert.That(msg.TargetChat, Is.EqualTo(_chat1));

            Assert.That(_user1.SentMessages, Contains.Item(msg));
            
            Assert.That(_chat1.History, Contains.Item(msg));
        }

        [Test]
        public void Constructor_NullArguments_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new TestMessage(null, _chat1));
            Assert.Throws<ArgumentNullException>(() => new TestMessage(_user1, null));
        }

        [Test]
        public void Changing_Sender_Updates_ReverseConnections()
        {
            var msg = new TestMessage(_user1, _chat1);

            // Act: Change sender from Alice to Bob
            msg.Sender = _user2;

            // Assert
            Assert.That(msg.Sender, Is.EqualTo(_user2));
            
            // Alice should no longer have the message
            Assert.That(_user1.SentMessages, Does.Not.Contain(msg));
            
            // Bob should now have the message
            Assert.That(_user2.SentMessages, Contains.Item(msg));
        }

        [Test]
        public void Changing_Chat_Updates_History()
        {
            var msg = new TestMessage(_user1, _chat1);

            // Act: Move message to Chat 2
            msg.TargetChat = _chat2;

            // Assert
            Assert.That(_chat1.History, Does.Not.Contain(msg));
            Assert.That(_chat2.History, Contains.Item(msg));
        }

        [Test]
        public void Delete_Removes_All_Links()
        {
            var msg = new TestMessage(_user1, _chat1);

            // Act
            msg.Delete();

            // Assert
            Assert.That(_user1.SentMessages, Is.Empty);
            Assert.That(_chat1.History, Is.Empty);
        }
    }
}