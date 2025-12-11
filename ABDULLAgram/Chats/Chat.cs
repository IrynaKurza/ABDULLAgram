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
        // ============================================================
        // BASIC ATTRIBUTES
        // ============================================================
        
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

        protected Chat() 
        {
            _createdAt = DateTime.Now;
        }

        // ============================================================
        // QUALIFIED ASSOCIATION: Chat ↔ User (qualified by phoneNumber)
        // Dictionary stores users indexed by their phone number (key)
        // Enables O(1) lookup: GetMemberByPhoneNumber()
        // phoneNumber is the QUALIFIER - it qualifies which user we want
        // ============================================================
        
        private Dictionary<string, User> _members = new Dictionary<string, User>();
        
        // IReadOnlyDictionary prevents external modification while allowing lookups
        public IReadOnlyDictionary<string, User> Members => _members;

        // PUBLIC METHOD: Use this to add a member from Chat side
        // Validates everything, then creates reverse connection
        public void AddMember(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User cannot be null.");

            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                throw new ArgumentException("User must have a valid phone number.");
            
            // Dictionary automatically prevents duplicate keys (phone numbers)
            if (_members.ContainsKey(user.PhoneNumber))
                throw new InvalidOperationException($"A user with phone number {user.PhoneNumber} is already a member of this chat.");
            
            // Add to dictionary with phoneNumber as key
            _members.Add(user.PhoneNumber, user);
            
            // REVERSE CONNECTION: Tell user about this chat
            // Use Internal to avoid infinite recursion
            user.AddChatInternal(this);
        }

        public void RemoveMember(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Phone number cannot be empty.");

            if (!_members.ContainsKey(phoneNumber))
                throw new InvalidOperationException($"No user with phone number {phoneNumber} found in this chat.");

            // Get user before removing (need for reverse connection)
            User user = _members[phoneNumber];
            _members.Remove(phoneNumber);
            
            // REVERSE CONNECTION: Tell user to forget this chat
            user.RemoveChatInternal(this);
        }

        // QUALIFIED LOOKUP
        // lookup by phone number instead of loop through all users
        public User? GetMemberByPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return null;

            return _members.ContainsKey(phoneNumber) ? _members[phoneNumber] : null;
        }
        
        // When user changes phone number, we must update the Dictionary key!
        // Called by User.PhoneNumber setter
        public void UpdateMemberPhoneNumber(string oldPhoneNumber, string newPhoneNumber)
        {
            if (!_members.ContainsKey(oldPhoneNumber))
                throw new InvalidOperationException($"No user with phone number {oldPhoneNumber} found in this chat.");

            // Prevent duplicate phone numbers in same chat
            if (_members.ContainsKey(newPhoneNumber) && oldPhoneNumber != newPhoneNumber)
                throw new InvalidOperationException($"A user with phone number {newPhoneNumber} already exists in this chat.");

            // Update dictionary: remove old key, add new key with same user
            User user = _members[oldPhoneNumber];
            _members.Remove(oldPhoneNumber);
            _members.Add(newPhoneNumber, user);
        }

        // INTERNAL METHOD: Called by User.JoinChat()
        // No validation - already done in User.JoinChat()
        // No reverse connection - would cause infinite loop!
        internal void AddMemberInternal(User user)
        {
            // Only add if not already there (defensive programming)
            if (!_members.ContainsKey(user.PhoneNumber))
            {
                _members.Add(user.PhoneNumber, user);
            }
        }

        // INTERNAL METHOD: Called by User.LeaveChat()
        internal void RemoveMemberInternal(string phoneNumber)
        {
            _members.Remove(phoneNumber);
        }

        // ============================================================
        // BASIC ASSOCIATION: Chat → Message (1 chat → 0..* messages)
        // Chat maintains history of all messages sent to it
        // ============================================================
        
        private readonly List<Message> _history = new();
        public IReadOnlyList<Message> History => _history.AsReadOnly();

        // INTERNAL: Called by Message constructor
        // Message creates association when it's sent to a chat
        public void AddMessage(Message message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            if (!_history.Contains(message))
            {
                _history.Add(message);
                
                // REVERSE CONNECTION
                if (message.TargetChat != this)
                {
                    message.TargetChat = this;
                }
            }
        }

        public void RemoveMessage(Message message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            if (_history.Contains(message))
            {
                _history.Remove(message);
                
                // REVERSE CONNECTION
                if (message.TargetChat == this)
                {
                    message.TargetChat = null;
                }
            }
        }
    }
}