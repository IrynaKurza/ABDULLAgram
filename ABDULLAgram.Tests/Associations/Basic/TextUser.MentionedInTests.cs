using ABDULLAgram.Attachments;
using ABDULLAgram.Chats;
using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Associations.Basic
{
    internal class TestUserForText : User
    {
        public TestUserForText()
            : base("TestUser", "555-0199", true)
        {
            InitializeAsRegular(1);
        }
    }

    internal class TestChat : Chat
    {
        public TestChat() : base(ChatType.Group)
        {
            Name = "Test Chat";
        }
    }

    [TestFixture]
    public class AttachmentsTests
    {
        [SetUp]
        public void SetUp()
        {
            Text.ClearExtent();
            User.ClearExtent();
        }

        [Test]
        public void AddMentionedUser_UpdatesBothSides()
        {
            var user = new TestUserForText();
            var chat = new TestChat();
            
            chat.AddMember(user);
            
            var text = new Text(user, chat, "Hello @user", false);

            text.AddMentionedUser(user);

            Assert.That(text.MentionedUsers, Does.Contain(user));
            Assert.That(user.MentionedInTexts, Does.Contain(text));
        }

        [Test]
        public void RemoveMentionedUser_UpdatesBothSides()
        {
            var user = new TestUserForText();
            var chat = new TestChat();
            
            chat.AddMember(user);
            
            var text = new Text(user, chat, "Hello @user", false);
            text.AddMentionedUser(user);

            text.RemoveMentionedUser(user);

            Assert.That(text.MentionedUsers, Does.Not.Contain(user));
            Assert.That(user.MentionedInTexts, Does.Not.Contain(text));
        }

        [Test]
        public void AddMentionedUser_Duplicate_ThrowsInvalidOperationException()
        {
            var user = new TestUserForText();
            var chat = new TestChat();
            
            chat.AddMember(user);
            
            var text = new Text(user, chat, "Hello @user", false);
            text.AddMentionedUser(user);

            Assert.Throws<InvalidOperationException>(() => text.AddMentionedUser(user));
        }

        [Test]
        public void RemoveMentionedUser_NotPresent_ThrowsInvalidOperationException()
        {
            var user = new TestUserForText();
            var chat = new TestChat();
            
            chat.AddMember(user);
            
            var text = new Text(user, chat, "Hello @user", false);

            Assert.Throws<InvalidOperationException>(() => text.RemoveMentionedUser(user));
        }

        [Test]
        public void AddMentionedUser_Null_ThrowsArgumentNullException()
        {
            var user = new TestUserForText();
            var chat = new TestChat();
            
            chat.AddMember(user);

            var text = new Text(user, chat, "Hello", false);

            Assert.Throws<ArgumentNullException>(() => text.AddMentionedUser(null));
        }
    }
}