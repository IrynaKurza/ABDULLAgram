using System;
using ABDULLAgram.Attachments;
using ABDULLAgram.Users;
using NUnit.Framework;

namespace ABDULLAgram.Tests.Attachments
{
    internal class TestUserForText : User
    {
    }

    [TestFixture]
    public class AttachmentsTests
    {
        [SetUp]
        public void SetUp()
        {
            Text.ClearExtent();
        }

        [Test]
        public void AddMentionedUser_UpdatesBothSides()
        {
            var user = new TestUserForText();
            var text = new Text("Hello @user", false);

            text.AddMentionedUser(user);

            Assert.That(text.MentionedUsers, Does.Contain(user));
            Assert.That(user.MentionedInTexts, Does.Contain(text));
        }

        [Test]
        public void RemoveMentionedUser_UpdatesBothSides()
        {
            var user = new TestUserForText();
            var text = new Text("Hello @user", false);
            text.AddMentionedUser(user);

            text.RemoveMentionedUser(user);

            Assert.That(text.MentionedUsers, Does.Not.Contain(user));
            Assert.That(user.MentionedInTexts, Does.Not.Contain(text));
        }

        [Test]
        public void AddMentionedUser_Duplicate_ThrowsInvalidOperationException()
        {
            var user = new TestUserForText();
            var text = new Text("Hello @user", false);
            text.AddMentionedUser(user);

            Assert.Throws<InvalidOperationException>(() => text.AddMentionedUser(user));
        }

        [Test]
        public void RemoveMentionedUser_NotPresent_ThrowsInvalidOperationException()
        {
            var user = new TestUserForText();
            var text = new Text("Hello @user", false);

            Assert.Throws<InvalidOperationException>(() => text.RemoveMentionedUser(user));
        }

        [Test]
        public void AddMentionedUser_Null_ThrowsArgumentNullException()
        {
            var text = new Text("Hello", false);

            Assert.Throws<ArgumentNullException>(() => text.AddMentionedUser(null));
        }
    }
}
