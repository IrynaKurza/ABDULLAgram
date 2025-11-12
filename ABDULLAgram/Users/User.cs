namespace ABDULLAgram.Users
{
    public abstract class User
    {
        public string Username { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? LastSeenAt { get; set; } 
        public bool IsOnline { get; set; }
    }
}