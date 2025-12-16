using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Core.Users;

[TestFixture]
    public class PersistenceTests
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
