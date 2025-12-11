namespace ABDULLAgram.Users
{
    [Serializable]
    public class Regular : User
    {
        // ============================================================
        // REGULAR-SPECIFIC ATTRIBUTES
        // ============================================================
        
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

        // POLYMORPHISM: Regular users limited to 10 stickerpacks
        public override int MaxSavedStickerpacks => 10;

        // ============================================================
        // CLASS EXTENT PATTERN
        // ============================================================
        
        private static readonly List<Regular> _extent = new();
        public static IReadOnlyCollection<Regular> GetAll() => _extent.AsReadOnly();
        private void AddToExtent()
        {
            // Check for duplicate phone numbers
            if (_extent.Any(u => u.PhoneNumber == this.PhoneNumber))
                throw new InvalidOperationException($"A user with phone number '{this.PhoneNumber}' already exists.");
                
            _extent.Add(this);
        }
        public static void ClearExtent() => _extent.Clear();
        
        public static void ReAdd(Regular r)
        {
            if (_extent.Any(x => x.PhoneNumber == r.PhoneNumber && !ReferenceEquals(x, r)))
                throw new InvalidOperationException("Duplicate PhoneNumber found during load of Regular users.");
            _extent.Add(r);
        }

        // Called by User.DeleteUser() for composition cleanup
        internal static void RemoveFromExtent(Regular user)
        {
            _extent.Remove(user);
        }

        // ============================================================
        // CONSTRUCTORS
        // ============================================================
        
        public Regular(string username, string phoneNumber, bool isOnline, int adFrequency)
        {
            Username = username;
            PhoneNumber = phoneNumber;
            IsOnline = isOnline;
            AdFrequency = adFrequency; // Uses property setter for validation
            
            AddToExtent(); // Will check for duplicates
        }

        // Parameterless constructor for XML serialization
        private Regular() { }
    }
}