using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Associations.Basic;

[TestFixture]
public class SavedByTests
{
    [Test]
    public void MaxSavedStickerpacks_IsCorrect()
    {
        var r1 = new Regular("bob",  "+111", false, 2);
        var r2 = new Regular("dina", "+222", true,  5);

        // Regular users have max 10 stickerpacks (polymorphism)
        Assert.That(r1.MaxSavedStickerpacks, Is.EqualTo(10));
        Assert.That(r2.MaxSavedStickerpacks, Is.EqualTo(10));
    }
    
}