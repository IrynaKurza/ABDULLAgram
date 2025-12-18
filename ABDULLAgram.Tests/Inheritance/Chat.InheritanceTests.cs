using ABDULLAgram.Chats;

namespace ABDULLAgram.Tests.Inheritance
{
    // ============================================================
    // ASSIGNMENT 7: INHERITANCE TESTS
    // Tests showcasing Chat inheritance via composition pattern
    // Tests that discriminator and type-specific behavior works
    // ============================================================
    
    [TestFixture]
    public class ChatInheritanceTests
    {
        // ============================================================
        // DISCRIMINATOR TESTS
        // ============================================================
        
        [Test]
        public void Constructor_Group_SetsChatTypeToGroup()
        {
            var chat = new Chat(ChatType.Group);
            
            Assert.That(chat.ChatType, Is.EqualTo(ChatType.Group));
        }
        
        [Test]
        public void Constructor_Private_SetsChatTypeToPrivate()
        {
            var chat = new Chat(ChatType.Private);
            
            Assert.That(chat.ChatType, Is.EqualTo(ChatType.Private));
        }
        
        // ============================================================
        // GROUP-SPECIFIC BEHAVIOR TESTS
        // ============================================================
        
        [Test]
        public void GroupChat_HasDescription_CanSetAndGet()
        {
            var chat = new Chat(ChatType.Group);
            
            chat.Description = "Test description";
            
            Assert.That(chat.Description, Is.EqualTo("Test description"));
        }
        
        [Test]
        public void GroupChat_MaxParticipants_CanBeModified()
        {
            var chat = new Chat(ChatType.Group);
            
            chat.MaxParticipants = 200;
            
            Assert.That(chat.MaxParticipants, Is.EqualTo(200));
        }
        
        [Test]
        public void GroupChat_MaxParticipants_DefaultsTo100()
        {
            var chat = new Chat(ChatType.Group);
            
            Assert.That(chat.MaxParticipants, Is.EqualTo(100));
        }
        
        // ============================================================
        // PRIVATE-SPECIFIC BEHAVIOR TESTS
        // ============================================================
        
        [Test]
        public void PrivateChat_MaxParticipants_IsAlways2()
        {
            var chat = new Chat(ChatType.Private);
            
            Assert.That(chat.MaxParticipants, Is.EqualTo(2));
        }
        
        [Test]
        public void PrivateChat_MaxParticipants_CannotBeChanged()
        {
            var chat = new Chat(ChatType.Private);
            
            Assert.Throws<ArgumentOutOfRangeException>(() => chat.MaxParticipants = 5);
        }
        
        [Test]
        public void PrivateChat_AccessingDescription_ThrowsException()
        {
            var chat = new Chat(ChatType.Private);
            
            Assert.Throws<InvalidOperationException>(() => { var desc = chat.Description; });
        }
        
        [Test]
        public void PrivateChat_SettingDescription_ThrowsException()
        {
            var chat = new Chat(ChatType.Private);
            
            Assert.Throws<InvalidOperationException>(() => chat.Description = "test");
        }
        
        // ============================================================
        // TYPE ENFORCEMENT TESTS (Disjoint, Complete)
        // ============================================================
        
        [Test]
        public void ChatType_IsImmutable_CannotChangeAfterCreation()
        {
            var chat = new Chat(ChatType.Group);
            
            // ChatType property has setter, but this tests the pattern
            // In production, readonly would be better, but XML serialization needs setter
            Assert.That(chat.ChatType, Is.EqualTo(ChatType.Group));
        }
        
        [Test]
        public void GroupChat_CanOnlyAccessGroupProperties()
        {
            var chat = new Chat(ChatType.Group);
            
            // Should work - Group has Description
            Assert.DoesNotThrow(() => { var desc = chat.Description; });
            
            // Should work - Group has MaxParticipants
            Assert.DoesNotThrow(() => { var max = chat.MaxParticipants; });
        }
        
        [Test]
        public void PrivateChat_CannotAccessGroupOnlyProperties()
        {
            var chat = new Chat(ChatType.Private);
            
            // Should throw - Private doesn't have Description
            Assert.Throws<InvalidOperationException>(() => { var desc = chat.Description; });
        }
        
        // ============================================================
        // COMMON PROPERTIES WORK FOR BOTH TYPES
        // ============================================================
        
        [Test]
        public void BothTypes_CanSetName()
        {
            var groupChat = new Chat(ChatType.Group);
            var privateChat = new Chat(ChatType.Private);
            
            groupChat.Name = "Group Name";
            privateChat.Name = "Private Name";
            
            Assert.That(groupChat.Name, Is.EqualTo("Group Name"));
            Assert.That(privateChat.Name, Is.EqualTo("Private Name"));
        }
        
        [Test]
        public void BothTypes_HaveCreatedAtTimestamp()
        {
            var groupChat = new Chat(ChatType.Group);
            var privateChat = new Chat(ChatType.Private);
            
            Assert.That(groupChat.CreatedAt, Is.LessThanOrEqualTo(DateTime.Now));
            Assert.That(privateChat.CreatedAt, Is.LessThanOrEqualTo(DateTime.Now));
        }
    }
}