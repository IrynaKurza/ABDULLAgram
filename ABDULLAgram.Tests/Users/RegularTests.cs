using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Users
{
    [TestFixture]
    public class RegularTests
    {
        [SetUp]
        public void Setup() => Regular.ClearExtent();

        [Test]
        public void Constructor_AddsToExtent()
        {
            var _ = new Regular("alice", "+380001", true, 3);

            Assert.That(Regular.GetAll().Count, Is.EqualTo(1));
            Assert.That(Regular.GetAll().First().Username, Is.EqualTo("alice"));
            Assert.That(Regular.GetAll().First().IsOnline, Is.True);
        }

        [Test]
        public void StaticProperty_AppliesToAll()
        {
            Regular.MaxStickerPacksSaved = 15;

            var _r1 = new Regular("bob",  "+111", false, 2);
            var _r2 = new Regular("dina", "+222", true,  5);

            Assert.That(Regular.MaxStickerPacksSaved, Is.EqualTo(15));
            Assert.That(Regular.GetAll().All(_ => Regular.MaxStickerPacksSaved == 15));
        }

        [Test]
        public void Status_Derived_IsCorrect()
        {
            var r = new Regular("test", "+333", true, 1);
            Assert.That(r.Status, Is.EqualTo("Online"));

            r.IsOnline = false;
            Assert.That(r.Status, Is.EqualTo("Offline"));
        }

        [Test]
        public void GetAll_IsReadOnly()
        {
            var r = new Regular("bob", "+111", false, 2);
            var view = Regular.GetAll();

            Assert.Throws<NotSupportedException>(() =>
                ((ICollection<Regular>)view).Add(r));
        }
    }

    [TestFixture]
    public class RegularPersistenceTests
    {
        private string TestPath => Path.Combine(TestContext.CurrentContext.WorkDirectory, "test_regulars.xml");

        [SetUp]
        public void Setup()
        {
            Regular.ClearExtent();
            Persistence.DeleteAll(TestPath);
        }

        [TearDown]
        public void Cleanup() => Persistence.DeleteAll(TestPath);

        [Test]
        public void SaveAndLoad_PreservesAllData()
        {
            new Regular("alice", "+000", true, 2) { LastSeenAt = DateTime.Today };
            new Regular("bob", "+111", false, 5) { LastSeenAt = null };

            Persistence.SaveAll(TestPath);
            Assert.That(File.Exists(TestPath), Is.True);

            Regular.ClearExtent();
            var ok = Persistence.LoadAll(TestPath);

            Assert.IsTrue(ok);
            var all = Regular.GetAll().ToList();

            Assert.That(all, Has.Count.EqualTo(2));
            Assert.That(all.Any(r => r.Username == "alice" && r.IsOnline && r.LastSeenAt != null));
            Assert.That(all.Any(r => r.Username == "bob" && !r.IsOnline && r.LastSeenAt == null));
        }

        [Test]
        public void Load_WhenFileMissing_ReturnsFalseAndClearsExtent()
        {
            new Regular("temp", "+999", false, 9);

            var ok = Persistence.LoadAll("__does_not_exist__.xml");

            Assert.IsFalse(ok);
            Assert.That(Regular.GetAll(), Is.Empty);
        }

        [Test]
        public void Save_ThrowsException_IfPathInvalid()
        {
            new Regular("err", "+999", true, 1);

            Assert.Throws<DirectoryNotFoundException>(() =>
                Persistence.SaveAll("Z:/folder_does_not_exist/regulars.xml"));
        }
    }
    
    [TestFixture]
    public class RegularValidationTests
    {
        [SetUp] 
        public void Setup() => Regular.ClearExtent();

        [Test]
        public void Ctor_EmptyUsername_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                new Regular("", "+111", true, 1));
        }

        [Test]
        public void Ctor_EmptyPhone_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                new Regular("alice", "   ", true, 1));
        }

        [Test]
        public void Set_Username_Empty_Throws()
        {
            var r = new Regular("valid", "+111", true, 1);
            
            Assert.Throws<ArgumentException>(() => r.Username = "");
            Assert.Throws<ArgumentException>(() => r.Username = "   ");
        }

        [Test]
        public void Set_PhoneNumber_Empty_Throws()
        {
            var r = new Regular("valid", "+111", true, 1);

            Assert.Throws<ArgumentException>(() => r.PhoneNumber = "");
        }

        [Test]
        public void Set_LastSeenAt_Future_Throws()
        {
            var r = new Regular("alice", "+111", true, 1);
            
            // Valid case
            r.LastSeenAt = DateTime.Now.AddMinutes(-1);

            // Invalid case
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                r.LastSeenAt = DateTime.Now.AddDays(1));
        }

        [Test]
        public void Set_AdFrequency_Negative_Throws()
        {
            var r = new Regular("alice", "+111", true, 1);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                r.AdFrequency = -1);
        }

        [Test]
        public void Set_MaxStickerPacksSaved_Negative_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Regular.MaxStickerPacksSaved = -5);
        }
        
        [Test]
        public void Ctor_DuplicatePhone_Throws()
        {
            var _ = new Regular("alice", "+111", true, 1);
            Assert.Throws<InvalidOperationException>(() =>
                new Regular("bob", "+111", false, 2)); 
        }
    }
}