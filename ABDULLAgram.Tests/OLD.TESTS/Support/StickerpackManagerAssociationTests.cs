using ABDULLAgram.Support;
using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Support
{
    [TestFixture]
    public class StickerpackManagerAssociationTests
    {
        private class TestUser : Regular
        {
            public TestUser(string name)
                : base(name, "+" + name.GetHashCode(), true, 1) { }
        }

        [SetUp]
        public void Setup()
        {
            Regular.ClearExtent();
        }

        [Test]
        public void SetManager_SetsReverseConnection()
        {
            var user = new TestUser(name:"Alice");
            var pack = new Stickerpack(name:"Animals", user);

            pack.SetManager(user);

            Assert.That(pack.Manager, Is.EqualTo(user));
            Assert.That(user.ManagedStickerpacks, Contains.Item(pack));
        }

        [Test]
        public void ChangingManager_UpdatesReverseConnections()
        {
            var alice = new TestUser("Alice");
            var bob = new TestUser("Bob");
            var pack = new Stickerpack("Animals", alice);

            pack.SetManager(alice);
            pack.SetManager(bob);

            Assert.That(pack.Manager, Is.EqualTo(bob));
            Assert.That(alice.ManagedStickerpacks, Does.Not.Contain(pack));
            Assert.That(bob.ManagedStickerpacks, Contains.Item(pack));
        }
    }
}