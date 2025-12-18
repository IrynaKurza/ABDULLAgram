using ABDULLAgram.Attachments;
using ABDULLAgram.Chats;
using ABDULLAgram.Messages;
using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Inheritance
{
    /// <summary>
    /// Tests for Message State Inheritance via Composition (Assignment 7).
    /// Message uses composition pattern with inner classes (DraftData, SentData)
    /// and a discriminator enum (MessageStateType) instead of class hierarchy.
    /// </summary>
    [TestFixture]
    public class MessageStateInheritanceTests
    {
        // ============================================================
        // TEST HELPERS
        // ============================================================
        
        private User _testUser = null!;
        private Chat _testChat = null!;
        private int _stickerCounter = 0;

        /// <summary>
        /// Creates a message initialized as Draft state.
        /// Uses Sticker as concrete Message subclass.
        /// </summary>
        private Sticker CreateDraftMessage()
        {
            var emojiCode = "🎨" + _stickerCounter;
            _stickerCounter++;
            var sticker = new Sticker(_testUser, _testChat, emojiCode, Sticker.BackgroundTypeEnum.Transparent);
            sticker.InitializeAsDraft();
            return sticker;
        }

        /// <summary>
        /// Creates a message initialized as Sent state.
        /// </summary>
        private Sticker CreateSentMessage()
        {
            var emojiCode = "📤" + _stickerCounter;
            _stickerCounter++;
            var sticker = new Sticker(_testUser, _testChat, emojiCode, Sticker.BackgroundTypeEnum.Transparent);
            var now = DateTime.Now;
            sticker.InitializeAsSent(now, now);
            return sticker;
        }

        [SetUp]
        public void Setup()
        {
            Regular.ClearExtent();
            Sticker.ClearExtent();
            _stickerCounter = 0;
            
            _testUser = new Regular("TestUser", "+1234567890", true, 1);
            _testChat = new Group { Name = "Test Group" };
            _testChat.AddMember(_testUser);
        }

        [TearDown]
        public void Cleanup()
        {
            Regular.ClearExtent();
            Sticker.ClearExtent();
        }

        // ============================================================
        // TEST 1: Message is created in Draft state by default
        // ============================================================
        
        [Test]
        public void Message_InitializedAsDraft_HasDraftState()
        {
            var message = CreateDraftMessage();

            Assert.That(message.StateType, Is.EqualTo(MessageStateType.Draft));
            Assert.That(message.IsDraft, Is.True);
            Assert.That(message.IsSent, Is.False);
        }

        // ============================================================
        // TEST 2: Draft → Sent transition is allowed via Send()
        // ============================================================
        
        [Test]
        public void Send_FromDraftState_TransitionsToSent()
        {
            var message = CreateDraftMessage();
            Assert.That(message.StateType, Is.EqualTo(MessageStateType.Draft));

            message.Send();

            Assert.That(message.StateType, Is.EqualTo(MessageStateType.Sent));
            Assert.That(message.IsSent, Is.True);
            Assert.That(message.IsDraft, Is.False);
        }

        // ============================================================
        // TEST 3: Sent → Draft transition throws exception (ONE-WAY CONSTRAINT)
        // ============================================================
        
        [Test]
        public void ConvertToDraft_FromSentState_ThrowsException()
        {
            var message = CreateSentMessage();
            Assert.That(message.StateType, Is.EqualTo(MessageStateType.Sent));

            Assert.Throws<InvalidOperationException>(() => message.ConvertToDraft());
        }

        [Test]
        public void Send_WhenAlreadySent_ThrowsException()
        {
            var message = CreateSentMessage();

            Assert.Throws<InvalidOperationException>(() => message.Send());
        }

        // ============================================================
        // TEST 4: SaveDraft updates LastSaveTimestamp
        // ============================================================
        
        [Test]
        public void SaveDraft_UpdatesLastSaveTimestamp()
        {
            var message = CreateDraftMessage();
            var initialTimestamp = message.LastSaveTimestamp;
            
            // Wait a tiny bit to ensure timestamp changes
            Thread.Sleep(10);
            
            message.SaveDraft();

            Assert.That(message.LastSaveTimestamp, Is.GreaterThan(initialTimestamp));
        }

        [Test]
        public void SaveDraft_WhenSent_ThrowsException()
        {
            var message = CreateSentMessage();

            Assert.Throws<InvalidOperationException>(() => message.SaveDraft());
        }

        // ============================================================
        // TEST 5: Send sets SendTimestamp
        // ============================================================
        
        [Test]
        public void Send_SetsSendTimestamp()
        {
            var message = CreateDraftMessage();
            var beforeSend = DateTime.Now;
            
            message.Send();
            
            var afterSend = DateTime.Now;

            Assert.That(message.SendTimestamp, Is.GreaterThanOrEqualTo(beforeSend));
            Assert.That(message.SendTimestamp, Is.LessThanOrEqualTo(afterSend));
        }

        [Test]
        public void SendTimestamp_WhenDraft_ThrowsException()
        {
            var message = CreateDraftMessage();

            Assert.Throws<InvalidOperationException>(() => 
            {
                var _ = message.SendTimestamp;
            });
        }

        // ============================================================
        // TEST 6: EditMessage updates EditedAt only when state is Sent
        // ============================================================
        
        [Test]
        public void EditMessage_WhenSent_UpdatesEditedAt()
        {
            var message = CreateSentMessage();
            Assert.That(message.EditedAt, Is.Null);

            message.EditMessage();

            Assert.That(message.EditedAt, Is.Not.Null);
            Assert.That(message.EditedAt, Is.LessThanOrEqualTo(DateTime.Now));
        }

        [Test]
        public void EditMessage_WhenDraft_UpdatesLastSaveTimestamp()
        {
            var message = CreateDraftMessage();
            var initialTimestamp = message.LastSaveTimestamp;
            
            Thread.Sleep(10);
            
            message.EditMessage();

            Assert.That(message.LastSaveTimestamp, Is.GreaterThan(initialTimestamp));
        }

        // ============================================================
        // TEST 7: MarkAsDeletedState sets DeletedAt only when state is Sent
        // ============================================================
        
        [Test]
        public void MarkAsDeletedState_WhenSent_SetsDeletedAt()
        {
            var message = CreateSentMessage();
            Assert.That(message.DeletedAt, Is.Null);

            message.MarkAsDeletedState();

            Assert.That(message.DeletedAt, Is.Not.Null);
            Assert.That(message.DeletedAt, Is.LessThanOrEqualTo(DateTime.Now));
        }

        [Test]
        public void MarkAsDeletedState_WhenDraft_ThrowsException()
        {
            var message = CreateDraftMessage();

            Assert.Throws<InvalidOperationException>(() => message.MarkAsDeletedState());
        }

        [Test]
        public void MarkAsDeletedState_WhenAlreadyDeleted_ThrowsException()
        {
            var message = CreateSentMessage();
            message.MarkAsDeletedState();

            Assert.Throws<InvalidOperationException>(() => message.MarkAsDeletedState());
        }

        // ============================================================
        // TEST 8: ReadByUsers is allowed only in Sent state
        // ============================================================
        
        [Test]
        public void MarkAsRead_WhenSent_AddsUserToReadByUsers()
        {
            var message = CreateSentMessage();
            var reader = new Regular("Reader", "+9876543210", true, 1);
            _testChat.AddMember(reader);

            message.MarkAsRead(reader);

            Assert.That(message.ReadByUsers, Contains.Item(reader));
        }

        [Test]
        public void MarkAsRead_WhenSent_IsIdempotent()
        {
            var message = CreateSentMessage();
            var reader = new Regular("Reader", "+9876543210", true, 1);
            _testChat.AddMember(reader);

            message.MarkAsRead(reader);
            message.MarkAsRead(reader); // Second call should not throw

            Assert.That(message.ReadByUsers.Count, Is.EqualTo(1));
        }

        // ============================================================
        // TEST 9: ReadByUsers in Draft state throws exception
        // ============================================================
        
        [Test]
        public void ReadByUsers_WhenDraft_ThrowsException()
        {
            var message = CreateDraftMessage();

            Assert.Throws<InvalidOperationException>(() => 
            {
                var _ = message.ReadByUsers;
            });
        }

        [Test]
        public void MarkAsRead_WhenDraft_ThrowsException()
        {
            var message = CreateDraftMessage();
            var reader = new Regular("Reader", "+9876543210", true, 1);

            Assert.Throws<InvalidOperationException>(() => message.MarkAsRead(reader));
        }

        // ============================================================
        // ADDITIONAL TESTS: State-specific property access
        // ============================================================

        [Test]
        public void LastSaveTimestamp_WhenSent_ThrowsException()
        {
            var message = CreateSentMessage();

            Assert.Throws<InvalidOperationException>(() => 
            {
                var _ = message.LastSaveTimestamp;
            });
        }

        [Test]
        public void DeliveredAt_WhenDraft_ThrowsException()
        {
            var message = CreateDraftMessage();

            Assert.Throws<InvalidOperationException>(() => 
            {
                var _ = message.DeliveredAt;
            });
        }

        [Test]
        public void EditedAt_WhenDraft_ThrowsException()
        {
            var message = CreateDraftMessage();

            Assert.Throws<InvalidOperationException>(() => 
            {
                var _ = message.EditedAt;
            });
        }

        [Test]
        public void DeletedAt_WhenDraft_ThrowsException()
        {
            var message = CreateDraftMessage();

            Assert.Throws<InvalidOperationException>(() => 
            {
                var _ = message.DeletedAt;
            });
        }

        // ============================================================
        // DYNAMIC CONSTRAINT TEST: Full lifecycle
        // ============================================================

        [Test]
        public void MessageLifecycle_DraftToSent_WorksCorrectly()
        {
            // Create as Draft
            var message = CreateDraftMessage();
            Assert.That(message.StateType, Is.EqualTo(MessageStateType.Draft));
            
            // Edit while Draft (updates LastSaveTimestamp)
            message.EditMessage();
            Assert.That(message.LastSaveTimestamp, Is.LessThanOrEqualTo(DateTime.Now));
            
            // Save Draft
            message.SaveDraft();
            
            // Transition to Sent
            message.Send();
            Assert.That(message.StateType, Is.EqualTo(MessageStateType.Sent));
            Assert.That(message.SendTimestamp, Is.LessThanOrEqualTo(DateTime.Now));
            
            // Edit while Sent (updates EditedAt)
            message.EditMessage();
            Assert.That(message.EditedAt, Is.Not.Null);
            
            // Mark as read
            var reader = new Regular("Reader", "+5555555555", true, 1);
            _testChat.AddMember(reader);
            message.MarkAsRead(reader);
            Assert.That(message.ReadByUsers, Contains.Item(reader));
            
            // Cannot go back to Draft!
            Assert.Throws<InvalidOperationException>(() => message.ConvertToDraft());
        }
    }
}

