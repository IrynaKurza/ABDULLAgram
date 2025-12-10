using ABDULLAgram.Attachments;
using ABDULLAgram.Users;

namespace ABDULLAgram.Support
{
    public class Stickerpack
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
                    throw new ArgumentException("Stickerpack Name cannot be empty.");
                _name = value;
            }
        }

        public bool IsPremium { get; set; }

        // ============================================================
        // AGGREGATION: Stickerpack ↔ Sticker (1 pack → 1..50 stickers)
        // Aggregation vs Composition: Stickers CAN exist independently
        // Pack has min 1, max 50 stickers
        // If pack is deleted, stickers are NOT automatically deleted
        // ============================================================
        
        private List<Sticker> _stickers = new();

        public void AddSticker(Sticker sticker)
        {
            // Business rule: max 50 stickers per pack
            if (_stickers.Count >= 50)
                throw new InvalidOperationException("Stickerpack cannot have more than 50 stickers.");
            
            if (_stickers.Contains(sticker)) return;
            
            // AGGREGATION FEATURE: Sticker can move between packs
            // If sticker belongs to another pack, remove it first
            sticker.BelongsToPack?.RemoveSticker(sticker);
            
            _stickers.Add(sticker);
            sticker.BelongsToPack = this;  // Set reverse reference
        }

        public void RemoveSticker(Sticker sticker)
        {
            if (!_stickers.Contains(sticker)) return;
            
            // Business rule: pack must have at least 1 sticker
            if (_stickers.Count <= 1)
                throw new InvalidOperationException("Stickerpack must have at least 1 sticker.");
            
            _stickers.Remove(sticker);
            sticker.BelongsToPack = null;  // Clear reverse reference
        }

        public IReadOnlyCollection<Sticker> GetStickers() => _stickers.AsReadOnly();

        // ============================================================
        // BASIC ASSOCIATION: Stickerpack ↔ User (many-to-many)
        // Multiple users can save the same pack
        // One user can save multiple packs
        // ============================================================
        
        private HashSet<User> _savedByUsers = new();
        public IReadOnlyCollection<User> SavedByUsers => _savedByUsers.ToList().AsReadOnly();

        // PUBLIC METHOD: Use this to add a user from Stickerpack side
        // Validates, updates pack's collection, creates reverse connection
        public void AddSavedByUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User cannot be null.");

            if (_savedByUsers.Contains(user))
                throw new InvalidOperationException("This user has already saved this stickerpack.");

            _savedByUsers.Add(user);
            
            // REVERSE CONNECTION: Tell user about this pack
            // Use Internal to avoid infinite recursion
            // NOTE: No max limit check here - that's User's responsibility!
            user.AddStickerpackInternal(this);
        }

        public void RemoveSavedByUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User cannot be null.");

            if (!_savedByUsers.Contains(user))
                throw new InvalidOperationException("This user has not saved this stickerpack.");

            _savedByUsers.Remove(user);
            
            // REVERSE CONNECTION: Tell user to forget this pack
            user.RemoveStickerpackInternal(this);
        }

        // INTERNAL METHOD: Called by User.SaveStickerpack()
        // No validation - already done in User.SaveStickerpack()
        // No reverse connection - would cause infinite loop!
        // NOTE: User's max limit is checked in User.SaveStickerpack(), not here
        internal void AddSavedByUserInternal(User user)
        {
            _savedByUsers.Add(user);
        }

        // INTERNAL METHOD: Called by User.UnsaveStickerpack()
        internal void RemoveSavedByUserInternal(User user)
        {
            _savedByUsers.Remove(user);
        }
    }
}