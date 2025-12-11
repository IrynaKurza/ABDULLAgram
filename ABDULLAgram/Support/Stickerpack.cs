using System;
using System.Collections.Generic;
using System.Linq;
using ABDULLAgram.Attachments;
using ABDULLAgram.Users;

namespace ABDULLAgram.Support
{
    [Serializable]
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
        // QUALIFIED AGGREGATION: Stickerpack ↔ Sticker (1 pack → 1..50 stickers)
        // Dictionary with emojiCode as QUALIFIER (key)
        // Stickers can move between packs (aggregation)
        // Pack has min 1, max 50 stickers
        // ============================================================
        
        private Dictionary<string, Sticker> _stickers = new Dictionary<string, Sticker>();

        // Add sticker to pack (moves between packs like before)
        public void AddSticker(Sticker sticker)
        {
            if (sticker == null)
                throw new ArgumentNullException(nameof(sticker));

            // Business rule: max 50 stickers per pack
            if (_stickers.Count >= 50)
                throw new InvalidOperationException("Stickerpack cannot have more than 50 stickers.");
            
            // If already in this pack, do nothing
            if (_stickers.ContainsKey(sticker.EmojiCode))
                return;
            
            // AGGREGATION FEATURE: Sticker can move between packs
            // If sticker belongs to another pack, remove it first
            if (sticker.BelongsToPack != null)
            {
                sticker.BelongsToPack.RemoveSticker(sticker.EmojiCode);
            }
            
            // Add to dictionary with emojiCode as key
            _stickers.Add(sticker.EmojiCode, sticker);
            sticker.BelongsToPack = this;
        }

        // Remove sticker by emojiCode (qualified)
        public void RemoveSticker(string emojiCode)
        {
            if (string.IsNullOrWhiteSpace(emojiCode))
                return;

            if (!_stickers.ContainsKey(emojiCode))
                return;
            
            // Business rule: pack must have at least 1 sticker
            if (_stickers.Count <= 1)
                throw new InvalidOperationException("Stickerpack must have at least 1 sticker.");
            
            var sticker = _stickers[emojiCode];
            _stickers.Remove(emojiCode);
            sticker.RemoveFromPack();
        }

        // QUALIFIED LOOKUP: O(1) lookup by emojiCode
        public Sticker? GetStickerByEmojiCode(string emojiCode)
        {
            if (string.IsNullOrWhiteSpace(emojiCode))
                return null;

            return _stickers.ContainsKey(emojiCode) ? _stickers[emojiCode] : null;
        }

        // Get all stickers as collection
        public IReadOnlyCollection<Sticker> GetStickers() => _stickers.Values.ToList().AsReadOnly();

        // ============================================================
        // BASIC ASSOCIATION: Stickerpack ↔ User (many-to-many)
        // Multiple users can save the same pack
        // One user can save multiple packs
        // ============================================================
        
        private HashSet<User> _savedByUsers = new();
        public IReadOnlyCollection<User> SavedByUsers => _savedByUsers.ToList().AsReadOnly();

        // PUBLIC METHOD: Use this to add a user from Stickerpack side
        public void AddSavedByUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User cannot be null.");

            if (_savedByUsers.Contains(user))
                throw new InvalidOperationException("This user has already saved this stickerpack.");

            _savedByUsers.Add(user);
            
            // REVERSE CONNECTION: Tell user about this pack
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