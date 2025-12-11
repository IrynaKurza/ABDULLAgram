using ABDULLAgram.Support;
using ABDULLAgram.Chats;
using ABDULLAgram.Users;

namespace ABDULLAgram.Attachments
{
    [Serializable]
    public class Sticker : Messages.Message
    {
        public enum BackgroundTypeEnum { Transparent, Filled }

        // Sticker-specific attributes
        public BackgroundTypeEnum BackgroundType { get; set; }

        // ============================================================
        // AGGREGATION: Sticker ↔ Stickerpack (reverse connection)
        // Setter handles both directions to ensure consistency
        // ============================================================
        
        private Stickerpack? _belongsToPack;
        
        [System.Xml.Serialization.XmlIgnore]
        public Stickerpack? BelongsToPack
        {
            get => _belongsToPack;
            internal set
            {
                if (_belongsToPack == value)
                    return;

                // Remove from old pack (reverse connection)
                _belongsToPack?.RemoveStickerInternal(this);
                
                _belongsToPack = value;
                
                // Add to new pack (reverse connection)
                _belongsToPack?.AddStickerInternal(this);
            }
        }

        // Override Id to add uniqueness check (like Regular does with PhoneNumber)
        public override string Id
        {
            get => base.Id;
            set
            {
                bool exists = _extent.Any(s => !ReferenceEquals(s, this) && s.Id == value);
                if (exists)
                    throw new InvalidOperationException("Sticker Id must be unique among all Sticker messages.");
                
                base.Id = value; // Calls parent validation
            }
        }

        // Class Extent
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

        // Constructors
        public Sticker(User sender, Chat chat, BackgroundTypeEnum backgroundType)
            : base(sender, chat)
        {
            BackgroundType = backgroundType;
            SetSize(0);

            AddToExtent();
        }
        
        // If we dont want to specify who sent this sticker. For example, when we just add it to StickerPack
        public Sticker(BackgroundTypeEnum backgroundType)
        {
            BackgroundType = backgroundType;
            SetSize(0);

            AddToExtent();
        }

        private Sticker() { } // for XML serialization
        protected override void RemoveFromExtent()
        {
            _extent.Remove(this);
        }
    }
}