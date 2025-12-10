using ABDULLAgram.Attachments;
using ABDULLAgram.Users;
using ABDULLAgram.Chats;

namespace ABDULLAgram.Tests.Attachments
{
    internal class TestUserForText : User
    {
        public override int MaxSavedStickerpacks => 10;

        public TestUserForText()
        {
            Username = "TestUser";
            PhoneNumber = "555-0199"; 
            IsOnline = true;
        }
    }

    internal class TestChat : Chat
    {
        public TestChat()
        {
            Name = "Test Chat";
        }
    }

    // ============================================================
    // BASIC ASSOCIATION TESTS: Text ↔ User (mentions)
    // Tests many-to-many association for user mentions in text
    // ============================================================
    
    [TestFixture]
    public class AttachmentsTests
    {
        [SetUp]
        public void SetUp()
        {
            Text.ClearExtent();
        }

        // TEST: Adding mention creates bidirectional link
        [Test]
        public void AddMentionedUser_UpdatesBothSides()
        {
            var user = new TestUserForText();
            var chat = new TestChat();
            
            var text = new Text(user, chat, "Hello @user", false);

            // Act
            text.AddMentionedUser(user);

            // Assert - Both sides updated
            Assert.That(text.MentionedUsers, Does.Contain(user)); // Text knows user
            Assert.That(user.MentionedInTexts, Does.Contain(text)); // User knows text
        }

        // TEST: Removing mention updates both sides
        [Test]
        public void RemoveMentionedUser_UpdatesBothSides()
        {
            var user = new TestUserForText();
            var chat = new TestChat();
            
            var text = new Text(user, chat, "Hello @user", false);
            text.AddMentionedUser(user);

            // Act
            text.RemoveMentionedUser(user);

            // Assert - Both sides updated
            Assert.That(text.MentionedUsers, Does.Not.Contain(user));
            Assert.That(user.MentionedInTexts, Does.Not.Contain(text));
        }

        // TEST: Can't mention same user twice
        [Test]
        public void AddMentionedUser_Duplicate_ThrowsInvalidOperationException()
        {
            var user = new TestUserForText();
            var chat = new TestChat();
            
            var text = new Text(user, chat, "Hello @user", false);
            text.AddMentionedUser(user);

            // Act & Assert - Duplicate mention not allowed
            Assert.Throws<InvalidOperationException>(() => text.AddMentionedUser(user));
        }

        // TEST: Can't remove user that's not mentioned
        [Test]
        public void RemoveMentionedUser_NotPresent_ThrowsInvalidOperationException()
        {
            var user = new TestUserForText();
            var chat = new TestChat();
            
            var text = new Text(user, chat, "Hello @user", false);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => text.RemoveMentionedUser(user));
        }

        // TEST: Null validation
        [Test]
        public void AddMentionedUser_Null_ThrowsArgumentNullException()
        {
            var user = new TestUserForText();
            var chat = new TestChat();

            var text = new Text(user, chat, "Hello", false);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => text.AddMentionedUser(null));
        }
    }
}