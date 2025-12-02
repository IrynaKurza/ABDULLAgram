using ABDULLAgram.Chats;
using ABDULLAgram.Support;

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

                // If phone number is changing, update it in all chats
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
                if (value is DateTime dt && dt > DateTime.Now)
                    throw new ArgumentOutOfRangeException(nameof(LastSeenAt), "LastSeenAt cannot be in the future.");
                _lastSeenAt = value;
            }
        }

        public bool IsOnline { get; set; }

        // ============================================================
        // QUALIFIED ASSOCIATION: User ↔ Chat (reverse connection)
        // ============================================================
        
        private HashSet<Chat> _joinedChats = new HashSet<Chat>();
        public IReadOnlyCollection<Chat> JoinedChats => _joinedChats.ToList().AsReadOnly();

        public void JoinChat(Chat chat)
        {
            if (chat == null)
                throw new ArgumentNullException(nameof(chat), "Chat cannot be null.");

            if (_joinedChats.Contains(chat))
                throw new InvalidOperationException("User is already a member of this chat.");

            _joinedChats.Add(chat);
            chat.AddMemberInternal(this);
        }

        public void LeaveChat(Chat chat)
        {
            if (chat == null)
                throw new ArgumentNullException(nameof(chat), "Chat cannot be null.");

            if (!_joinedChats.Contains(chat))
                throw new InvalidOperationException("User is not a member of this chat.");

            _joinedChats.Remove(chat);
            chat.RemoveMemberInternal(this.PhoneNumber);
        }

        internal void AddChatInternal(Chat chat)
        {
            _joinedChats.Add(chat);
        }

        internal void RemoveChatInternal(Chat chat)
        {
            _joinedChats.Remove(chat);
        }

        // ============================================================
        // BASIC ASSOCIATION: User ↔ Stickerpack
        // ============================================================
        
        private HashSet<Stickerpack> _savedStickerpacks = new HashSet<Stickerpack>();
        public IReadOnlyCollection<Stickerpack> SavedStickerpacks => _savedStickerpacks.ToList().AsReadOnly();

        public abstract int MaxSavedStickerpacks { get; }

        public virtual void SaveStickerpack(Stickerpack pack)
        {
            if (pack == null)
                throw new ArgumentNullException(nameof(pack), "Stickerpack cannot be null.");

            if (_savedStickerpacks.Contains(pack))
                throw new InvalidOperationException("This stickerpack is already saved.");

            if (_savedStickerpacks.Count >= MaxSavedStickerpacks)
                throw new InvalidOperationException($"Cannot save more than {MaxSavedStickerpacks} stickerpacks.");

            _savedStickerpacks.Add(pack);
            pack.AddSavedByUserInternal(this);
        }

        public void UnsaveStickerpack(Stickerpack pack)
        {
            if (pack == null)
                throw new ArgumentNullException(nameof(pack), "Stickerpack cannot be null.");

            if (!_savedStickerpacks.Contains(pack))
                throw new InvalidOperationException("This stickerpack is not saved.");

            _savedStickerpacks.Remove(pack);
            pack.RemoveSavedByUserInternal(this);
        }

        internal void AddStickerpackInternal(Stickerpack pack)
        {
            _savedStickerpacks.Add(pack);
        }

        internal void RemoveStickerpackInternal(Stickerpack pack)
        {
            _savedStickerpacks.Remove(pack);
        }
    }
}