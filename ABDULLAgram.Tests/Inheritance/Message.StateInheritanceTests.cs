using ABDULLAgram.Attachments;
using ABDULLAgram.Chats;
using ABDULLAgram.Messages;
using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Inheritance
{
    [TestFixture]
    public class MessageStateInheritanceTests
    {
        private User _testUser = null!;
        private Chat _testChat = null!;
        private int _stickerCounter = 0;

        private Sticker CreateDraftMessage()
        {
            var emojiCode = "🎨" + _stickerCounter;
            _stickerCounter++;
            var sticker = new Sticker(_testUser, _testChat, emojiCode, Sticker.BackgroundTypeEnum.Transparent);
            sticker.InitializeAsDraft();
            return sticker;
        }

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
            User.ClearExtent();
            Sticker.ClearExtent();
            _stickerCounter = 0;
            
            _testUser = new User("TestUser", "+1234567890", true);
            _testUser.InitializeAsRegular(1);
            _testChat = new Chat(ChatType.Group) { Name = "Test Group" };
            _testChat.AddMember(_testUser);
        }

        [TearDown]
        public void Cleanup()
        {
            User.ClearExtent();
            Sticker.ClearExtent();
        }

        [Test]
        public void Message_InitializedAsDraft_HasDraftState()
        {
            var message = CreateDraftMessage();
            Assert.That(message.StateType, Is.EqualTo(MessageStateType.Draft));
            Assert.That(message.IsDraft, Is.True);
            Assert.That(message.IsSent, Is.False);
        }

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

        [Test]
        public void SaveDraft_UpdatesLastSaveTimestamp()
        {
            var message = CreateDraftMessage();
            var initialTimestamp = message.LastSaveTimestamp;
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
            Assert.Throws<InvalidOperationException>(() => { var _ = message.SendTimestamp; });
        }

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

        [Test]
        public void MarkAsRead_WhenSent_AddsUserToReadByUsers()
        {
            var message = CreateSentMessage();
            var reader = new User("Reader", "+9876543210", true);
            reader.InitializeAsRegular(1);
            _testChat.AddMember(reader);
            message.MarkAsRead(reader);
            Assert.That(message.ReadByUsers, Contains.Item(reader));
        }

        [Test]
        public void MarkAsRead_WhenSent_IsIdempotent()
        {
            var message = CreateSentMessage();
            var reader = new User("Reader", "+9876543210", true);
            reader.InitializeAsRegular(1);
            _testChat.AddMember(reader);
            message.MarkAsRead(reader);
            message.MarkAsRead(reader);
            Assert.That(message.ReadByUsers.Count, Is.EqualTo(1));
        }

        [Test]
        public void ReadByUsers_WhenDraft_ThrowsException()
        {
            var message = CreateDraftMessage();
            Assert.Throws<InvalidOperationException>(() => { var _ = message.ReadByUsers; });
        }

        [Test]
        public void MarkAsRead_WhenDraft_ThrowsException()
        {
            var message = CreateDraftMessage();
            var reader = new User("Reader", "+9876543210", true);
            reader.InitializeAsRegular(1);
            Assert.Throws<InvalidOperationException>(() => message.MarkAsRead(reader));
        }

        [Test]
        public void LastSaveTimestamp_WhenSent_ThrowsException()
        {
            var message = CreateSentMessage();
            Assert.Throws<InvalidOperationException>(() => { var _ = message.LastSaveTimestamp; });
        }

        [Test]
        public void DeliveredAt_WhenDraft_ThrowsException()
        {
            var message = CreateDraftMessage();
            Assert.Throws<InvalidOperationException>(() => { var _ = message.DeliveredAt; });
        }

        [Test]
        public void EditedAt_WhenDraft_ThrowsException()
        {
            var message = CreateDraftMessage();
            Assert.Throws<InvalidOperationException>(() => { var _ = message.EditedAt; });
        }

        [Test]
        public void DeletedAt_WhenDraft_ThrowsException()
        {
            var message = CreateDraftMessage();
            Assert.Throws<InvalidOperationException>(() => { var _ = message.DeletedAt; });
        }

        [Test]
        public void MessageLifecycle_DraftToSent_WorksCorrectly()
        {
            var message = CreateDraftMessage();
            Assert.That(message.StateType, Is.EqualTo(MessageStateType.Draft));
            message.EditMessage();
            Assert.That(message.LastSaveTimestamp, Is.LessThanOrEqualTo(DateTime.Now));
            message.SaveDraft();
            message.Send();
            Assert.That(message.StateType, Is.EqualTo(MessageStateType.Sent));
            Assert.That(message.SendTimestamp, Is.LessThanOrEqualTo(DateTime.Now));
            message.EditMessage();
            Assert.That(message.EditedAt, Is.Not.Null);
            var reader = new User("Reader", "+5555555555", true);
            reader.InitializeAsRegular(1);
            _testChat.AddMember(reader);
            message.MarkAsRead(reader);
            Assert.That(message.ReadByUsers, Contains.Item(reader));
            Assert.Throws<InvalidOperationException>(() => message.ConvertToDraft());
        }
    }
}