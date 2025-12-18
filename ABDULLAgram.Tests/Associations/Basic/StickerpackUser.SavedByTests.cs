using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Associations.Basic;

[TestFixture]
public class SavedByTests
{
    [Test]
    public void MaxSavedStickerpacks_IsCorrect()
    {
        var r1 = new User(
            username: "bob",
            phoneNumber: "+111",
            isOnline: false,
            behavior: new RegularUserBehavior(3)
        );

        var r2 = new User(
            username: "dina",
            phoneNumber: "+222",
            isOnline: true,
            behavior: new RegularUserBehavior(5)
        );


        // Regular users have max 10 stickerpacks (polymorphism)
        Assert.That(r1.MaxSavedStickerpacks, Is.EqualTo(10));
        Assert.That(r2.MaxSavedStickerpacks, Is.EqualTo(10));
    }
    
}