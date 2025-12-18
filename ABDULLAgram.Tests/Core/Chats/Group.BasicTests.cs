using ABDULLAgram.Chats;

namespace ABDULLAgram.Tests.Core.Chats;

[TestFixture]
public partial class GroupTests
{
    [Test]
    public void Set_MaxParticipants_NegativeOrZero_ThrowsArgumentOutOfRangeException()
    {
        var group = new Chat(ChatType.Group);
        Assert.Throws<ArgumentOutOfRangeException>(() => group.MaxParticipants = 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => group.MaxParticipants = -5);
    }

    [Test]
    public void Set_Description_Null_ThrowsArgumentNullException()
    {
        var group = new Chat(ChatType.Group);
        Assert.Throws<ArgumentNullException>(() => group.Description = null);
    }

    [Test]
    public void Set_Description_Valid_SetsValue()
    {
        var group = new Chat(ChatType.Group);
        group.Description = "Official group";
        Assert.That(group.Description, Is.EqualTo("Official group"));
    }
}