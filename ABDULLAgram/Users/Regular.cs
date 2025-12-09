namespace ABDULLAgram.Users
{
    [Serializable]
    public class Regular : User
    {
        // User attributes override
        public sealed override string PhoneNumber
        {
            get => base.PhoneNumber;
            set
            {
                // First call base to validate "not empty"
                // Then check uniqueness in this extent
                bool exists = Extent.Any(r => !ReferenceEquals(r, this) && r.PhoneNumber == value);
                if (exists)
                    throw new InvalidOperationException("PhoneNumber must be unique among Regular users.");
                
                base.PhoneNumber = value;
            }
        }

        private int _adFrequency;
        public int AdFrequency
        {
            get => _adFrequency;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(AdFrequency), "AdFrequency cannot be negative.");
                _adFrequency = value;
            }
        }

        // Regular attributes
        private static int _maxStickerPacksSaved = 10;
        public static int MaxStickerPacksSaved
        {
            get => _maxStickerPacksSaved;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(MaxStickerPacksSaved), "MaxStickerPacksSaved cannot be negative.");
                _maxStickerPacksSaved = value;
            }
        }

        // Override abstract property from User - Regular users limited to 10 packs
        public override int MaxSavedStickerpacks => MaxStickerPacksSaved;

        // Derived attribute
        public string Status => IsOnline ? "Online" : "Offline";
        
        // Class Extent
        private static readonly List<Regular> Extent = new();
        public static IReadOnlyCollection<Regular> GetAll() => Extent.AsReadOnly();

        private void AddToExtent() => Extent.Add(this);
        public static void ClearExtent() => Extent.Clear();
        public static void ReAdd(Regular r)
        {
            if (Extent.Any(x => x.PhoneNumber == r.PhoneNumber && !ReferenceEquals(x, r)))
                throw new InvalidOperationException("Duplicate PhoneNumber found during Load.");
            Extent.Add(r);
        }

        // Constructors
        public Regular(string username, string phoneNumber, bool isOnline, int adFrequency)
        {
            Username = username;
            PhoneNumber = phoneNumber;
            IsOnline = isOnline;
            AdFrequency = adFrequency;

            AddToExtent();
        }

        private Regular() {} // For XML serialization
    }
}