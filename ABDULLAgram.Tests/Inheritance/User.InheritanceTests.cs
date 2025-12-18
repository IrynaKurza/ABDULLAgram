using NUnit.Framework;
using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Inheritance
{
    [TestFixture]
    public class UserInheritanceTests
    {
        [SetUp]
        public void Setup()
        {
            User.ClearExtent();
        }

        // ============================================================
        // CREATION
        // ============================================================

        [Test]
        public void RegularUser_CanBeCreated()
        {
            var user = new User(
                "alice",
                "+111",
                true,
                new RegularUserBehavior(adFrequency: 3)
            );

            Assert.That(user.UserType, Is.EqualTo(UserType.Regular));
        }

        [Test]
        public void PremiumUser_CanBeCreated()
        {
            var user = new User(
                "bob",
                "+222",
                true,
                new PremiumUserBehavior(
                    DateTime.Now.AddDays(-5),
                    DateTime.Now.AddDays(10)
                )
            );

            Assert.That(user.UserType, Is.EqualTo(UserType.Premium));
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        [Test]
        public void RegularUser_NegativeAdFrequency_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new RegularUserBehavior(adFrequency: -1)
            );
        }

        [Test]
        public void PremiumUser_InvalidDates_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                new PremiumUserBehavior(
                    DateTime.Now,
                    DateTime.Now.AddDays(-1)
                )
            );
        }

        // ============================================================
        // BEHAVIOR DIFFERENCE
        // ============================================================

        [Test]
        public void RegularUser_HasMax10Stickerpacks()
        {
            var user = new User(
                "alice",
                "+333",
                true,
                new RegularUserBehavior(1)
            );

            Assert.That(user.MaxSavedStickerpacks, Is.EqualTo(10));
        }

        [Test]
        public void PremiumUser_HasUnlimitedStickerpacks()
        {
            var user = new User(
                "bob",
                "+444",
                true,
                new PremiumUserBehavior(
                    DateTime.Now.AddDays(-1),
                    DateTime.Now.AddDays(30)
                )
            );

            Assert.That(user.MaxSavedStickerpacks, Is.EqualTo(int.MaxValue));
        }

        // ============================================================
        // DYNAMIC INHERITANCE (KEY PART)
        // ============================================================

        [Test]
        public void RegularUser_CanUpgradeToPremium()
        {
            var user = new User(
                "alice",
                "+555",
                true,
                new RegularUserBehavior(2)
            );

            user.UpgradeToPremium(
                DateTime.Now,
                DateTime.Now.AddDays(30)
            );

            Assert.That(user.UserType, Is.EqualTo(UserType.Premium));
        }

        [Test]
        public void PremiumUser_UpgradeAgain_Throws()
        {
            var user = new User(
                "bob",
                "+666",
                true,
                new PremiumUserBehavior(
                    DateTime.Now.AddDays(-5),
                    DateTime.Now.AddDays(5)
                )
            );

            Assert.Throws<InvalidOperationException>(() =>
                user.UpgradeToPremium(
                    DateTime.Now,
                    DateTime.Now.AddDays(10)
                )
            );
        }

        [Test]
        public void AfterUpgrade_PremiumBehaviorIsAvailable()
        {
            var user = new User(
                "alice",
                "+777",
                true,
                new RegularUserBehavior(1)
            );

            user.UpgradeToPremium(
                DateTime.Now,
                DateTime.Now.AddDays(20)
            );

            var premium = user.AsPremium();

            Assert.That(premium.CalculateDaysUntilDue(), Is.GreaterThanOrEqualTo(0));
        }
    }
}
