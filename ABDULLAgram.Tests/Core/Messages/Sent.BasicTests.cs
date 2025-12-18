using System.Text.RegularExpressions;
using ABDULLAgram.Attachments;
using ABDULLAgram.Chats;
using ABDULLAgram.Messages;
using ABDULLAgram.Users;
using File = System.IO.File;

namespace ABDULLAgram.Tests.Core.Messages
{
    [TestFixture]
    public class SentTests
    {
        private User _user;
        private Chat _chat;

        [SetUp]
        public void Setup()
        {
            Text.ClearExtent();
            User.ClearExtent();

            _user = new User("sender", "+123456789", true);
            _user.InitializeAsRegular(1);
            _chat = new Chat(ChatType.Group) { Name = "Test Group" };

            _chat.AddMember(_user);
        }

        [Test]
        public void Constructor_AddsToExtent()
        {
            var sent = new Text(_user, _chat, "test message", false);
            sent.InitializeAsSent(DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(-5), null, null);

            Assert.That(Text.GetAll().Count, Is.EqualTo(1));
            Assert.That(Text.GetAll().First().SendTimestamp, Is.LessThanOrEqualTo(DateTime.Now));
        }

        [Test]
        public void GetAll_IsReadOnly()
        {
            var sent = new Text(_user, _chat, "test message", false);
            sent.InitializeAsSent(DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(-5), null, null);
            var view = Text.GetAll();

            Assert.Throws<NotSupportedException>(() =>
                ((ICollection<Text>)view).Add(sent));
        }
    }

    [TestFixture]
    public class SentValidationTests
    {
        private User _user;
        private Chat _chat;

        [SetUp]
        public void Setup()
        {
            Text.ClearExtent();
            User.ClearExtent();
            _user = new User("sender", "+987654321", true);
            _user.InitializeAsRegular(1);
            _chat = new Chat(ChatType.Group) { Name = "Validation Group" };

            _chat.AddMember(_user);
        }

        [Test]
        public void Duplicate_Id_Throws()
        {
            var s1 = new Text(_user, _chat, "test1", false);
            s1.InitializeAsSent(DateTime.Now, DateTime.Now, null, null);
            var s2 = new Text(_user, _chat, "test2", false);
            s2.InitializeAsSent(DateTime.Now, DateTime.Now, null, null);

            Assert.Throws<InvalidOperationException>(() =>
                s2.Id = s1.Id);
        }

        [Test]
        public void InitializeAsSent_FutureSendTime_Throws()
        {
            var sent = new Text(_user, _chat, "test", false);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sent.InitializeAsSent(DateTime.Now.AddDays(1), DateTime.Now, null, null));
        }

        [Test]
        public void Set_DeliveredAt_Future_Throws()
        {
            var sent = new Text(_user, _chat, "test", false);
            sent.InitializeAsSent(DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(-5), null, null);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sent.DeliveredAt = DateTime.Now.AddDays(1));
        }

        [Test]
        public void Set_DeliveredAt_BeforeSendTimestamp_Throws()
        {
            var sendTime = DateTime.Now.AddMinutes(-10);
            var sent = new Text(_user, _chat, "test", false);
            sent.InitializeAsSent(sendTime, DateTime.Now.AddMinutes(-5), null, null);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sent.DeliveredAt = sendTime.AddMinutes(-5));
        }

        [Test]
        public void Set_EditedAt_Future_Throws()
        {
            var sent = new Text(_user, _chat, "test", false);
            sent.InitializeAsSent(DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(-5), null, null);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sent.EditedAt = DateTime.Now.AddDays(1));
        }

        [Test]
        public void Set_EditedAt_BeforeSendTimestamp_Throws()
        {
            var sendTime = DateTime.Now.AddMinutes(-10);
            var sent = new Text(_user, _chat, "test", false);
            sent.InitializeAsSent(sendTime, DateTime.Now.AddMinutes(-5), null, null);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sent.EditedAt = sendTime.AddMinutes(-5));
        }

        [Test]
        public void Set_EditedAt_Null_Succeeds()
        {
            var sent = new Text(_user, _chat, "test", false);
            sent.InitializeAsSent(DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(-5),
                DateTime.Now.AddMinutes(-3), null);

            sent.EditedAt = null;
            Assert.That(sent.EditedAt, Is.Null);
        }

        [Test]
        public void Set_DeletedAt_Future_Throws()
        {
            var sent = new Text(_user, _chat, "test", false);
            sent.InitializeAsSent(DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(-5), null, null);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sent.DeletedAt = DateTime.Now.AddDays(1));
        }

        [Test]
        public void Set_DeletedAt_BeforeSendTimestamp_Throws()
        {
            var sendTime = DateTime.Now.AddMinutes(-10);
            var sent = new Text(_user, _chat, "test", false);
            sent.InitializeAsSent(sendTime, DateTime.Now.AddMinutes(-5), null, null);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sent.DeletedAt = sendTime.AddMinutes(-5));
        }

        [Test]
        public void Set_DeletedAt_Null_Succeeds()
        {
            var sent = new Text(_user, _chat, "test", false);
            sent.InitializeAsSent(DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(-5), null,
                DateTime.Now.AddMinutes(-2));

            sent.DeletedAt = null;
            Assert.That(sent.DeletedAt, Is.Null);
        }
    }

    [TestFixture]
    public class SentPersistenceTests
    {
        private string TestPath => Path.Combine(TestContext.CurrentContext.WorkDirectory, "test_sent.xml");

        [SetUp]
        public void Setup()
        {
            Text.ClearExtent();
            User.ClearExtent();
            Persistence.DeleteAll(TestPath);
        }

        [TearDown]
        public void Cleanup() => Persistence.DeleteAll(TestPath);

        [Test]
        public void Load_WhenFileMissing_ReturnsFalseAndClearsExtent()
        {
            var u = new User("temp", "+999", true);
            var c = new Chat(ChatType.Group);
            c.AddMember(u);

            var sent = new Text(u, c, "test", false);
            sent.InitializeAsSent(DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(-5), null, null);

            var ok = Persistence.LoadAll("__does_not_exist__.xml");

            Assert.IsFalse(ok);
            Assert.That(Text.GetAll(), Is.Empty);
        }

        [Test]
        public void Save_ThrowsException_IfPathInvalid()
        {
            var u = new User("temp", "+999", true);
            var c = new Chat(ChatType.Group);
            c.AddMember(u);

            var sent = new Text(u, c, "test", false);
            sent.InitializeAsSent(DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(-5), null, null);

            Assert.Throws<DirectoryNotFoundException>(() =>
                Persistence.SaveAll("Z:/folder_does_not_exist/sent.xml"));
        }

    }
}