namespace ABDULLAgram.Users
{
    [Serializable]
    public class Regular : User
    {
        
        // User attributes
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
               
                bool exists = _extent.Any(r => !ReferenceEquals(r, this) && r.PhoneNumber == value);
                if (exists)
                    throw new InvalidOperationException("PhoneNumber must be unique among Regular users.");
                
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

        // Derived attribute (no setter)
        public string Status => IsOnline ? "Online" : "Offline";
        
        
        // Class Extent
        private static readonly List<Regular> _extent = new();
        public static IReadOnlyCollection<Regular> GetAll() => _extent.AsReadOnly();

        private void AddToExtent() => _extent.Add(this);
        public static void ClearExtent() => _extent.Clear();
        public static void ReAdd(Regular r)
        {
            if (_extent.Any(x => x.PhoneNumber == r.PhoneNumber && !ReferenceEquals(x, r)))
                throw new InvalidOperationException("Duplicate PhoneNumber found during Load.");
            _extent.Add(r);
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