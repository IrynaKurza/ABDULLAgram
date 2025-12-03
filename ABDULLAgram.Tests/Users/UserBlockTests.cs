using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Users
{
    [TestFixture]
    public class UserBlockTests
    {
        [SetUp]
        public void Setup() => Regular.ClearExtent();

        [Test]
        public void BlockUser_Self_ThrowsException()
        {
            var alice = new Regular("alice", "+100", true, 1);

            Assert.Throws<InvalidOperationException>(() => alice.BlockUser(alice));
        }

        [Test]
        public void BlockUser_CreatesReverseLink()
        {
            var alice = new Regular("alice", "+100", true, 1);
            var bob = new Regular("bob", "+200", false, 2);

            alice.BlockUser(bob);

            Assert.That(alice.GetBlockedUsers().Contains(bob), Is.True);
            Assert.That(bob.GetBlockedBy().Contains(alice), Is.True);
        }

        [Test]
        public void BlockUser_Idempotent_KeepsReverseLinkStable()
        {
            var alice = new Regular("alice", "+100", true, 1);
            var bob = new Regular("bob", "+200", false, 2);

            alice.BlockUser(bob);
            alice.BlockUser(bob);

            Assert.That(alice.GetBlockedUsers().Count, Is.EqualTo(1));
            Assert.That(bob.GetBlockedBy().Count, Is.EqualTo(1));
        }
    }
}

