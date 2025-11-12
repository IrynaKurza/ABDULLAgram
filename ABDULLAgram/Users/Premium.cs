namespace ABDULLAgram.Users
{
    [Serializable]
    public class Premium : User
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
               
                bool exists = _extent.Any(p => !ReferenceEquals(p, this) && p.PhoneNumber == value);
                if (exists)
                    throw new InvalidOperationException("PhoneNumber must be unique among Premium users.");
                
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

        // Premium attributes
        private DateTime _premiumStartDate;
        public DateTime PremiumStartDate
        {
            get => _premiumStartDate;
            set
            {
                if (value > DateTime.Now)
                    throw new ArgumentOutOfRangeException(nameof(PremiumStartDate), "PremiumStartDate cannot be in the future.");
                if (_premiumEndDate != default && value >= _premiumEndDate)
                    throw new ArgumentOutOfRangeException(nameof(PremiumStartDate), "PremiumStartDate must be before PremiumEndDate.");
                _premiumStartDate = value;
            }
        }

        private DateTime _premiumEndDate;
        public DateTime PremiumEndDate
        {
            get => _premiumEndDate;
            set
            {
                if (_premiumStartDate != default && value <= _premiumStartDate)
                    throw new ArgumentOutOfRangeException(nameof(PremiumEndDate), "PremiumEndDate must be after PremiumStartDate.");
                _premiumEndDate = value;
            }
        }

        // Derived attribute (no setter)
        public int RemainingDays => (_premiumEndDate - DateTime.Now).Days;

        // Class Extent
        private static readonly List<Premium> _extent = new();
        public static IReadOnlyCollection<Premium> GetAll() => _extent.AsReadOnly();

        private void AddToExtent() => _extent.Add(this);
        public static void ClearExtent() => _extent.Clear();
        public static void ReAdd(Premium p)
        {
            if (_extent.Any(x => x.PhoneNumber == p.PhoneNumber && !ReferenceEquals(x, p)))
                throw new InvalidOperationException("Duplicate PhoneNumber found during Load.");
            _extent.Add(p);
        }

        // Constructors
        public Premium(string username, string phoneNumber, bool isOnline, DateTime premiumStartDate, DateTime premiumEndDate)
        {
            Username = username;
            PhoneNumber = phoneNumber;
            IsOnline = isOnline;
            PremiumStartDate = premiumStartDate;
            PremiumEndDate = premiumEndDate;

            AddToExtent();
        }

        private Premium() {} // For XML serialization
    }
}