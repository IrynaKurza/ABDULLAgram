namespace ABDULLAgram.Messages
{
    [Serializable]
    public class Draft : Message
    {
        // Draft-specific attributes
        private DateTime _lastSaveTimestamp;
        public DateTime LastSaveTimestamp
        {
            get => _lastSaveTimestamp;
            set
            {
                if (value > DateTime.Now)
                    throw new ArgumentOutOfRangeException(nameof(LastSaveTimestamp), "LastSaveTimestamp cannot be in the future.");
                _lastSaveTimestamp = value;
            }
        }

        // Override Id to add uniqueness check (like Regular does with PhoneNumber)
        public override string Id
        {
            get => base.Id;
            set
            {
                bool exists = _extent.Any(m => !ReferenceEquals(m, this) && m.Id == value);
                if (exists)
                    throw new InvalidOperationException("Draft Id must be unique among all Draft messages.");
                
                base.Id = value; // Calls parent validation
            }
        }

        // Class Extent
        private static readonly List<Draft> _extent = new();
        public static IReadOnlyCollection<Draft> GetAll() => _extent.AsReadOnly();
        private void AddToExtent() => _extent.Add(this);
        public static void ClearExtent() => _extent.Clear();
        public static void ReAdd(Draft d)
        {
            if (_extent.Any(x => x.Id == d.Id && !ReferenceEquals(x, d)))
                throw new InvalidOperationException("Duplicate Id found during load of Draft messages.");
            _extent.Add(d);
        }

        // Constructors
        public Draft(DateTime lastSaveTimestamp)
        {
            LastSaveTimestamp = lastSaveTimestamp;
            SetSize(0);

            AddToExtent();
        }

        private Draft() { } // for XML serialization
    }
}