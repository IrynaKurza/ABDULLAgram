using ABDULLAgram.Users;
using System.Xml.Serialization;
using ABDULLAgram.Messages;
using ABDULLAgram.Support;

namespace ABDULLAgram.Chats
{
    [Serializable]
    public class Chat
    {
        // ============================================================
        // ASSIGNMENT 7: INHERITANCE VIA COMPOSITION
        // Private inner classes hold type-specific data
        // Discriminator enum determines which inner class is active
        // ============================================================
        
        private class GroupData
        {
            private int _maxParticipants = 100;
            public int MaxParticipants
            {
                get => _maxParticipants;
                set
                {
                    if (value <= 0)
                        throw new ArgumentOutOfRangeException(nameof(MaxParticipants), 
                            "MaxParticipants must be greater than zero.");
                    _maxParticipants = value;
                }
            }

            private string _description = "";
            public string Description
            {
                get => _description;
                set
                {
                    if (value is null)
                        throw new ArgumentNullException(nameof(Description), 
                            "Description cannot be null.");
                    _description = value;
                }
            }
        }
        
        private class PrivateData
        {
            public int MaxParticipants => 2;
        }

        // Discriminator field - determines which inner class is used
        public ChatType ChatType { get; set; }

        private GroupData? _groupData;
        private PrivateData? _privateData;

        // Constructor takes ChatType and creates appropriate inner class
        public Chat(ChatType chatType)
        {
            ChatType = chatType;
            _createdAt = DateTime.Now;

            switch (chatType)
            {
                case ChatType.Group:
                    _groupData = new GroupData();
                    _privateData = null;
                    break;
                case ChatType.Private:
                    _privateData = new PrivateData();
                    _groupData = null;
                    break;
                default:
                    throw new ArgumentException($"Unknown ChatType: {chatType}");
            }
        }

        // Parameterless constructor for XML serialization
        public Chat()
        {
            _createdAt = DateTime.Now;
            ChatType = ChatType.Group; // Default for XML
            _groupData = new GroupData();
        }

        // Properties delegate to appropriate inner class
        public int MaxParticipants
        {
            get
            {
                return ChatType switch
                {
                    ChatType.Group => _groupData!.MaxParticipants,
                    ChatType.Private => _privateData!.MaxParticipants,
                    _ => throw new InvalidOperationException("Invalid chat type")
                };
            }
            set
            {
                switch (ChatType)
                {
                    case ChatType.Group:
                        _groupData!.MaxParticipants = value;
                        break;
                    case ChatType.Private:
                        if (value != 2)
                            throw new ArgumentOutOfRangeException(nameof(MaxParticipants),
                                "Private chats must have exactly 2 participants.");
                        break;
                    default:
                        throw new InvalidOperationException("Invalid chat type");
                }
            }
        }
        
        public string Description
        {
            get
            {
                if (ChatType != ChatType.Group)
                    throw new InvalidOperationException("Description is only available for Group chats.");
                return _groupData!.Description;
            }
            set
            {
                if (ChatType != ChatType.Group)
                    throw new InvalidOperationException("Description is only available for Group chats.");
                _groupData!.Description = value;
            }
        }

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

        // QUALIFIED LOOKUP: This is the key feature of qualified associations!
        // O(1) lookup by phone number instead of O(n) loop through all users
        public User? GetMemberByPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return null;

            return _members.ContainsKey(phoneNumber) ? _members[phoneNumber] : null;
        }

        // SPECIAL METHOD for qualified associations:
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
        
                // REVERSE CONNECTION - use internal method instead
                message.ClearTargetChatInternal();
            }
        }

        // INTERNAL: Called by Message.Sender setter and Delete
        internal void AddMessageInternal(Message message)
        {
            if (!_history.Contains(message))
            {
                _history.Add(message);
            }
        }

        internal void RemoveMessageInternal(Message message)
        {
            _history.Remove(message);
        }

        // ============================================================
        // AGGREGATION: Chat ↔ Folder (0..* chats ↔ 0..* folders)
        // ============================================================
        
        private readonly HashSet<Folder> _folders = new();
        public IReadOnlyCollection<Folder> Folders => _folders.ToList().AsReadOnly();

        internal void AddToFolderInternal(Folder folder)
        {
            _folders.Add(folder);
        }

        internal void RemoveFromFolderInternal(Folder folder)
        {
            _folders.Remove(folder);
        }

        // ============================================================
        // BASIC ASSOCIATION: Group (0..*) — admin — User (1)
        // Only valid for Group chats
        // ============================================================
        
        private User? _admin;
        
        public User Admin
        {
            get
            {
                if (ChatType != ChatType.Group)
                    throw new InvalidOperationException("Only Group chats have admins.");
                return _admin ?? throw new InvalidOperationException("Group must have an admin.");
            }
        }

        public void SetAdmin(User user)
        {
            if (ChatType != ChatType.Group)
                throw new InvalidOperationException("Only Group chats can have admins.");

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (_admin == user)
                return;

            // Remove reverse connection from old admin
            _admin?.RemoveAdminOfGroupInternal(this);

            _admin = user;

            // Create reverse connection (internal)
            user.AddAdminOfGroupInternal(this);
        }

        public void KickMember(User requester, string phoneNumber)
        {
            if (ChatType != ChatType.Group)
                throw new InvalidOperationException("Only Group chats support kicking members.");

            if (requester == null)
                throw new ArgumentNullException(nameof(requester));

            if (_admin != requester)
                throw new InvalidOperationException("Only admin can kick members.");

            RemoveMember(phoneNumber);
        }
    }
}