using ABDULLAgram.Chats;
using ABDULLAgram.Support;
using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Associations.Qualified
{
    // ============================================================
    // QUALIFIED ASSOCIATION TESTS: Chat ↔ User (by phoneNumber)
    // Tests the Dictionary-based qualified association
    // Key feature: O(1) lookup by phone number
    // ============================================================
    
    [TestFixture]
    public class QualifiedAssociationTests
    {
        private Regular? _user1;
        private Regular? _user2;
        private Group? _testChat;

        [SetUp]
        public void SetUp()
        {
            Regular.ClearExtent();
            Premium.ClearExtent();
            _user1 = new Regular("Alice", "+48111222333", true, 5);
            _user2 = new Regular("Bob", "+48222333444", true, 3);
            _testChat = new Group { Name = "Test Group", Description = "Test Description" };
        }

        // TEST: Adding member from Chat side creates bidirectional link
        [Test]
        public void AddMember_ValidUser_AddsToChat()
        {
            // Act
            _testChat!.AddMember(_user1!);

            // Assert - Check both sides of association
            Assert.That(_testChat.Members.ContainsKey(_user1!.PhoneNumber), Is.True);
            Assert.That(_testChat.Members[_user1.PhoneNumber], Is.EqualTo(_user1));
        }

        // TEST: Reverse connection - User knows about chat
        [Test]
        public void AddMember_ReverseConnection_UserKnowsAboutChat()
        {
            // Act
            _testChat!.AddMember(_user1!);

            // Assert - User's collection updated
            Assert.That(_user1!.JoinedChats.Contains(_testChat), Is.True);
        }

        // TEST: Dictionary prevents duplicate phone numbers (qualifier uniqueness)
        [Test]
        public void AddMember_DuplicatePhoneNumber_ThrowsException()
        {
            // Arrange
            _testChat!.AddMember(_user1!);

            // Act & Assert - Can't add user with same phone number
            Assert.Throws<InvalidOperationException>(() => _testChat.AddMember(_user1));
        }

        // TEST: Null validation
        [Test]
        public void AddMember_NullUser_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _testChat!.AddMember(null!));
        }

        // TEST: Removing member updates both sides
        [Test]
        public void RemoveMember_ExistingUser_RemovesFromChat()
        {
            // Arrange
            _testChat!.AddMember(_user1!);

            // Act
            _testChat.RemoveMember(_user1!.PhoneNumber);

            // Assert
            Assert.That(_testChat.Members.ContainsKey(_user1.PhoneNumber), Is.False);
        }

        // TEST: Reverse connection on removal
        [Test]
        public void RemoveMember_ReverseConnection_UserDoesNotKnowAboutChat()
        {
            // Arrange
            _testChat!.AddMember(_user1!);

            // Act
            _testChat.RemoveMember(_user1!.PhoneNumber);

            // Assert
            Assert.That(_user1.JoinedChats.Contains(_testChat), Is.False);
        }

        // TEST: Can't remove non-existent user
        [Test]
        public void RemoveMember_NonExistentUser_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _testChat!.RemoveMember("+48999999999"));
        }

        // TEST: QUALIFIED LOOKUP - Key feature of qualified associations!
        // lookup by phone number instead of iterating through collection
        [Test]
        public void GetMemberByPhoneNumber_ExistingUser_ReturnsUser()
        {
            // Arrange
            _testChat!.AddMember(_user1!);

            // Act
            var foundUser = _testChat.GetMemberByPhoneNumber(_user1!.PhoneNumber);

            // Assert - Fast lookup by qualifier
            Assert.That(foundUser, Is.EqualTo(_user1));
        }

        // TEST: Lookup returns null for non-existent key
        [Test]
        public void GetMemberByPhoneNumber_NonExistentUser_ReturnsNull()
        {
            // Act
            var foundUser = _testChat!.GetMemberByPhoneNumber("+48999999999");

            // Assert
            Assert.That(foundUser, Is.Null);
        }

        // TEST: CRITICAL for qualified associations - updating the qualifier (key)
        // When phone number changes, Dictionary key must be updated
        [Test]
        public void UpdateMemberPhoneNumber_ValidChange_UpdatesDictionary()
        {
            // Arrange
            _testChat!.AddMember(_user1!);
            string oldPhone = _user1.PhoneNumber;
            string newPhone = "+48555666777";

            // Act
            _testChat.UpdateMemberPhoneNumber(oldPhone, newPhone);

            // Assert - Old key removed, new key added
            Assert.That(_testChat.Members.ContainsKey(oldPhone), Is.False);
            Assert.That(_testChat.Members.ContainsKey(newPhone), Is.True);
            Assert.That(_testChat.Members[newPhone], Is.EqualTo(_user1));
        }

        // TEST: Adding from User side (reverse direction)
        [Test]
        public void JoinChat_FromUserSide_CreatesProperConnection()
        {
            // Act
            _user1!.JoinChat(_testChat!);

            // Assert - Both sides updated
            Assert.That(_user1.JoinedChats.Contains(_testChat), Is.True);
            Assert.That(_testChat.Members.ContainsKey(_user1.PhoneNumber), Is.True);
        }

        // TEST: Leaving from User side
        [Test]
        public void LeaveChat_FromUserSide_RemovesConnection()
        {
            // Arrange
            _user1!.JoinChat(_testChat!);

            // Act
            _user1.LeaveChat(_testChat);

            // Assert - Both sides updated
            Assert.That(_user1.JoinedChats.Contains(_testChat), Is.False);
            Assert.That(_testChat.Members.ContainsKey(_user1.PhoneNumber), Is.False);
        }
    }

    // ============================================================
    // BASIC ASSOCIATION TESTS: User ↔ Stickerpack (many-to-many)
    // Tests polymorphic business rules (Regular max 10, Premium unlimited)
    // ============================================================
    
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
            _pack1 = new Stickerpack("Funny Pack", _regularUser) { IsPremium = false };
            _pack2 = new Stickerpack("Premium Pack", _premiumUser) { IsPremium = true };

        }

        // TEST: Saving from User side creates bidirectional link
        [Test]
        public void SaveStickerpack_ValidPack_AddsToBothSides()
        {
            // Act
            _regularUser!.SaveStickerpack(_pack1!);

            // Assert - Both sides of association updated
            Assert.That(_regularUser.SavedStickerpacks.Contains(_pack1), Is.True);
            Assert.That(_pack1!.SavedByUsers.Contains(_regularUser), Is.True);
        }

        // TEST: BUSINESS RULE - Regular users limited to 10 packs
        [Test]
        public void SaveStickerpack_RegularUser_EnforcesMaxLimit()
        {
            // Arrange - Fill to limit (10 packs)
            for (int i = 0; i < 10; i++)
            {
                var pack = new Stickerpack($"Pack {i}", _regularUser) { IsPremium = false };
                _regularUser!.SaveStickerpack(pack);

            }

            // Act & Assert - 11th pack should fail
            var extraPack = new Stickerpack ( "Extra Pack", _regularUser) { IsPremium = false };
            Assert.Throws<InvalidOperationException>(() => _regularUser!.SaveStickerpack(extraPack));
        }

        // TEST: POLYMORPHISM - Premium users have no limit
        [Test]
        public void SaveStickerpack_PremiumUser_NoLimit()
        {
            // Act - Save way more than 10 packs
            for (int i = 0; i < 50; i++)
            {
                var pack = new Stickerpack ($"Pack {i}", _premiumUser) {IsPremium = false };
                _premiumUser!.SaveStickerpack(pack);
            }

            // Assert - No exception, all saved
            Assert.That(_premiumUser!.SavedStickerpacks.Count, Is.EqualTo(50));
        }

        // TEST: Can't save same pack twice
        [Test]
        public void SaveStickerpack_DuplicatePack_ThrowsException()
        {
            // Arrange
            _regularUser!.SaveStickerpack(_pack1!);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _regularUser.SaveStickerpack(_pack1));
        }

        // TEST: Null validation
        [Test]
        public void SaveStickerpack_NullPack_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _regularUser!.SaveStickerpack(null!));
        }

        // TEST: Unsaving updates both sides
        [Test]
        public void UnsaveStickerpack_ExistingPack_RemovesFromBothSides()
        {
            // Arrange
            _regularUser!.SaveStickerpack(_pack1!);

            // Act
            _regularUser.UnsaveStickerpack(_pack1);

            // Assert - Both sides updated
            Assert.That(_regularUser.SavedStickerpacks.Contains(_pack1), Is.False);
            Assert.That(_pack1!.SavedByUsers.Contains(_regularUser), Is.False);
        }

        // TEST: Can't unsave pack that wasn't saved
        [Test]
        public void UnsaveStickerpack_NonSavedPack_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _regularUser!.UnsaveStickerpack(_pack1!));
        }

        // TEST: Adding from Stickerpack side (reverse direction)
        [Test]
        public void AddSavedByUser_FromPackSide_CreatesConnection()
        {
            // Act
            _pack1!.AddSavedByUser(_regularUser!);

            // Assert - Both sides updated
            Assert.That(_pack1.SavedByUsers.Contains(_regularUser), Is.True);
            Assert.That(_regularUser!.SavedStickerpacks.Contains(_pack1), Is.True);
        }

        // TEST: Removing from Stickerpack side
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

        // TEST: Many-to-many - multiple users can save same pack
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

        // TEST: Many-to-many - one user can save multiple packs
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

        // TEST: Can save exactly up to limit
        [Test]
        public void RegularUser_CanSaveUpToExactLimit()
        {
            // Act - Save exactly 10 packs
            for (int i = 0; i < 10; i++)
            {
                var pack = new Stickerpack ( $"Pack {i}", _regularUser) { IsPremium = false };
                _regularUser!.SaveStickerpack(pack);
            }

            // Assert
            Assert.That(_regularUser!.SavedStickerpacks.Count, Is.EqualTo(10));
        }

        // TEST: Unsaving frees up space for new packs
        [Test]
        public void UnsaveStickerpack_AllowsSavingAgain()
        {
            // Arrange - Fill to limit
            for (int i = 0; i < 10; i++)
            {
                var pack = new Stickerpack ( $"Pack {i}", _regularUser) { IsPremium = false };
                _regularUser!.SaveStickerpack(pack);
            }

            // Act - Unsave one
            _regularUser!.UnsaveStickerpack(_regularUser.SavedStickerpacks.First());

            // Assert - Should be able to save a new one
            var newPack = new Stickerpack ("New Pack", _regularUser) { IsPremium = false };
            Assert.DoesNotThrow(() => _regularUser.SaveStickerpack(newPack));
            Assert.That(_regularUser.SavedStickerpacks.Count, Is.EqualTo(10));
        }
    }

}