using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Core.Users;

[TestFixture]
public class RegularValidationTests
{
    [SetUp] 
    public void Setup() => User.ClearExtent();

    [Test]
    public void Ctor_EmptyUsername_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new User("", "+111", true,  new RegularUserBehavior(adFrequency: 1)));
    }

    [Test]
    public void Ctor_EmptyPhone_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new User("alice", "   ", true,  new RegularUserBehavior(adFrequency: 1)));
    }

    [Test]
    public void Set_Username_Empty_Throws()
    {
        var r = new User("valid", "+111", true,  new RegularUserBehavior(adFrequency: 1));
            
        Assert.Throws<ArgumentException>(() => r.Username = "");
        Assert.Throws<ArgumentException>(() => r.Username = "   ");
    }

    [Test]
    public void Set_PhoneNumber_Empty_Throws()
    {
        var r = new User("valid", "+111", true,  new RegularUserBehavior(adFrequency: 1));

        Assert.Throws<ArgumentException>(() => r.PhoneNumber = "");
    }

    [Test]
    public void Set_LastSeenAt_Future_Throws()
    {
        var r = new User("alice", "+111", true,  new RegularUserBehavior(adFrequency: 1));
            
        // Valid case
        r.LastSeenAt = DateTime.Now.AddMinutes(-1);

        // Invalid case
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            r.LastSeenAt = DateTime.Now.AddDays(1));
    }

    [Test]
    public void Set_AdFrequency_Negative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RegularUserBehavior(adFrequency: -1)
        );
    }
        
    [Test]
    public void Ctor_DuplicatePhone_Throws()
    {
        var _ = new User("alice", "+111", true,  new RegularUserBehavior(adFrequency: 1));
        Assert.Throws<InvalidOperationException>(() =>
            new User("bob", "+111", false,  new RegularUserBehavior(adFrequency: 2))); 
    }
}