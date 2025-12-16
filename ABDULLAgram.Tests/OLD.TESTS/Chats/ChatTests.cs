using ABDULLAgram.Chats;

namespace ABDULLAgram.Tests.Chats
{
    [TestFixture]
    public class ChatTests
    {
        // Since Chat is abstract, we use a concrete implementation (Group) for testing base properties.
        
        [Test]
        public void Set_Name_Empty_ThrowsArgumentException()
        {
            var chat = new Group();
            Assert.Throws<ArgumentException>(() => chat.Name = "");
            Assert.Throws<ArgumentException>(() => chat.Name = "   ");
        }

        [Test]
        public void Set_Name_Valid_SetsValue()
        {
            var chat = new Group();
            chat.Name = "General Chat";
            Assert.That(chat.Name, Is.EqualTo("General Chat"));
        }

        [Test]
        public void Set_CreatedAt_Future_ThrowsArgumentOutOfRangeException()
        {
            var chat = new Group();
            Assert.Throws<ArgumentOutOfRangeException>(() => chat.CreatedAt = DateTime.Now.AddDays(1));
        }

        [Test]
        public void Set_CreatedAt_Valid_SetsValue()
        {
            var chat = new Group();
            var past = DateTime.Now.AddDays(-1);
            chat.CreatedAt = past;
            Assert.That(chat.CreatedAt, Is.EqualTo(past));
        }
    }

    [TestFixture]
    public class GroupTests
    {
        [Test]
        public void Set_MaxParticipants_NegativeOrZero_ThrowsArgumentOutOfRangeException()
        {
            var group = new Group();
            Assert.Throws<ArgumentOutOfRangeException>(() => group.MaxParticipants = 0);
            Assert.Throws<ArgumentOutOfRangeException>(() => group.MaxParticipants = -5);
        }

        [Test]
        public void Set_Description_Null_ThrowsArgumentNullException()
        {
            var group = new Group();
            Assert.Throws<ArgumentNullException>(() => group.Description = null);
        }

        [Test]
        public void Set_Description_Valid_SetsValue()
        {
            var group = new Group();
            group.Description = "Official group";
            Assert.That(group.Description, Is.EqualTo("Official group"));
        }
    }

    [TestFixture]
    public class PrivateTests
    {
        [Test]
        public void Set_MaxParticipants_Invalid_ThrowsArgumentOutOfRangeException()
        {
            var pChat = new Private();
            // Private chats are strictly for 2 people
            Assert.Throws<ArgumentOutOfRangeException>(() => pChat.MaxParticipants = 3);
            Assert.Throws<ArgumentOutOfRangeException>(() => pChat.MaxParticipants = 1);
        }

        [Test]
        public void Set_MaxParticipants_Valid_SetsValue()
        {
            var pChat = new Private();
            pChat.MaxParticipants = 2;
            Assert.That(pChat.MaxParticipants, Is.EqualTo(2));
        }
    }
}