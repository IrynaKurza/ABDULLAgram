using ABDULLAgram.Chats;

namespace ABDULLAgram.Tests.Core.Chats
{
    [TestFixture]
    public class ChatTests
    {
        // Since Chat is abstract, we use a concrete implementation (Group) for testing base properties.
        
        [Test]
        public void Set_Name_Empty_ThrowsArgumentException()
        {
            var chat = new Chat(ChatType.Group);
            Assert.Throws<ArgumentException>(() => chat.Name = "");
            Assert.Throws<ArgumentException>(() => chat.Name = "   ");
        }

        [Test]
        public void Set_Name_Valid_SetsValue()
        {
            var chat = new Chat(ChatType.Group);
            chat.Name = "General Chat";
            Assert.That(chat.Name, Is.EqualTo("General Chat"));
        }

        [Test]
        public void Set_CreatedAt_Future_ThrowsArgumentOutOfRangeException()
        {
            var chat = new Chat(ChatType.Group);
            Assert.Throws<ArgumentOutOfRangeException>(() => chat.CreatedAt = DateTime.Now.AddDays(1));
        }

        [Test]
        public void Set_CreatedAt_Valid_SetsValue()
        {
            var chat = new Chat(ChatType.Group);
            var past = DateTime.Now.AddDays(-1);
            chat.CreatedAt = past;
            Assert.That(chat.CreatedAt, Is.EqualTo(past));
        }
    }
}