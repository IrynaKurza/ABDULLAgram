using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Core.Users;

[TestFixture]
    public class PersistenceTests
    {
        private string TestPath => Path.Combine(TestContext.CurrentContext.WorkDirectory, "test_regulars.xml");

        [SetUp]
        public void Setup()
        {
            User.ClearExtent();
            Persistence.DeleteAll(TestPath);
        }

        [TearDown]
        public void Cleanup() => Persistence.DeleteAll(TestPath);

        [Test]
        public void SaveAndLoad_PreservesAllData()
        {
            var user1 = new User("alice", "+000", true) { LastSeenAt = DateTime.Today };
            user1.InitializeAsRegular(2);
            var user2 = new User("bob", "+111", false) { LastSeenAt = null };
            user2.InitializeAsRegular(5);
            
            Persistence.SaveAll(TestPath);
            Assert.That(File.Exists(TestPath), Is.True);

            User.ClearExtent();
            var ok = Persistence.LoadAll(TestPath);

            Assert.IsTrue(ok);
            var all = User.GetAll().ToList();

            Assert.That(all, Has.Count.EqualTo(2));
            Assert.That(all.Any(r => r.Username == "alice" && r.IsOnline && r.LastSeenAt != null));
            Assert.That(all.Any(r => r.Username == "bob" && !r.IsOnline && r.LastSeenAt == null));
        }

        [Test]
        public void Load_WhenFileMissing_ReturnsFalseAndClearsExtent()
        {
            var user = new User("temp", "+999", false);
            user.InitializeAsRegular(9);
            
            var ok = Persistence.LoadAll("__does_not_exist__.xml");

            Assert.IsFalse(ok);
            Assert.That(User.GetAll(), Is.Empty);
        }

        [Test]
        public void Save_ThrowsException_IfPathInvalid()
        {
            var user = new User("err", "+999", true);
            user.InitializeAsRegular(1);

            Assert.Throws<DirectoryNotFoundException>(() =>
                Persistence.SaveAll("Z:/folder_does_not_exist/regulars.xml"));
        }
    }
