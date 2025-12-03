using ABDULLAgram.Users;
using System.Xml.Serialization;
using ABDULLAgram.Messages;

namespace ABDULLAgram.Chats
{
    [XmlInclude(typeof(Group))]
    [XmlInclude(typeof(Private))]
    [Serializable]
    public abstract class Chat
    {
        private string _name = "";
        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Chat Name cannot be empty.");
                _name = value;
            }
        }

        private DateTime _createdAt;
        public DateTime CreatedAt
        {
            get => _createdAt;
            set
            {
                if (value > DateTime.Now)
                    throw new ArgumentOutOfRangeException(nameof(CreatedAt), "CreatedAt cannot be in the future.");
                _createdAt = value;
            }
        }

        // ============================================================
        // QUALIFIED ASSOCIATION: Chat ↔ User (qualified by phoneNumber)
        // ============================================================
        
        // Dictionary stores users indexed by their phone number
        private Dictionary<string, User> _members = new Dictionary<string, User>();

        // Public getter returns read-only view
        public IReadOnlyDictionary<string, User> Members => _members;

        protected Chat() 
        {
            _createdAt = DateTime.Now;
        }

        // ==== PUBLIC METHODS ====
        
        // Add user to chat by phoneNumber (qualified association)
        public void AddMember(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User cannot be null.");

            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                throw new ArgumentException("User must have a valid phone number.");

            // Check if this phone number already exists in the chat
            if (_members.ContainsKey(user.PhoneNumber))
                throw new InvalidOperationException($"A user with phone number {user.PhoneNumber} is already a member of this chat.");

            // Add to this chat's members dictionary
            _members.Add(user.PhoneNumber, user);

            // REVERSE CONNECTION: Tell user about this chat
            user.AddChatInternal(this);
        }

        // Remove user from chat by phoneNumber
        public void RemoveMember(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Phone number cannot be empty.");

            if (!_members.ContainsKey(phoneNumber))
                throw new InvalidOperationException($"No user with phone number {phoneNumber} found in this chat.");

            User user = _members[phoneNumber];
            _members.Remove(phoneNumber);

            // REVERSE CONNECTION: Tell user to forget this chat
            user.RemoveChatInternal(this);
        }

        // Get user by phone number (qualified lookup) - THIS IS THE KEY FEATURE!
        public User? GetMemberByPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return null;

            return _members.ContainsKey(phoneNumber) ? _members[phoneNumber] : null;
        }

        // Update user's phone number in the dictionary
        public void UpdateMemberPhoneNumber(string oldPhoneNumber, string newPhoneNumber)
        {
            if (!_members.ContainsKey(oldPhoneNumber))
                throw new InvalidOperationException($"No user with phone number {oldPhoneNumber} found in this chat.");

            if (_members.ContainsKey(newPhoneNumber) && oldPhoneNumber != newPhoneNumber)
                throw new InvalidOperationException($"A user with phone number {newPhoneNumber} already exists in this chat.");

            User user = _members[oldPhoneNumber];
            _members.Remove(oldPhoneNumber);
            _members.Add(newPhoneNumber, user);
        }

        // ==== INTERNAL METHODS (called by User) ====
        
        internal void AddMemberInternal(User user)
        {
            if (!_members.ContainsKey(user.PhoneNumber))
            {
                _members.Add(user.PhoneNumber, user);
            }
        }

        internal void RemoveMemberInternal(string phoneNumber)
        {
            _members.Remove(phoneNumber);
        
        // 1. Reverse Connection: A Chat knows its history
        private readonly List<Message> _history = new();

        // This represents the {History} association constraint
        public IReadOnlyList<Message> History => _history.AsReadOnly();

        internal void AddMessage(Message message)
        {
            if (!_history.Contains(message))
            {
                _history.Add(message);
            }
        }

        internal void RemoveMessage(Message message)
        {
            if (_history.Contains(message))
            {
                _history.Remove(message);
            }
        }
    }
}