namespace ABDULLAgram.Messages
{
    public class Sent : Message
    {
        public DateTime SendTimestamp { get; set; }
        public DateTime ServerReadAt { get; set; }
        public DateTime? DeletedAt { get; set; } 
        public bool IsEdited { get; set; }
    }
}