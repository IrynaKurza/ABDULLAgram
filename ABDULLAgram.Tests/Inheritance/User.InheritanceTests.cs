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
            var user = new User("alice", "+111", true);
            user.InitializeAsRegular(3);

            Assert.That(user.UserType, Is.EqualTo(UserType.Regular));
            Assert.That(user.IsRegular, Is.True);
            Assert.That(user.IsPremium, Is.False);
        }

        [Test]
        public void PremiumUser_CanBeCreated()
        {
            var user = new User("bob", "+222", true);
            user.InitializeAsPremium(
                DateTime.Now.AddDays(-5),
                DateTime.Now.AddDays(10)
            );

            Assert.That(user.UserType, Is.EqualTo(UserType.Premium));
            Assert.That(user.IsRegular, Is.False);
            Assert.That(user.IsPremium, Is.True);
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        [Test]
        public void RegularUser_NegativeAdFrequency_Throws()
        {
            var user = new User("alice", "+333", true);
            
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                user.InitializeAsRegular(-1)
            );
        }

        [Test]
        public void PremiumUser_InvalidDates_Throws()
        {
            var user = new User("bob", "+444", true);
            
            Assert.Throws<ArgumentException>(() =>
                user.InitializeAsPremium(
                    DateTime.Now,
                    DateTime.Now.AddDays(-1) // End before start!
                )
            );
        }

        // ============================================================
        // BEHAVIOR DIFFERENCE (POLYMORPHISM)
        // ============================================================

        [Test]
        public void RegularUser_HasMax10Stickerpacks()
        {
            var user = new User("alice", "+555", true);
            user.InitializeAsRegular(1);

            Assert.That(user.MaxSavedStickerpacks, Is.EqualTo(10));
        }

        [Test]
        public void PremiumUser_HasUnlimitedStickerpacks()
        {
            var user = new User("bob", "+666", true);
            user.InitializeAsPremium(
                DateTime.Now.AddDays(-1),
                DateTime.Now.AddDays(30)
            );

            Assert.That(user.MaxSavedStickerpacks, Is.EqualTo(int.MaxValue));
        }

        // ============================================================
        // TYPE-SPECIFIC PROPERTIES
        // ============================================================

        [Test]
        public void RegularUser_CanAccessAdFrequency()
        {
            var user = new User("alice", "+777", true);
            user.InitializeAsRegular(5);

            Assert.That(user.AdFrequency, Is.EqualTo(5));
            
            // Can modify it
            user.AdFrequency = 10;
            Assert.That(user.AdFrequency, Is.EqualTo(10));
        }

        [Test]
        public void PremiumUser_CannotAccessAdFrequency()
        {
            var user = new User("bob", "+888", true);
            user.InitializeAsPremium(DateTime.Now, DateTime.Now.AddDays(30));

            Assert.Throws<InvalidOperationException>(() =>
            {
                var _ = user.AdFrequency; // Trying to access Regular-only property
            });
        }

        [Test]
        public void PremiumUser_CanAccessPremiumDates()
        {
            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddDays(30);
            
            var user = new User("bob", "+999", true);
            user.InitializeAsPremium(startDate, endDate);

            Assert.That(user.PremiumStartDate, Is.EqualTo(startDate));
            Assert.That(user.PremiumEndDate, Is.EqualTo(endDate));
        }

        [Test]
        public void RegularUser_CannotAccessPremiumDates()
        {
            var user = new User("alice", "+1000", true);
            user.InitializeAsRegular(1);

            Assert.Throws<InvalidOperationException>(() =>
            {
                var _ = user.PremiumStartDate;
            });
            
            Assert.Throws<InvalidOperationException>(() =>
            {
                var _ = user.PremiumEndDate;
            });
        }

        // ============================================================
        // DYNAMIC INHERITANCE (KEY PART OF ASSIGNMENT 7)
        // ============================================================

        [Test]
        public void RegularUser_CanUpgradeToPremium()
        {
            var user = new User("alice", "+1100", true);
            user.InitializeAsRegular(2);

            Assert.That(user.UserType, Is.EqualTo(UserType.Regular));

            // Upgrade!
            user.UpgradeToPremium(
                DateTime.Now,
                DateTime.Now.AddDays(30)
            );

            Assert.That(user.UserType, Is.EqualTo(UserType.Premium));
            Assert.That(user.IsPremium, Is.True);
            Assert.That(user.IsRegular, Is.False);
        }

        [Test]
        public void AfterUpgrade_RegularPropertiesNotAccessible()
        {
            var user = new User("alice", "+1200", true);
            user.InitializeAsRegular(5);
            
            Assert.That(user.AdFrequency, Is.EqualTo(5)); // Works before upgrade

            user.UpgradeToPremium(DateTime.Now, DateTime.Now.AddDays(30));

            // After upgrade, AdFrequency no longer accessible
            Assert.Throws<InvalidOperationException>(() =>
            {
                var _ = user.AdFrequency;
            });
        }

        [Test]
        public void AfterUpgrade_PremiumPropertiesAccessible()
        {
            var user = new User("alice", "+1300", true);
            user.InitializeAsRegular(1);

            user.UpgradeToPremium(
                DateTime.Now,
                DateTime.Now.AddDays(20)
            );

            // Now can access Premium properties
            Assert.That(user.PremiumStartDate, Is.LessThanOrEqualTo(DateTime.Now));
            Assert.That(user.PremiumEndDate, Is.GreaterThan(DateTime.Now));
        }

        [Test]
        public void AfterUpgrade_MaxStickerpacksChanges()
        {
            var user = new User("alice", "+1400", true);
            user.InitializeAsRegular(1);

            Assert.That(user.MaxSavedStickerpacks, Is.EqualTo(10)); // Regular limit

            user.UpgradeToPremium(DateTime.Now, DateTime.Now.AddDays(30));

            Assert.That(user.MaxSavedStickerpacks, Is.EqualTo(int.MaxValue)); // Unlimited!
        }

        [Test]
        public void PremiumUser_UpgradeAgain_Throws()
        {
            var user = new User("bob", "+1500", true);
            user.InitializeAsPremium(
                DateTime.Now.AddDays(-5),
                DateTime.Now.AddDays(5)
            );

            Assert.Throws<InvalidOperationException>(() =>
                user.UpgradeToPremium(
                    DateTime.Now,
                    DateTime.Now.AddDays(10)
                )
            );
        }

        // ============================================================
        // PREMIUM-SPECIFIC METHODS
        // ============================================================

        [Test]
        public void PremiumUser_CanCalculateDaysUntilDue()
        {
            var user = new User("bob", "+1600", true);
            user.InitializeAsPremium(
                DateTime.Now,
                DateTime.Now.AddDays(20)
            );

            var days = user.CalculateDaysUntilDue();
            
            Assert.That(days, Is.GreaterThanOrEqualTo(0));
            Assert.That(days, Is.LessThanOrEqualTo(20));
        }

        [Test]
        public void RegularUser_CannotCalculateDaysUntilDue()
        {
            var user = new User("alice", "+1700", true);
            user.InitializeAsRegular(1);

            Assert.Throws<InvalidOperationException>(() =>
                user.CalculateDaysUntilDue()
            );
        }

        [Test]
        public void PremiumUser_CanCancelSubscription()
        {
            var user = new User("bob", "+1800", true);
            user.InitializeAsPremium(
                DateTime.Now,
                DateTime.Now.AddDays(30)
            );

            user.CancelSubscription();

            // After cancellation, end date should be now or very close
            var daysRemaining = user.CalculateDaysUntilDue();
            Assert.That(daysRemaining, Is.LessThanOrEqualTo(1));
        }

        [Test]
        public void RegularUser_CannotCancelSubscription()
        {
            var user = new User("alice", "+1900", true);
            user.InitializeAsRegular(1);

            Assert.Throws<InvalidOperationException>(() =>
                user.CancelSubscription()
            );
        }
    }
}