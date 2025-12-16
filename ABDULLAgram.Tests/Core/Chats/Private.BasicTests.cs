using ABDULLAgram.Chats;

namespace ABDULLAgram.Tests.Core.Chats;

[TestFixture]
public class PrivateTests
{
    [Test]
    public void Set_MaxParticipants_Invalid_ThrowsArgumentOutOfRangeException()
    {
        var pChat = new Private();
        // Private chats are strictly for 2 people
        Assert.Throws<ArgumentOutOfRangeException>(() => pChat.MaxParticipants = 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => pChat.MaxParticipants = 1);
    }

    [Test]
    public void Set_MaxParticipants_Valid_SetsValue()
    {
        var pChat = new Private();
        pChat.MaxParticipants = 2;
        Assert.That(pChat.MaxParticipants, Is.EqualTo(2));
    }
}