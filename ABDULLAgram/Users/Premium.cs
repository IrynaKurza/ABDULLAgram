namespace ABDULLAgram.Users
{
    [Serializable]
    public class Premium : User
    {
        // User attributes override
        public sealed override string PhoneNumber
        {
            get => base.PhoneNumber;
            set
            {
                bool exists = Extent.Any(p => !ReferenceEquals(p, this) && p.PhoneNumber == value);
                if (exists)
                    throw new InvalidOperationException("PhoneNumber must be unique among Premium users.");
                
                base.PhoneNumber = value;
            }
        }

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

        // Derived attribute
        public int RemainingDays => (_premiumEndDate - DateTime.Now).Days;

        // Override abstract property from User - Premium users have unlimited packs
        public override int MaxSavedStickerpacks => int.MaxValue;

        // Class Extent
        private static readonly List<Premium> Extent = new();
        public static IReadOnlyCollection<Premium> GetAll() => Extent.AsReadOnly();

        private void AddToExtent() => Extent.Add(this);
        public static void ClearExtent() => Extent.Clear();
        public static void ReAdd(Premium p)
        {
            if (Extent.Any(x => x.PhoneNumber == p.PhoneNumber && !ReferenceEquals(x, p)))
                throw new InvalidOperationException("Duplicate PhoneNumber found during Load.");
            Extent.Add(p);
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