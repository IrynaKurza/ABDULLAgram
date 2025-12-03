namespace ABDULLAgram.Users
{
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

        // Reflex Association
        private List<User> _blockedUsers = new();
        private List<User> _blockedBy = new();

        public void BlockUser(User user)
        {
            if (user == this)
                throw new InvalidOperationException("Cannot block yourself.");
            if (_blockedUsers.Contains(user)) return;
            _blockedUsers.Add(user);
            user._blockedBy.Add(this);
        }

        public void UnblockUser(User user)
        {
            if (!_blockedUsers.Contains(user)) return;
            _blockedUsers.Remove(user);
            user._blockedBy.Remove(this);
        }

        public IReadOnlyCollection<User> GetBlockedUsers() => _blockedUsers.AsReadOnly();
        public IReadOnlyCollection<User> GetBlockedBy() => _blockedBy.AsReadOnly();
    }
}