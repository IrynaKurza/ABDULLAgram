using ABDULLAgram.Support;

namespace ABDULLAgram.Attachments
{
    [Serializable]
    public class Sticker : Messages.Message
    {
        public enum BackgroundTypeEnum { Transparent, Filled }

        // Sticker-specific attributes
        public BackgroundTypeEnum BackgroundType { get; set; }

        // Aggregation reverse link
        [System.Xml.Serialization.XmlIgnore]
        public Stickerpack? BelongsToPack { get; internal set; }

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
        public Sticker(BackgroundTypeEnum backgroundType)
        {
            BackgroundType = backgroundType;
            SetSize(0);

            AddToExtent();
        }

        private Sticker() { } // for XML serialization
    }
}