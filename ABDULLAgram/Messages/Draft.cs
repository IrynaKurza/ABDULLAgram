namespace ABDULLAgram.Messages
{
    [Serializable]
    public class Draft : Message
    {
        // Draft attributes
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

        // Message attributes
        private string _id = Guid.NewGuid().ToString();
        public override string Id
        {
            get => _id;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Id cannot be empty.");

                // uniqueness check
                bool exists = _extent.Any(d => !ReferenceEquals(d, this) && d.Id == value);
                if (exists)
                    throw new InvalidOperationException("Message Id must be unique among all Draft messages.");

                _id = value;
            }
        }

        private long _messageSize;
        public override long MessageSize => _messageSize;

        private void SetSize(long bytes)
        {
            if (bytes < 0) throw new ArgumentOutOfRangeException(nameof(bytes));
            if (bytes > MaximumSize) throw new ArgumentOutOfRangeException(nameof(bytes), "Message size cannot exceed 10GB.");
            _messageSize = bytes;
        }

        // Class extent
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