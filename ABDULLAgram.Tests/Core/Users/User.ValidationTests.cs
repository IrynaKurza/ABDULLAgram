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
        // Constructor validation - happens at User() level
        Assert.Throws<ArgumentException>(() =>
            new User("", "+111", true));
    }

    [Test]
    public void Ctor_EmptyPhone_Throws()
    {
        // Constructor validation - happens at User() level
        Assert.Throws<ArgumentException>(() =>
            new User("alice", "   ", true));
    }

    [Test]
    public void Set_Username_Empty_Throws()
    {
        var user = new User("valid", "+111", true);
        user.InitializeAsRegular(1);
            
        Assert.Throws<ArgumentException>(() => user.Username = "");
        Assert.Throws<ArgumentException>(() => user.Username = "   ");
    }

    [Test]
    public void Set_PhoneNumber_Empty_Throws()
    {
        var user = new User("valid", "+111", true);
        user.InitializeAsRegular(1);

        Assert.Throws<ArgumentException>(() => user.PhoneNumber = "");
    }

    [Test]
    public void Set_LastSeenAt_Future_Throws()
    {
        var user = new User("alice", "+111", true);
        user.InitializeAsRegular(1);
            
        // Valid case
        user.LastSeenAt = DateTime.Now.AddMinutes(-1);

        // Invalid case
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            user.LastSeenAt = DateTime.Now.AddDays(1));
    }

    [Test]
    public void Set_AdFrequency_Negative_Throws()
    {
        // AdFrequency validation happens in InitializeAsRegular()
        var user = new User("alice", "+222", true);
        
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            user.InitializeAsRegular(-1)
        );
    }
        
    [Test]
    public void Ctor_DuplicatePhone_Throws()
    {
        var user1 = new User("alice", "+111", true);
        user1.InitializeAsRegular(1);
        
        // Duplicate phone throws at constructor level
        Assert.Throws<InvalidOperationException>(() =>
            new User("bob", "+111", false)); 
    }
}