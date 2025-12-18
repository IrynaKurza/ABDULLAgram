using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Core.Users
{
    [TestFixture]
    public class UserBlockTests
    {
        [SetUp]
        public void Setup() => User.ClearExtent();

        [Test]
        public void BlockUser_Self_ThrowsException()
        {
            var alice = new User("alice", "+100", true);
            alice.InitializeAsRegular(1);
            Assert.Throws<InvalidOperationException>(() => alice.BlockUser(alice));
        }

        [Test]
        public void BlockUser_CreatesReverseLink()
        {
            var alice = new User("alice", "+100", true);
            alice.InitializeAsRegular(1);
            var bob = new User("bob", "+200", false);
            bob.InitializeAsRegular(2);

            alice.BlockUser(bob);

            Assert.That(alice.GetBlockedUsers().Contains(bob), Is.True);
            Assert.That(bob.GetBlockedBy().Contains(alice), Is.True);
        }

        [Test]
        public void BlockUser_Idempotent_KeepsReverseLinkStable()
        {
            var alice = new User("alice", "+100", true);
            alice.InitializeAsRegular(1);
            var bob = new User("bob", "+200", false);
            bob.InitializeAsRegular(2);
            
            alice.BlockUser(bob);
            alice.BlockUser(bob);

            Assert.That(alice.GetBlockedUsers().Count, Is.EqualTo(1));
            Assert.That(bob.GetBlockedBy().Count, Is.EqualTo(1));
        }
    }
}