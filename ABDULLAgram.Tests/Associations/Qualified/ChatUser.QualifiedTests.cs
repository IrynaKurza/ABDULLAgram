using ABDULLAgram.Chats;
using ABDULLAgram.Support;
using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Associations.Qualified
{
    [TestFixture]
    public class QualifiedAssociationTests
    {
        private User? _user1;
        private User? _user2;
        private Chat? _testChat;

        [SetUp]
        public void SetUp()
        {
            User.ClearExtent();
            _user1 = new User("Alice", "+48111222333", true);
            _user1.InitializeAsRegular(5);
            _user2 = new User("Bob", "+48222333444", true);
            _user2.InitializeAsRegular(3);
            _testChat = new Chat(ChatType.Group) { Name = "Test Group", Description = "Test Description" };
        }

        [Test]
        public void AddMember_ValidUser_AddsToChat()
        {
            _testChat!.AddMember(_user1!);
            Assert.That(_testChat.Members.ContainsKey(_user1!.PhoneNumber), Is.True);
            Assert.That(_testChat.Members[_user1.PhoneNumber], Is.EqualTo(_user1));
        }

        [Test]
        public void AddMember_ReverseConnection_UserKnowsAboutChat()
        {
            _testChat!.AddMember(_user1!);
            Assert.That(_user1!.JoinedChats.Contains(_testChat), Is.True);
        }

        [Test]
        public void AddMember_DuplicatePhoneNumber_ThrowsException()
        {
            _testChat!.AddMember(_user1!);
            Assert.Throws<InvalidOperationException>(() => _testChat.AddMember(_user1));
        }

        [Test]
        public void AddMember_NullUser_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _testChat!.AddMember(null!));
        }

        [Test]
        public void RemoveMember_ExistingUser_RemovesFromChat()
        {
            _testChat!.AddMember(_user1!);
            _testChat.RemoveMember(_user1!.PhoneNumber);
            Assert.That(_testChat.Members.ContainsKey(_user1.PhoneNumber), Is.False);
        }

        [Test]
        public void RemoveMember_ReverseConnection_UserDoesNotKnowAboutChat()
        {
            _testChat!.AddMember(_user1!);
            _testChat.RemoveMember(_user1!.PhoneNumber);
            Assert.That(_user1.JoinedChats.Contains(_testChat), Is.False);
        }

        [Test]
        public void RemoveMember_NonExistentUser_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => _testChat!.RemoveMember("+48999999999"));
        }

        [Test]
        public void GetMemberByPhoneNumber_ExistingUser_ReturnsUser()
        {
            _testChat!.AddMember(_user1!);
            var foundUser = _testChat.GetMemberByPhoneNumber(_user1!.PhoneNumber);
            Assert.That(foundUser, Is.EqualTo(_user1));
        }

        [Test]
        public void GetMemberByPhoneNumber_NonExistentUser_ReturnsNull()
        {
            var foundUser = _testChat!.GetMemberByPhoneNumber("+48999999999");
            Assert.That(foundUser, Is.Null);
        }

        [Test]
        public void UpdateMemberPhoneNumber_ValidChange_UpdatesDictionary()
        {
            _testChat!.AddMember(_user1!);
            string oldPhone = _user1.PhoneNumber;
            string newPhone = "+48555666777";
            _testChat.UpdateMemberPhoneNumber(oldPhone, newPhone);
            Assert.That(_testChat.Members.ContainsKey(oldPhone), Is.False);
            Assert.That(_testChat.Members.ContainsKey(newPhone), Is.True);
            Assert.That(_testChat.Members[newPhone], Is.EqualTo(_user1));
        }

        [Test]
        public void JoinChat_FromUserSide_CreatesProperConnection()
        {
            _user1!.JoinChat(_testChat!);
            Assert.That(_user1.JoinedChats.Contains(_testChat), Is.True);
            Assert.That(_testChat.Members.ContainsKey(_user1.PhoneNumber), Is.True);
        }

        [Test]
        public void LeaveChat_FromUserSide_RemovesConnection()
        {
            _user1!.JoinChat(_testChat!);
            _user1.LeaveChat(_testChat);
            Assert.That(_user1.JoinedChats.Contains(_testChat), Is.False);
            Assert.That(_testChat.Members.ContainsKey(_user1.PhoneNumber), Is.False);
        }
    }

    [TestFixture]
    public class BasicAssociationTests
    {
        private User? _regularUser;
        private User? _premiumUser;
        private Stickerpack? _pack1;
        private Stickerpack? _pack2;

        [SetUp]
        public void SetUp()
        {
            User.ClearExtent();
            _regularUser = new User("Alice", "+48111222333", true);
            _regularUser.InitializeAsRegular(5);
            _premiumUser = new User("Bob", "+48222333444", true);
            _premiumUser.InitializeAsPremium(DateTime.Now.AddDays(-30), DateTime.Now.AddDays(30));
            _pack1 = new Stickerpack("Funny Pack", _regularUser) { IsPremium = false };
            _pack2 = new Stickerpack("Premium Pack", _premiumUser) { IsPremium = true };
        }

        [Test]
        public void SaveStickerpack_ValidPack_AddsToBothSides()
        {
            _regularUser!.SaveStickerpack(_pack1!);
            Assert.That(_regularUser.SavedStickerpacks.Contains(_pack1), Is.True);
            Assert.That(_pack1!.SavedByUsers.Contains(_regularUser), Is.True);
        }

        [Test]
        public void SaveStickerpack_RegularUser_EnforcesMaxLimit()
        {
            for (int i = 0; i < 10; i++)
            {
                var pack = new Stickerpack($"Pack {i}", _regularUser) { IsPremium = false };
                _regularUser!.SaveStickerpack(pack);
            }
            var extraPack = new Stickerpack("Extra Pack", _regularUser) { IsPremium = false };
            Assert.Throws<InvalidOperationException>(() => _regularUser!.SaveStickerpack(extraPack));
        }

        [Test]
        public void SaveStickerpack_PremiumUser_NoLimit()
        {
            for (int i = 0; i < 50; i++)
            {
                var pack = new Stickerpack($"Pack {i}", _premiumUser) { IsPremium = false };
                _premiumUser!.SaveStickerpack(pack);
            }
            Assert.That(_premiumUser!.SavedStickerpacks.Count, Is.EqualTo(50));
        }

        [Test]
        public void SaveStickerpack_DuplicatePack_ThrowsException()
        {
            _regularUser!.SaveStickerpack(_pack1!);
            Assert.Throws<InvalidOperationException>(() => _regularUser.SaveStickerpack(_pack1));
        }

        [Test]
        public void SaveStickerpack_NullPack_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _regularUser!.SaveStickerpack(null!));
        }

        [Test]
        public void UnsaveStickerpack_ExistingPack_RemovesFromBothSides()
        {
            _regularUser!.SaveStickerpack(_pack1!);
            _regularUser.UnsaveStickerpack(_pack1);
            Assert.That(_regularUser.SavedStickerpacks.Contains(_pack1), Is.False);
            Assert.That(_pack1!.SavedByUsers.Contains(_regularUser), Is.False);
        }

        [Test]
        public void UnsaveStickerpack_NonSavedPack_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => _regularUser!.UnsaveStickerpack(_pack1!));
        }

        [Test]
        public void AddSavedByUser_FromPackSide_CreatesConnection()
        {
            _pack1!.AddSavedByUser(_regularUser!);
            Assert.That(_pack1.SavedByUsers.Contains(_regularUser), Is.True);
            Assert.That(_regularUser!.SavedStickerpacks.Contains(_pack1), Is.True);
        }

        [Test]
        public void RemoveSavedByUser_FromPackSide_RemovesConnection()
        {
            _pack1!.AddSavedByUser(_regularUser!);
            _pack1.RemoveSavedByUser(_regularUser!);
            Assert.That(_pack1.SavedByUsers.Contains(_regularUser), Is.False);
            Assert.That(_regularUser!.SavedStickerpacks.Contains(_pack1), Is.False);
        }

        [Test]
        public void MultipleUsers_CanSaveSamePack()
        {
            _regularUser!.SaveStickerpack(_pack1!);
            _premiumUser!.SaveStickerpack(_pack1!);
            Assert.That(_pack1!.SavedByUsers.Count, Is.EqualTo(2));
            Assert.That(_pack1.SavedByUsers.Contains(_regularUser), Is.True);
            Assert.That(_pack1.SavedByUsers.Contains(_premiumUser), Is.True);
        }

        [Test]
        public void OneUser_CanSaveMultiplePacks()
        {
            _regularUser!.SaveStickerpack(_pack1!);
            _regularUser.SaveStickerpack(_pack2!);
            Assert.That(_regularUser.SavedStickerpacks.Count, Is.EqualTo(2));
            Assert.That(_regularUser.SavedStickerpacks.Contains(_pack1), Is.True);
            Assert.That(_regularUser.SavedStickerpacks.Contains(_pack2), Is.True);
        }

        [Test]
        public void RegularUser_CanSaveUpToExactLimit()
        {
            for (int i = 0; i < 10; i++)
            {
                var pack = new Stickerpack($"Pack {i}", _regularUser) { IsPremium = false };
                _regularUser!.SaveStickerpack(pack);
            }
            Assert.That(_regularUser!.SavedStickerpacks.Count, Is.EqualTo(10));
        }

        [Test]
        public void UnsaveStickerpack_AllowsSavingAgain()
        {
            for (int i = 0; i < 10; i++)
            {
                var pack = new Stickerpack($"Pack {i}", _regularUser) { IsPremium = false };
                _regularUser!.SaveStickerpack(pack);
            }
            _regularUser!.UnsaveStickerpack(_regularUser.SavedStickerpacks.First());
            var newPack = new Stickerpack("New Pack", _regularUser) { IsPremium = false };
            Assert.DoesNotThrow(() => _regularUser.SaveStickerpack(newPack));
            Assert.That(_regularUser.SavedStickerpacks.Count, Is.EqualTo(10));
        }
    }
}