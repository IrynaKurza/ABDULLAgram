using System.Xml.Serialization;
using ABDULLAgram.Messages;

namespace ABDULLAgram.Users
{
    [XmlInclude(typeof(Regular))]
    [XmlInclude(typeof(Premium))]
    [Serializable]
    public abstract class User
    {
        private string _username = "";
        public string Username
        {
            get => _username;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Username cannot be empty.");
                _username = value;
            }
        }

        private string _phoneNumber = "";
        public virtual string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("PhoneNumber cannot be empty.");
                _phoneNumber = value;
            }
        }

        private DateTime? _lastSeenAt;
        public DateTime? LastSeenAt
        {
            get => _lastSeenAt;
            set
            {
                if (value is DateTime dt && dt > DateTime.Now)
                    throw new ArgumentOutOfRangeException(nameof(LastSeenAt), "LastSeenAt cannot be in the future.");
                _lastSeenAt = value;
            }
        }

        public bool IsOnline { get; set; }
        
        // Reverse Connection: A User knows about the messages they sent
        private readonly List<Message> _messages = new();
        
        public IReadOnlyList<Message> SentMessages => _messages.AsReadOnly();
        
        internal void AddMessage(Message message)
        {
            if (!_messages.Contains(message))
            {
                _messages.Add(message);
            }
        }
        
        internal void RemoveMessage(Message message)
        {
            if (_messages.Contains(message))
            {
                _messages.Remove(message);
            }
        }
    }
}