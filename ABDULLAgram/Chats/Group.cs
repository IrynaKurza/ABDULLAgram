namespace ABDULLAgram.Chats
{
    public class Group : Chat
    {
        public int MaxParticipants { get; set; } = 100;
        public string Description { get; set; }
    }
}