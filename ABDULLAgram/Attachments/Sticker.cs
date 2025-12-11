using System;
using System.Collections.Generic;
using System.Linq;
using ABDULLAgram.Support;
using ABDULLAgram.Chats;
using ABDULLAgram.Users;

namespace ABDULLAgram.Attachments
{
    [Serializable]
    public class Sticker : Messages.Message
    {
        public enum BackgroundTypeEnum { Transparent, Filled }

        // ============================================================
        // STICKER ATTRIBUTES
        // ============================================================
        
        private Stickerpack _belongsToPack;
        private string _emojiCode = "";

        public BackgroundTypeEnum BackgroundType { get; set; }
        
        // EmojiCode is the QUALIFIER for the qualified aggregation
        public string EmojiCode
        {
            get => _emojiCode;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("EmojiCode cannot be empty.");
                _emojiCode = value;
            }
        }

        // ============================================================
        // AGGREGATION: Sticker → Stickerpack (many-to-one)
        // Stickers can move between packs
        // ============================================================
        
        [System.Xml.Serialization.XmlIgnore]
        public Stickerpack BelongsToPack
        {
            get => _belongsToPack;
            internal set => _belongsToPack = value;
        }

        // ============================================================
        // UNIQUE ID VALIDATION
        // ============================================================
        
        public override string Id
        {
            get => base.Id;
            set
            {
                bool exists = _extent.Any(s => !ReferenceEquals(s, this) && s.Id == value);
                if (exists)
                    throw new InvalidOperationException("Sticker Id must be unique among all Sticker messages.");
                
                base.Id = value;
            }
        }

        // ============================================================
        // CLASS EXTENT PATTERN
        // ============================================================
        
        private static readonly List<Sticker> _extent = new();
        public static IReadOnlyCollection<Sticker> GetAll() => _extent.AsReadOnly();
        private void AddToExtent() => _extent.Add(this);
        public static void ClearExtent() => _extent.Clear();
        public static void ReAdd(Sticker s)
        {
            if (_extent.Any(x => x.Id == s.Id && !ReferenceEquals(x, s)))
                throw new InvalidOperationException("Duplicate Id found during load of Sticker messages.");
            _extent.Add(s);
        }

        // ============================================================
        // CONSTRUCTORS
        // ============================================================
        
        public Sticker(string emojiCode, BackgroundTypeEnum backgroundType)
        {
            if (string.IsNullOrWhiteSpace(emojiCode))
                throw new ArgumentException("EmojiCode cannot be empty.");
                
            _emojiCode = emojiCode;
            BackgroundType = backgroundType;
            SetSize(0);
            AddToExtent();
        }

        // Constructor for sending as message
        public Sticker(User sender, Chat chat, string emojiCode, BackgroundTypeEnum backgroundType)
            : base(sender, chat)
        {
            if (string.IsNullOrWhiteSpace(emojiCode))
                throw new ArgumentException("EmojiCode cannot be empty.");
                
            _emojiCode = emojiCode;
            BackgroundType = backgroundType;
            SetSize(0);
            AddToExtent();
        }

        // Parameterless constructor for XML serialization
        private Sticker() { }

        // ============================================================
        // REMOVE FROM PACK
        // Called when sticker is being removed from pack
        // ============================================================
        
        internal void RemoveFromPack()
        {
            _belongsToPack = null!;
        }

        protected override void RemoveFromExtent()
        {
            _extent.Remove(this);
        }
    }
}