using ABDULLAgram.Chats;
using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Associations.Basic
{
    [TestFixture]
    public class GroupAdminAssociationTests
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
        public void SetAdmin_SetsReverseConnection()
        {
            var user = new TestUser("Alice");
            var group = new Chat(ChatType.Group) { Name = "Study Group" };

            group.SetAdmin(user);

            Assert.That(group.Admin, Is.EqualTo(user));
            Assert.That(user.AdminOfGroups, Contains.Item(group));
        }

        [Test]
        public void ChangingAdmin_UpdatesReverseConnections()
        {
            var alice = new TestUser("Alice");
            var bob = new TestUser("Bob");
            var group = new Chat(ChatType.Group) { Name = "Study Group" };

            group.SetAdmin(alice);
            group.SetAdmin(bob);

            Assert.That(group.Admin, Is.EqualTo(bob));
            Assert.That(alice.AdminOfGroups, Does.Not.Contain(group));
            Assert.That(bob.AdminOfGroups, Contains.Item(group));
        }

        [Test]
        public void OnlyAdmin_CanKickMember()
        {
            var admin = new TestUser("Admin");
            var user = new TestUser("User");
            var group = new Chat(ChatType.Group) { Name = "Study Group" };

            group.AddMember(admin);
            group.AddMember(user);
            group.SetAdmin(admin);

            Assert.Throws<InvalidOperationException>(() =>
                group.KickMember(user, admin.PhoneNumber));
        }
    }
}