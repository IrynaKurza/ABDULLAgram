using ABDULLAgram.Chats;
using ABDULLAgram.Support;
using ABDULLAgram.Attachments;
using ABDULLAgram.Messages;
using System.Xml.Serialization;

namespace ABDULLAgram.Users
{
    [XmlInclude(typeof(Regular))]
    [XmlInclude(typeof(Premium))]
    [Serializable]
    public abstract class User
    {
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
        public virtual string PhoneNumber
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
            user._blockedBy.Add(this);  // Direct access because same class
        }

        public void UnblockUser(User user)
        {
            if (!_blockedUsers.Contains(user)) return;
            
            // Remove from both directions
            _blockedUsers.Remove(user);
            user._blockedBy.Remove(this);  // Direct access because same class
        }

        public IReadOnlyCollection<User> GetBlockedUsers() => _blockedUsers.AsReadOnly();
        public IReadOnlyCollection<User> GetBlockedBy() => _blockedBy.AsReadOnly();

        // ============================================================
        // QUALIFIED ASSOCIATION: User ↔ Chat (qualified by phoneNumber)
        // Reverse connection - User knows which Chats they're in
        // ============================================================
        
        private HashSet<Chat> _joinedChats = new HashSet<Chat>();
        
        // ReadOnly prevents external code from modifying the collection directly
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
        // Regular users: max 10 packs, Premium users: unlimited
        // ============================================================
        
        private HashSet<Stickerpack> _savedStickerpacks = new();
        public IReadOnlyCollection<Stickerpack> SavedStickerpacks => _savedStickerpacks.ToList().AsReadOnly();

        // Abstract property - each subclass (Regular/Premium) defines their own limit
        public abstract int MaxSavedStickerpacks { get; }

        // PUBLIC METHOD: Use this to save a stickerpack
        // Virtual allows subclasses to override if needed
        public virtual void SaveStickerpack(Stickerpack pack)
        {
            if (pack == null)
                throw new ArgumentNullException(nameof(pack), "Stickerpack cannot be null.");

            if (_savedStickerpacks.Contains(pack))
                throw new InvalidOperationException("This stickerpack is already saved.");

            // Business rule enforcement: check max limit
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
        // Folders are destroyed when User is destroyed
        // Strong ownership: Folder cannot exist without User
        // ============================================================
        
        private readonly HashSet<Folder> _folders = new();
        public IReadOnlyCollection<Folder> Folders => _folders.ToList().AsReadOnly();

        // INTERNAL: Called by Folder constructor
        // Establishes reverse connection when folder is created
        internal void AddFolderInternal(Folder folder)
        {
            if (folder == null) throw new ArgumentNullException(nameof(folder));
            _folders.Add(folder);
        }

        // INTERNAL: Called by Folder.Delete()
        // Just removes from collection - extent removal handled in Folder
        internal void RemoveFolderInternal(Folder folder)
        {
            if (folder == null) throw new ArgumentNullException(nameof(folder));
            _folders.Remove(folder);
        }

        // Factory method pattern: User creates its own folders
        // Ensures folder is always created with an owner
        public Folder CreateFolder(string name)
        {
            return new Folder(this, name);
        }

        // Delete single folder - calls folder's Delete method
        // Folder.Delete() removes from extent AND from user's collection
        public void DeleteFolder(Folder folder)
        {
            if (folder == null) throw new ArgumentNullException(nameof(folder));
            if (!_folders.Contains(folder))
                throw new InvalidOperationException("Folder does not belong to this user.");

            // Folder.Delete() will call RemoveFolderInternal and remove from extent
            folder.Delete();
        }

        // COMPOSITION: When user is deleted, all folders must be deleted
        // Each folder is removed from extent (true deletion)
        public void DeleteAllFolders()
        {
            // ToList() creates copy to avoid collection modification during iteration
            foreach (var folder in _folders.ToList())
            {
                folder.Delete(); // Removes from extent AND user's collection
            }
        }

        // DELETE USER: Removes user from extent and deletes all owned folders
        // This is the proper way to delete a user (composition cleanup)
        public void DeleteUser()
        {
            // First, delete all owned folders (composition rule)
            DeleteAllFolders();
            
            // Then remove user from their extent
            if (this is Regular)
            {
                Regular.RemoveFromExtent((Regular)this);
            }
            else if (this is Premium)
            {
                Premium.RemoveFromExtent((Premium)this);
            }
            
            // Note: Could also clean up other associations here if needed
            // For now, just handle composition deletion
        }
        

        // ============================================================
        // BASIC ASSOCIATION: User ↔ Text (many-to-many)
        // User can be mentioned in multiple Text messages
        // ============================================================
        
        private readonly HashSet<Text> _mentionedInTexts = new();
        public IReadOnlyCollection<Text> MentionedInTexts => _mentionedInTexts.ToList().AsReadOnly();

        // INTERNAL: Called by Text when adding mention
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
        // User knows about all messages they sent
        // ============================================================
        
        private readonly List<Message> _messages = new();
        public IReadOnlyList<Message> SentMessages => _messages.AsReadOnly();
        
        // INTERNAL: Called by Message constructor
        // Message creates the association when it's created
        public void AddMessage(Message message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            
            if (!_messages.Contains(message))
            {
                _messages.Add(message);
                
                // REVERSE CONNECTION
                if (message.Sender != this)
                {
                    message.Sender = this;
                }
            }
        }
        
        public void RemoveMessage(Message message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            
            if (_messages.Contains(message))
            {
                _messages.Remove(message);
                
                // REVERSE CONNECTION
                if (message.Sender == this)
                {
                    message.Sender = null;
                }
            }
        }
    }
}