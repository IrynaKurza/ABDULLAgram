using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Core.Users
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
        public void GetAll_IsReadOnly()
        {
            var r = new Regular("bob", "+111", false, 2);
            var view = Regular.GetAll();

            Assert.Throws<NotSupportedException>(() =>
                ((ICollection<Regular>)view).Add(r));
        }
    }
    
}