using ABDULLAgram.Support;  // ← ADD THIS LINE

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

        // ============================================================
        // BASIC ASSOCIATION: User ↔ Stickerpack
        // ============================================================
        
        // Collection of saved stickerpacks
        private HashSet<Stickerpack> _savedStickerpacks = new HashSet<Stickerpack>();
        public IReadOnlyCollection<Stickerpack> SavedStickerpacks => _savedStickerpacks.ToList().AsReadOnly();

        // Abstract property - each subclass (Regular/Premium) defines max packs
        public abstract int MaxSavedStickerpacks { get; }

        // PUBLIC METHOD: Save a stickerpack
        public virtual void SaveStickerpack(Stickerpack pack)
        {
            if (pack == null)
                throw new ArgumentNullException(nameof(pack), "Stickerpack cannot be null.");

            if (_savedStickerpacks.Contains(pack))
                throw new InvalidOperationException("This stickerpack is already saved.");

            // Check max limit (different for Regular vs Premium)
            if (_savedStickerpacks.Count >= MaxSavedStickerpacks)
                throw new InvalidOperationException($"Cannot save more than {MaxSavedStickerpacks} stickerpacks.");

            // Add to our collection
            _savedStickerpacks.Add(pack);

            // REVERSE CONNECTION: Tell pack about us
            pack.AddSavedByUserInternal(this);
        }

        // PUBLIC METHOD: Unsave a stickerpack
        public void UnsaveStickerpack(Stickerpack pack)
        {
            if (pack == null)
                throw new ArgumentNullException(nameof(pack), "Stickerpack cannot be null.");

            if (!_savedStickerpacks.Contains(pack))
                throw new InvalidOperationException("This stickerpack is not saved.");

            // Remove from our collection
            _savedStickerpacks.Remove(pack);

            // REVERSE CONNECTION: Tell pack to forget us
            pack.RemoveSavedByUserInternal(this);
        }

        // INTERNAL METHODS: Called by Stickerpack class only
        internal void AddStickerpackInternal(Stickerpack pack)
        {
            // No max check here - only in public method
            _savedStickerpacks.Add(pack);
        }

        internal void RemoveStickerpackInternal(Stickerpack pack)
        {
            _savedStickerpacks.Remove(pack);
        }
    }
}