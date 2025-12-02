using ABDULLAgram.Messages;

namespace ABDULLAgram.Tests.Messages
{
    [TestFixture]
    public class SentTests
    {
        [SetUp]
        public void Setup() => Sent.ClearExtent();

        [Test]
        public void Constructor_AddsToExtent()
        {
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
        public void Setup() => Sent.ClearExtent();

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

    [TestFixture]
    public class SentPersistenceTests
    {
        private string TestPath => Path.Combine(TestContext.CurrentContext.WorkDirectory, "test_sent.xml");

        [SetUp]
        public void Setup()
        {
            Sent.ClearExtent();
            Persistence.DeleteAll(TestPath);
        }

        [TearDown]
        public void Cleanup() => Persistence.DeleteAll(TestPath);

        [Test]
        public void SaveAndLoad_PreservesAllData()
        {
            var sendTime1 = DateTime.Now.AddMinutes(-20);
            var sendTime2 = DateTime.Now.AddMinutes(-15);
            
            new Sent(sendTime1, sendTime1.AddMinutes(2), sendTime1.AddMinutes(5), null);
            new Sent(sendTime2, sendTime2.AddMinutes(1), null, sendTime2.AddMinutes(3));

            Persistence.SaveAll(TestPath);
            Assert.That(File.Exists(TestPath), Is.True);

            Sent.ClearExtent();
            var ok = Persistence.LoadAll(TestPath);

            Assert.IsTrue(ok);
            var all = Sent.GetAll().ToList();

            Assert.That(all, Has.Count.EqualTo(2));
            Assert.That(all.Any(s => s.EditedAt != null && s.DeletedAt == null));
            Assert.That(all.Any(s => s.EditedAt == null && s.DeletedAt != null));
        }

        [Test]
        public void Load_WhenFileMissing_ReturnsFalseAndClearsExtent()
        {
            new Sent(DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(-5), null, null);

            var ok = Persistence.LoadAll("__does_not_exist__.xml");

            Assert.IsFalse(ok);
            Assert.That(Sent.GetAll(), Is.Empty);
        }

        [Test]
        public void Save_ThrowsException_IfPathInvalid()
        {
            new Sent(DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(-5), null, null);

            Assert.Throws<DirectoryNotFoundException>(() =>
                Persistence.SaveAll("Z:/folder_does_not_exist/sent.xml"));
        }
    }
}