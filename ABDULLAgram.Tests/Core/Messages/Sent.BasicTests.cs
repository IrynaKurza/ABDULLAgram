using ABDULLAgram.Messages;
using ABDULLAgram.Users;
using ABDULLAgram.Chats;

namespace ABDULLAgram.Tests.Core.Messages
{
    [TestFixture]
    public class SentTests
    {
        [SetUp]
        public void Setup()
        {
            Sent.ClearExtent();
        }

        [Test]
        public void Constructor_AddsToExtent()
        {
            // Sent is now a component, initialized with timestamps only
            var sent = new Sent(DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(-5), null, null);

            Assert.That(Sent.GetAll().Count, Is.EqualTo(1));
            Assert.That(Sent.GetAll().First().SendTimestamp, Is.LessThanOrEqualTo(DateTime.Now));
        }

        [Test]
        public void GetAll_IsReadOnly()
        {
            var sent = new Sent(DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(-5), null, null);
            var view = Sent.GetAll();

            Assert.Throws<NotSupportedException>(() =>
                ((ICollection<Sent>)view).Add(sent));
        }
    }

    [TestFixture]
    public class SentValidationTests
    {
        [SetUp]
        public void Setup()
        {
            Sent.ClearExtent();
        }

        [Test]
        public void Duplicate_Id_Throws()
        {
            var s1 = new Sent(DateTime.Now, DateTime.Now, null, null);
            var s2 = new Sent(DateTime.Now, DateTime.Now, null, null);
            
            Assert.Throws<InvalidOperationException>(() =>
                s2.Id = s1.Id);
        }

        [Test]
        public void Set_SendTimestamp_Future_Throws()
        {
            var sent = new Sent(DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(-5), null, null);
            
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sent.SendTimestamp = DateTime.Now.AddDays(1));
        }

        [Test]
        public void Set_DeliveredAt_Future_Throws()
        {
            var sent = new Sent(DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(-5), null, null);
            
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sent.DeliveredAt = DateTime.Now.AddDays(1));
        }

        [Test]
        public void Set_DeliveredAt_BeforeSendTimestamp_Throws()
        {
            var sendTime = DateTime.Now.AddMinutes(-10);
            var sent = new Sent(sendTime, DateTime.Now.AddMinutes(-5), null, null);
            
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sent.DeliveredAt = sendTime.AddMinutes(-5));
        }

        [Test]
        public void Set_EditedAt_Future_Throws()
        {
            var sent = new Sent(DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(-5), null, null);
            
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sent.EditedAt = DateTime.Now.AddDays(1));
        }

        [Test]
        public void Set_EditedAt_BeforeSendTimestamp_Throws()
        {
            var sendTime = DateTime.Now.AddMinutes(-10);
            var sent = new Sent(sendTime, DateTime.Now.AddMinutes(-5), null, null);
            
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sent.EditedAt = sendTime.AddMinutes(-5));
        }

        [Test]
        public void Set_EditedAt_Null_Succeeds()
        {
            var sent = new Sent(DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(-5), DateTime.Now.AddMinutes(-3), null);
            
            sent.EditedAt = null;
            Assert.That(sent.EditedAt, Is.Null);
        }

        [Test]
        public void Set_DeletedAt_Future_Throws()
        {
            var sent = new Sent(DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(-5), null, null);
            
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sent.DeletedAt = DateTime.Now.AddDays(1));
        }

        [Test]
        public void Set_DeletedAt_BeforeSendTimestamp_Throws()
        {
            var sendTime = DateTime.Now.AddMinutes(-10);
            var sent = new Sent(sendTime, DateTime.Now.AddMinutes(-5), null, null);
            
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sent.DeletedAt = sendTime.AddMinutes(-5));
        }

        [Test]
        public void Set_DeletedAt_Null_Succeeds()
        {
            var sent = new Sent(DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(-5), null, DateTime.Now.AddMinutes(-2));
            
            sent.DeletedAt = null;
            Assert.That(sent.DeletedAt, Is.Null);
        }
    }
}