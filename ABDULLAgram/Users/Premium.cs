namespace ABDULLAgram.Users
{
    [Serializable]
    public class Premium : User
    {
        // ============================================================
        // PREMIUM-SPECIFIC ATTRIBUTES
        // ============================================================
        
        private DateTime _premiumStartDate;
        public DateTime PremiumStartDate
        {
            get => _premiumStartDate;
            set
            {
                if (value > DateTime.Now)
                    throw new ArgumentOutOfRangeException(nameof(PremiumStartDate), "Premium start date cannot be in the future.");
                _premiumStartDate = value;
            }
        }

        private DateTime _premiumEndDate;
        public DateTime PremiumEndDate
        {
            get => _premiumEndDate;
            set
            {
                if (value < PremiumStartDate)
                    throw new ArgumentException("Premium end date must be after start date.");
                _premiumEndDate = value;
            }
        }

        // POLYMORPHISM: Premium users have unlimited stickerpacks
        public override int MaxSavedStickerpacks => int.MaxValue;

        // DERIVED ATTRIBUTE: Calculated property
        public int RemainingDays => (PremiumEndDate - DateTime.Now).Days;

        // ============================================================
        // CLASS EXTENT PATTERN
        // ============================================================
        
        private static readonly List<Premium> _extent = new();
        public static IReadOnlyCollection<Premium> GetAll() => _extent.AsReadOnly();
        private void AddToExtent()
        {
            // Check for duplicate phone numbers
            if (_extent.Any(u => u.PhoneNumber == this.PhoneNumber))
                throw new InvalidOperationException($"A user with phone number '{this.PhoneNumber}' already exists.");
                
            _extent.Add(this);
        }
        public static void ClearExtent() => _extent.Clear();
        
        public static void ReAdd(Premium p)
        {
            if (_extent.Any(x => x.PhoneNumber == p.PhoneNumber && !ReferenceEquals(x, p)))
                throw new InvalidOperationException("Duplicate PhoneNumber found during load of Premium users.");
            _extent.Add(p);
        }

        // Called by User.DeleteUser() for composition cleanup
        internal static void RemoveFromExtent(Premium user)
        {
            _extent.Remove(user);
        }

        // ============================================================
        // CONSTRUCTORS
        // ============================================================
        
        public Premium(string username, string phoneNumber, bool isOnline, DateTime premiumStartDate, DateTime premiumEndDate)
        {
            Username = username;
            PhoneNumber = phoneNumber;
            IsOnline = isOnline;
            PremiumStartDate = premiumStartDate;
            PremiumEndDate = premiumEndDate;
            
            AddToExtent(); // Will check for duplicates
        }

        // Parameterless constructor for XML serialization
        private Premium() { }
    }
}