using ABDULLAgram.Chats;
using ABDULLAgram.Support;
using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Integration;

[TestFixture]
public class IntegrationTests
{
    [SetUp]
    public void SetUp()
    {
        User.ClearExtent();
    }

    // TEST: Complex scenario with multiple associations
    [Test]
    public void ComplexScenario_MultipleUsersAndChats()
    {
        // Arrange
        var chat1 = new Group { Name = "Friends" };
        var chat2 = new Group { Name = "Family" };
        var user1 = new User("Alice", "+48111222333", true, new RegularUserBehavior(adFrequency: 5));
        var user2 = new User("Bob", "+48222333444", true, new RegularUserBehavior(adFrequency: 3));
        var pack1 = new Stickerpack ("Emojis", user1) { IsPremium = false };

        // Act - Create multiple associations
        chat1.AddMember(user1);
        chat1.AddMember(user2);
        chat2.AddMember(user1);

        user1.SaveStickerpack(pack1);
        user2.SaveStickerpack(pack1);

        // Assert - All associations correct
        Assert.That(chat1.Members.Count, Is.EqualTo(2));
        Assert.That(chat2.Members.Count, Is.EqualTo(1));
        Assert.That(user1.JoinedChats.Count, Is.EqualTo(2));
        Assert.That(user2.JoinedChats.Count, Is.EqualTo(1));
        Assert.That(pack1.SavedByUsers.Count, Is.EqualTo(2));
    }
}