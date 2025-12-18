using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Associations.Basic;

[TestFixture]
public class SavedByTests
{
    [Test]
    public void MaxSavedStickerpacks_IsCorrect()
    {
        var r1 = new User("bob", "+111", false);
        r1.InitializeAsRegular(3);

        var r2 = new User("dina", "+222", true);
        r2.InitializeAsRegular(5);

        Assert.That(r1.MaxSavedStickerpacks, Is.EqualTo(10));
        Assert.That(r2.MaxSavedStickerpacks, Is.EqualTo(10));
    }
    
}