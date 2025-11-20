using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using ABDULLAgram.Users;
using ABDULLAgram; // For Persistence

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
                ((System.Collections.Generic.ICollection<Regular>)view).Add(r));
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
    }
    
    [TestFixture]
    public class UserValidationTests
    {
        [SetUp] public void Setup() => Regular.ClearExtent();

        [Test]
        public void User_SetUsername_Empty_Throws()
        {
            // Valid case
            var r = new Regular("valid", "+111", true, 1);
            
            // Invalid cases (Base class logic)
            Assert.Throws<ArgumentException>(() => r.Username = "");
            Assert.Throws<ArgumentException>(() => r.Username = "   ");
            Assert.Throws<ArgumentException>(() => new Regular("", "+222", true, 1));
        }

        [Test]
        public void User_SetPhoneNumber_Empty_Throws()
        {
            // Valid case
            var r = new Regular("valid", "+111", true, 1);

            // Invalid cases (Base class logic)
            Assert.Throws<ArgumentException>(() => r.PhoneNumber = "");
            Assert.Throws<ArgumentException>(() => new Regular("valid", "", true, 1));
        }

        [Test]
        public void User_SetLastSeenAt_Future_Throws()
        {
            var r = new Regular("alice", "+111", true, 1);
            
            // Valid
            r.LastSeenAt = DateTime.Now.AddMinutes(-1);

            // Invalid (Base class logic)
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                r.LastSeenAt = DateTime.Now.AddDays(1));
        }

        [Test]
        public void Regular_PhoneNumber_Duplicate_Throws()
        {
            // This tests that the overridden logic in Regular still works
            var _ = new Regular("alice", "+111", true, 1);
            
            Assert.Throws<InvalidOperationException>(() =>
                new Regular("bob", "+111", false, 2)); // Duplicate check
        }
    }
}