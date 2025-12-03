using ABDULLAgram.Users;
using ABDULLAgram.Chats;
using ABDULLAgram.Support;

namespace ABDULLAgram.Tests.QualifiedBasicAssociations
{
    [TestFixture]
    public class QualifiedAssociationTests
    {
        private Group? _testChat;
        private Regular? _user1;
        private Regular? _user2;

        [SetUp]
        public void SetUp()
        {
            Regular.ClearExtent();
            _testChat = new Group { Name = "Test Chat" };
            _user1 = new Regular("Alice", "+48111222333", true, 5);
            _user2 = new Regular("Bob", "+48222333444", true, 3);
        }

        // Tests will go here
    }

    [TestFixture]
    public class BasicAssociationTests
    {
        private Regular? _regularUser;
        private Premium? _premiumUser;
        private Stickerpack? _pack1;
        private Stickerpack? _pack2;

        [SetUp]
        public void SetUp()
        {
            Regular.ClearExtent();
            Premium.ClearExtent();
            _regularUser = new Regular("Alice", "+48111222333", true, 5);
            _premiumUser = new Premium("Bob", "+48222333444", true, DateTime.Now.AddDays(-30), DateTime.Now.AddDays(30));
            _pack1 = new Stickerpack { Name = "Funny Pack", IsPremium = false };
            _pack2 = new Stickerpack { Name = "Premium Pack", IsPremium = true };
        }

        // Tests 
    }
}