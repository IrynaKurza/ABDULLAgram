using ABDULLAgram.Chats;
using ABDULLAgram.Support;
using ABDULLAgram.Attachments;
using ABDULLAgram.Messages;

namespace ABDULLAgram.Users
{
    [Serializable]
    public class User
    {
        // ============================================================
        // PERSISTENCE SUPPORT (composition reconstruction)
        // ============================================================

        public UserType PersistedUserType { get; set; }

        public int? PersistedAdFrequency { get; set; }
        public DateTime? PersistedPremiumStart { get; set; }
        public DateTime? PersistedPremiumEnd { get; set; }
        
        // ============================================================
        // CLASS EXTENT PATTERN
        // ============================================================

        private static readonly List<User> _extent = new();
        public static IReadOnlyCollection<User> GetAll() => _extent.AsReadOnly();

        private void AddToExtent()
        {
            if (_extent.Any(u => u.PhoneNumber == PhoneNumber))
                throw new InvalidOperationException($"A user with phone number '{PhoneNumber}' already exists.");

            _extent.Add(this);
        }

        public static void ClearExtent() => _extent.Clear();

        public static void ReAdd(User user)
        {
            if (_extent.Any(u => u.PhoneNumber == user.PhoneNumber && !ReferenceEquals(u, user)))
                throw new InvalidOperationException("Duplicate PhoneNumber found during load of Users.");

            _extent.Add(user);
            
            user._behavior = user.PersistedUserType switch
            {
                UserType.Regular => new RegularUserBehavior(user.PersistedAdFrequency!.Value),
                UserType.Premium => new PremiumUserBehavior(
                    user.PersistedPremiumStart!.Value,
                    user.PersistedPremiumEnd!.Value
                ),
                _ => throw new InvalidOperationException()
            };

            user._behavior.ValidateOnAttach(user);

        }

        // ============================================================
        // COMPOSITION-BASED INHERITANCE
        // ============================================================

        private IUserTypeBehavior _behavior;
        public UserType UserType => _behavior.Type;
        public int MaxSavedStickerpacks => _behavior.MaxSavedStickerpacks;

        public User(string username, string phoneNumber, bool isOnline, IUserTypeBehavior behavior)
        {
            Username = username;
            PhoneNumber = phoneNumber;
            IsOnline = isOnline;

            _behavior = behavior ?? throw new ArgumentNullException(nameof(behavior));
            _behavior.ValidateOnAttach(this);

            AddToExtent();
            
            PersistedUserType = _behavior.Type;

            if (_behavior is RegularUserBehavior r)
            {
                PersistedAdFrequency = r.AdFrequency;
            }
            else if (_behavior is PremiumUserBehavior p)
            {
                PersistedPremiumStart = p.PremiumStartDate;
                PersistedPremiumEnd = p.PremiumEndDate;
            }

        }

        public void UpgradeToPremium(DateTime start, DateTime end)
        {
            if (_behavior is PremiumUserBehavior)
                throw new InvalidOperationException("User is already Premium.");

            _behavior = new PremiumUserBehavior(start, end);

            PersistedUserType = UserType.Premium;
            PersistedPremiumStart = start;
            PersistedPremiumEnd = end;
            PersistedAdFrequency = null;
        }

        public RegularUserBehavior AsRegular()
        {
            if (_behavior is not RegularUserBehavior regular)
                throw new InvalidOperationException("User is not Regular.");

            return regular;
        }

        public PremiumUserBehavior AsPremium()
        {
            if (_behavior is not PremiumUserBehavior premium)
                throw new InvalidOperationException("User is not Premium.");

            return premium;
        }

        // ============================================================
        // BASIC ATTRIBUTES
        // ============================================================

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
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("PhoneNumber cannot be empty.");

                // CRITICAL: When phone number changes, update it in ALL chats
                // Because Chat uses phoneNumber as Dictionary key (qualified association)
                if (!string.IsNullOrWhiteSpace(_phoneNumber) && _phoneNumber != value)
                {
                    foreach (var chat in _joinedChats.ToList())
                    {
                        chat.UpdateMemberPhoneNumber(_phoneNumber, value);
                    }
                }

                _phoneNumber = value;
            }
        }

        private DateTime? _lastSeenAt;
        public DateTime? LastSeenAt
        {
            get => _lastSeenAt;
            set
            {
                if (value is { } dt && dt > DateTime.Now)
                    throw new ArgumentOutOfRangeException(nameof(LastSeenAt), "LastSeenAt cannot be in the future.");
                _lastSeenAt = value;
            }
        }

        public bool IsOnline { get; set; }

        // ============================================================
        // REFLEX ASSOCIATION: User ↔ User (Block/Unblock)
        // ============================================================

        private List<User> _blockedUsers = new();
        private List<User> _blockedBy = new();

        public void BlockUser(User user)
        {
            if (user == this)
                throw new InvalidOperationException("Cannot block yourself.");
            if (_blockedUsers.Contains(user)) return;

            // Add to both directions (reflex association = same class on both sides)
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

        // ============================================================
        // QUALIFIED ASSOCIATION: User ↔ Chat (qualified by phoneNumber)
        // Reverse connection - User knows which Chats they're in
        // ============================================================

        private HashSet<Chat> _joinedChats = new HashSet<Chat>();
        public IReadOnlyCollection<Chat> JoinedChats => _joinedChats.ToList().AsReadOnly();

        // PUBLIC METHOD: Use this to join a chat from User side
        // Validates, updates User's collection, then calls Chat's internal method
        public void JoinChat(Chat chat)
        {
            if (chat == null)
                throw new ArgumentNullException(nameof(chat), "Chat cannot be null.");

            if (_joinedChats.Contains(chat))
                throw new InvalidOperationException("User is already a member of this chat.");

            _joinedChats.Add(chat);

            // REVERSE CONNECTION: Tell the chat about this user
            // Use Internal method to avoid infinite recursion
            chat.AddMemberInternal(this);
        }

        public void LeaveChat(Chat chat)
        {
            if (chat == null)
                throw new ArgumentNullException(nameof(chat), "Chat cannot be null.");

            if (!_joinedChats.Contains(chat))
                throw new InvalidOperationException("User is not a member of this chat.");

            _joinedChats.Remove(chat);

            // REVERSE CONNECTION: Tell the chat to remove this user
            chat.RemoveMemberInternal(PhoneNumber);
        }

        // INTERNAL METHOD: Called by Chat.AddMember()
        // No validation - already done in Chat.AddMember()
        // No reverse connection - would cause infinite loop!
        // Only updates this user's collection
        internal void AddChatInternal(Chat chat)
        {
            _joinedChats.Add(chat);
        }

        // INTERNAL METHOD: Called by Chat.RemoveMember()
        // Just removes from collection, no callbacks
        internal void RemoveChatInternal(Chat chat)
        {
            _joinedChats.Remove(chat);
        }

        // ============================================================
        // BASIC ASSOCIATION: User ↔ Stickerpack (many-to-many)
        // ============================================================

        private HashSet<Stickerpack> _savedStickerpacks = new();
        public IReadOnlyCollection<Stickerpack> SavedStickerpacks => _savedStickerpacks.ToList().AsReadOnly();

        // PUBLIC METHOD: Use this to save a stickerpack
        public virtual void SaveStickerpack(Stickerpack pack)
        {
            if (pack == null)
                throw new ArgumentNullException(nameof(pack), "Stickerpack cannot be null.");

            if (_savedStickerpacks.Contains(pack))
                throw new InvalidOperationException("This stickerpack is already saved.");

            // Regular = 10, Premium = unlimited (int.MaxValue)
            if (_savedStickerpacks.Count >= MaxSavedStickerpacks)
                throw new InvalidOperationException($"Cannot save more than {MaxSavedStickerpacks} stickerpacks.");

            _savedStickerpacks.Add(pack);

            // REVERSE CONNECTION: Tell pack about this user
            pack.AddSavedByUserInternal(this);
        }

        public void UnsaveStickerpack(Stickerpack pack)
        {
            if (pack == null)
                throw new ArgumentNullException(nameof(pack), "Stickerpack cannot be null.");

            if (!_savedStickerpacks.Contains(pack))
                throw new InvalidOperationException("This stickerpack is not saved.");

            _savedStickerpacks.Remove(pack);

            // REVERSE CONNECTION: Tell pack to remove this user
            pack.RemoveSavedByUserInternal(this);
        }

        // INTERNAL METHOD: Called by Stickerpack.AddSavedByUser()
        // No max limit check here! Already checked in public method
        // This prevents duplicate validation
        internal void AddStickerpackInternal(Stickerpack pack)
        {
            _savedStickerpacks.Add(pack);
        }

        // INTERNAL METHOD: Called by Stickerpack.RemoveSavedByUser()
        internal void RemoveStickerpackInternal(Stickerpack pack)
        {
            _savedStickerpacks.Remove(pack);
        }

        // ============================================================
        // COMPOSITION: User owns Folders (1 user → 0..* folders)
        // ============================================================

        private readonly HashSet<Folder> _folders = new();
        public IReadOnlyCollection<Folder> Folders => _folders.ToList().AsReadOnly();

        internal void AddFolderInternal(Folder folder)
        {
            if (folder == null) throw new ArgumentNullException(nameof(folder));
            _folders.Add(folder);
        }

        internal void RemoveFolderInternal(Folder folder)
        {
            if (folder == null) throw new ArgumentNullException(nameof(folder));
            _folders.Remove(folder);
        }

        public Folder CreateFolder(string name)
        {
            return new Folder(this, name);
        }

        public void DeleteFolder(Folder folder)
        {
            if (folder == null) throw new ArgumentNullException(nameof(folder));
            if (!_folders.Contains(folder))
                throw new InvalidOperationException("Folder does not belong to this user.");

            folder.Delete();
        }

        public void DeleteAllFolders()
        {
            foreach (var folder in _folders.ToList())
            {
                folder.Delete();
            }
        }

        public void DeleteUser()
        {
            foreach (var g in _adminOfGroups.ToList())
                g.SetAdmin(null!); 

            foreach (var p in _managedStickerpacks.ToList())
                p.SetManager(null!);

            foreach (var s in _savedStickerpacks.ToList())
                UnsaveStickerpack(s);

            DeleteAllFolders();
            _extent.Remove(this);
        }

        // ============================================================
        // BASIC ASSOCIATION: User ↔ Text (many-to-many)
        // ============================================================

        private readonly HashSet<Text> _mentionedInTexts = new();
        public IReadOnlyCollection<Text> MentionedInTexts => _mentionedInTexts.ToList().AsReadOnly();

        internal void AddMentionedInText(Text text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            _mentionedInTexts.Add(text);
        }

        internal void RemoveMentionedInText(Text text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            _mentionedInTexts.Remove(text);
        }

        // ============================================================
        // BASIC ASSOCIATION: User → Message (1 user → 0..* messages)
        // ============================================================

        private readonly List<Message> _messages = new();
        public IReadOnlyList<Message> SentMessages => _messages.AsReadOnly();

        internal void AddMessageInternal(Message message)
        {
            _messages.Add(message);
        }

        internal void RemoveMessageInternal(Message message)
        {
            _messages.Remove(message);
        }

        public void AddMessage(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (_messages.Contains(message))
                return;

            _messages.Add(message);

            if (message.Sender != this)
            {
                message.SetSenderInternal(this);
            }
        }

        public void RemoveMessage(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (_messages.Contains(message))
            {
                _messages.Remove(message);

                if (message.Sender == this)
                {
                    message.ClearSenderInternal();
                }
            }
        }

        // ============================================================
        // BASIC ASSOCIATION (reverse): User — admin of — Group
        // ============================================================

        private readonly HashSet<Chat> _adminOfGroups = new();
        public IReadOnlyCollection<Chat> AdminOfGroups => _adminOfGroups.ToList().AsReadOnly();

        internal void AddAdminOfGroupInternal(Chat chat)
        {
            _adminOfGroups.Add(chat);
        }

        internal void RemoveAdminOfGroupInternal(Chat chat)
        {
            _adminOfGroups.Remove(chat);
        }

        // ============================================================
        // BASIC ASSOCIATION (reverse): User — manages — Stickerpack
        // ============================================================

        private readonly HashSet<Stickerpack> _managedStickerpacks = new();
        public IReadOnlyCollection<Stickerpack> ManagedStickerpacks =>
            _managedStickerpacks.ToList().AsReadOnly();

        internal void AddManagedStickerpackInternal(Stickerpack pack)
        {
            _managedStickerpacks.Add(pack);
        }

        internal void RemoveManagedStickerpackInternal(Stickerpack pack)
        {
            _managedStickerpacks.Remove(pack);
        }

        // ============================================================
        // BASIC ASSOCIATION (reverse): User — read — Sent
        // ============================================================

        private readonly HashSet<Sent> _readMessages = new();
        public IReadOnlyCollection<Sent> ReadMessages =>
            _readMessages.ToList().AsReadOnly();

        internal void AddReadMessageInternal(Sent message)
        {
            _readMessages.Add(message);
        }

        internal void RemoveReadMessageInternal(Sent message)
        {
            _readMessages.Remove(message);
        }
    }
}
