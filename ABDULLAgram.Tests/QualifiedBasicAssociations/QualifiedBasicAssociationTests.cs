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

        // ========== QUALIFIED ASSOCIATION TESTS (Chat ↔ User by phoneNumber) ==========

        [Test]
        public void AddMember_ValidUser_AddsToChat()
        {
            // Arrange & Act
            _testChat!.AddMember(_user1!);

            // Assert
            Assert.That(_testChat.Members.Count, Is.EqualTo(1));
            Assert.That(_testChat.Members.ContainsKey(_user1!.PhoneNumber), Is.True);
            Assert.That(_testChat.Members[_user1.PhoneNumber], Is.EqualTo(_user1));
        }

        [Test]
        public void AddMember_ReverseConnection_UserKnowsAboutChat()
        {
            // Arrange & Act
            _testChat!.AddMember(_user1!);

            // Assert - Check reverse connection
            Assert.That(_user1!.JoinedChats.Count, Is.EqualTo(1));
            Assert.That(_user1.JoinedChats.Contains(_testChat), Is.True);
        }

        [Test]
        public void AddMember_DuplicatePhoneNumber_ThrowsException()
        {
            // Arrange
            _testChat!.AddMember(_user1!);

            // Act & Assert - Cannot add same user twice
            Assert.Throws<InvalidOperationException>(() => _testChat.AddMember(_user1!));
        }

        [Test]
        public void AddMember_NullUser_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _testChat!.AddMember(null!));
        }

        [Test]
        public void RemoveMember_ExistingUser_RemovesFromChat()
        {
            // Arrange
            _testChat!.AddMember(_user1!);
            _testChat.AddMember(_user2!);

            // Act
            _testChat.RemoveMember(_user1!.PhoneNumber);

            // Assert
            Assert.That(_testChat.Members.Count, Is.EqualTo(1));
            Assert.That(_testChat.Members.ContainsKey(_user1.PhoneNumber), Is.False);
            Assert.That(_testChat.Members.ContainsKey(_user2!.PhoneNumber), Is.True);
        }

        [Test]
        public void RemoveMember_ReverseConnection_UserDoesNotKnowAboutChat()
        {
            // Arrange
            _testChat!.AddMember(_user1!);

            // Act
            _testChat.RemoveMember(_user1!.PhoneNumber);

            // Assert - Check reverse connection is removed
            Assert.That(_user1.JoinedChats.Count, Is.EqualTo(0));
            Assert.That(_user1.JoinedChats.Contains(_testChat), Is.False);
        }

        [Test]
        public void RemoveMember_NonExistentUser_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _testChat!.RemoveMember("+48999888777"));
        }

        [Test]
        public void GetMemberByPhoneNumber_ExistingUser_ReturnsUser()
        {
            // Arrange
            _testChat!.AddMember(_user1!);

            // Act
            User? foundUser = _testChat.GetMemberByPhoneNumber(_user1!.PhoneNumber);

            // Assert
            Assert.That(foundUser, Is.Not.Null);
            Assert.That(foundUser, Is.EqualTo(_user1));
        }

        [Test]
        public void GetMemberByPhoneNumber_NonExistentUser_ReturnsNull()
        {
            // Arrange
            _testChat!.AddMember(_user1!);

            // Act
            User? foundUser = _testChat.GetMemberByPhoneNumber("+48999888777");

            // Assert
            Assert.That(foundUser, Is.Null);
        }

        [Test]
        public void UpdateMemberPhoneNumber_ValidChange_UpdatesDictionary()
        {
            // Arrange
            _testChat!.AddMember(_user1!);
            string oldPhone = _user1!.PhoneNumber;
            string newPhone = "+48555666777";

            // Act
            _user1.PhoneNumber = newPhone; // This triggers update in chat

            // Assert
            Assert.That(_testChat.Members.ContainsKey(oldPhone), Is.False);
            Assert.That(_testChat.Members.ContainsKey(newPhone), Is.True);
            Assert.That(_testChat.Members[newPhone], Is.EqualTo(_user1));
        }

        [Test]
        public void JoinChat_FromUserSide_CreatesProperConnection()
        {
            // Act
            _user1!.JoinChat(_testChat!);

            // Assert
            Assert.That(_user1.JoinedChats.Contains(_testChat), Is.True);
            Assert.That(_testChat.Members.ContainsKey(_user1.PhoneNumber), Is.True);
        }

        [Test]
        public void LeaveChat_FromUserSide_RemovesConnection()
        {
            // Arrange
            _user1!.JoinChat(_testChat!);

            // Act
            _user1.LeaveChat(_testChat);

            // Assert
            Assert.That(_user1.JoinedChats.Contains(_testChat), Is.False);
            Assert.That(_testChat.Members.ContainsKey(_user1.PhoneNumber), Is.False);
        }
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

        // ========== BASIC ASSOCIATION TESTS (User ↔ Stickerpack) ==========

        [Test]
        public void SaveStickerpack_ValidPack_AddsToBothSides()
        {
            // Act
            _regularUser!.SaveStickerpack(_pack1!);

            // Assert
            Assert.That(_regularUser.SavedStickerpacks.Count, Is.EqualTo(1));
            Assert.That(_regularUser.SavedStickerpacks.Contains(_pack1), Is.True);
            // Check reverse connection
            Assert.That(_pack1!.SavedByUsers.Count, Is.EqualTo(1));
            Assert.That(_pack1.SavedByUsers.Contains(_regularUser), Is.True);
        }

        [Test]
        public void SaveStickerpack_RegularUser_EnforcesMaxLimit()
        {
            // Arrange - Save 10 packs (the limit)
            for (int i = 0; i < 10; i++)
            {
                var pack = new Stickerpack { Name = $"Pack {i}", IsPremium = false };
                _regularUser!.SaveStickerpack(pack);
            }

            // Act & Assert - 11th pack should fail
            var extraPack = new Stickerpack { Name = "Extra Pack", IsPremium = false };
            Assert.Throws<InvalidOperationException>(() => _regularUser!.SaveStickerpack(extraPack));
        }

        [Test]
        public void SaveStickerpack_PremiumUser_NoLimit()
        {
            // Arrange & Act - Save 15 packs (more than regular limit)
            for (int i = 0; i < 15; i++)
            {
                var pack = new Stickerpack { Name = $"Pack {i}", IsPremium = false };
                _premiumUser!.SaveStickerpack(pack);
            }

            // Assert - Should succeed without exception
            Assert.That(_premiumUser!.SavedStickerpacks.Count, Is.EqualTo(15));
        }

        [Test]
        public void SaveStickerpack_DuplicatePack_ThrowsException()
        {
            // Arrange
            _regularUser!.SaveStickerpack(_pack1!);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _regularUser.SaveStickerpack(_pack1!));
        }

        [Test]
        public void SaveStickerpack_NullPack_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _regularUser!.SaveStickerpack(null!));
        }

        [Test]
        public void UnsaveStickerpack_ExistingPack_RemovesFromBothSides()
        {
            // Arrange
            _regularUser!.SaveStickerpack(_pack1!);
            _regularUser.SaveStickerpack(_pack2!);

            // Act
            _regularUser.UnsaveStickerpack(_pack1!);

            // Assert
            Assert.That(_regularUser.SavedStickerpacks.Count, Is.EqualTo(1));
            Assert.That(_regularUser.SavedStickerpacks.Contains(_pack1), Is.False);
            // Check reverse connection
            Assert.That(_pack1!.SavedByUsers.Contains(_regularUser), Is.False);
        }

        [Test]
        public void UnsaveStickerpack_NonSavedPack_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _regularUser!.UnsaveStickerpack(_pack1!));
        }

        [Test]
        public void AddSavedByUser_FromPackSide_CreatesConnection()
        {
            // Act
            _pack1!.AddSavedByUser(_regularUser!);

            // Assert
            Assert.That(_pack1.SavedByUsers.Contains(_regularUser), Is.True);
            Assert.That(_regularUser!.SavedStickerpacks.Contains(_pack1), Is.True);
        }

        [Test]
        public void RemoveSavedByUser_FromPackSide_RemovesConnection()
        {
            // Arrange
            _pack1!.AddSavedByUser(_regularUser!);

            // Act
            _pack1.RemoveSavedByUser(_regularUser!);

            // Assert
            Assert.That(_pack1.SavedByUsers.Contains(_regularUser), Is.False);
            Assert.That(_regularUser!.SavedStickerpacks.Contains(_pack1), Is.False);
        }

        [Test]
        public void MultipleUsers_CanSaveSamePack()
        {
            // Act
            _regularUser!.SaveStickerpack(_pack1!);
            _premiumUser!.SaveStickerpack(_pack1!);

            // Assert
            Assert.That(_pack1!.SavedByUsers.Count, Is.EqualTo(2));
            Assert.That(_pack1.SavedByUsers.Contains(_regularUser), Is.True);
            Assert.That(_pack1.SavedByUsers.Contains(_premiumUser), Is.True);
        }

        [Test]
        public void OneUser_CanSaveMultiplePacks()
        {
            // Act
            _regularUser!.SaveStickerpack(_pack1!);
            _regularUser.SaveStickerpack(_pack2!);

            // Assert
            Assert.That(_regularUser.SavedStickerpacks.Count, Is.EqualTo(2));
            Assert.That(_regularUser.SavedStickerpacks.Contains(_pack1), Is.True);
            Assert.That(_regularUser.SavedStickerpacks.Contains(_pack2), Is.True);
        }

        [Test]
        public void RegularUser_CanSaveUpToExactLimit()
        {
            // Act - Save exactly 10 packs
            for (int i = 0; i < 10; i++)
            {
                var pack = new Stickerpack { Name = $"Pack {i}", IsPremium = false };
                _regularUser!.SaveStickerpack(pack);
            }

            // Assert
            Assert.That(_regularUser!.SavedStickerpacks.Count, Is.EqualTo(10));
        }

        [Test]
        public void UnsaveStickerpack_AllowsSavingAgain()
        {
            // Arrange - Fill to limit
            for (int i = 0; i < 10; i++)
            {
                var pack = new Stickerpack { Name = $"Pack {i}", IsPremium = false };
                _regularUser!.SaveStickerpack(pack);
            }

            // Act - Unsave one
            _regularUser!.UnsaveStickerpack(_regularUser.SavedStickerpacks.First());

            // Assert - Should be able to save a new one
            var newPack = new Stickerpack { Name = "New Pack", IsPremium = false };
            Assert.DoesNotThrow(() => _regularUser.SaveStickerpack(newPack));
            Assert.That(_regularUser.SavedStickerpacks.Count, Is.EqualTo(10));
        }
    }

    [TestFixture]
    public class IntegrationTests
    {
        [SetUp]
        public void SetUp()
        {
            Regular.ClearExtent();
            Premium.ClearExtent();
        }

        [Test]
        public void ComplexScenario_MultipleUsersAndChats()
        {
            // Arrange
            var chat1 = new Group { Name = "Friends" };
            var chat2 = new Group { Name = "Family" };
            var user1 = new Regular("Alice", "+48111222333", true, 5);
            var user2 = new Regular("Bob", "+48222333444", true, 3);
            var pack1 = new Stickerpack { Name = "Emojis", IsPremium = false };

            // Act
            chat1.AddMember(user1);
            chat1.AddMember(user2);
            chat2.AddMember(user1);

            user1.SaveStickerpack(pack1);
            user2.SaveStickerpack(pack1);

            // Assert
            Assert.That(chat1.Members.Count, Is.EqualTo(2));
            Assert.That(chat2.Members.Count, Is.EqualTo(1));
            Assert.That(user1.JoinedChats.Count, Is.EqualTo(2));
            Assert.That(user2.JoinedChats.Count, Is.EqualTo(1));
            Assert.That(pack1.SavedByUsers.Count, Is.EqualTo(2));
        }
    }
}