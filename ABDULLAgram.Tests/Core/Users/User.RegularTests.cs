using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Core.Users
{
    [TestFixture]
    public class RegularTests
    {
        [SetUp]
        public void Setup() => User.ClearExtent();

        [Test]
        public void Constructor_AddsToExtent()
        {
            var _ = new User("alice", "+380001", true,  new RegularUserBehavior(adFrequency: 3));

            Assert.That(User.GetAll().Count, Is.EqualTo(1));
            Assert.That(User.GetAll().First().Username, Is.EqualTo("alice"));
            Assert.That(User.GetAll().First().IsOnline, Is.True);
        }

        [Test]
        public void GetAll_IsReadOnly()
        {
            var r = new User("bob", "+111", false,  new RegularUserBehavior(adFrequency: 2));
            var view = User.GetAll();

            Assert.Throws<NotSupportedException>(() =>
                ((ICollection<User>)view).Add(r));
        }
    }
    
}