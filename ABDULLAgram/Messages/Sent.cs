namespace ABDULLAgram.Messages
{
    [Serializable]
    public class Sent : Message
    {
        
        // Sent attributes
        private DateTime _sendTimestamp;
        public DateTime SendTimestamp
        {
            get => _sendTimestamp;
            set
            {
                if (value > DateTime.Now)
                    throw new ArgumentOutOfRangeException(nameof(SendTimestamp), "SendTimestamp cannot be in the future.");
                _sendTimestamp = value;
            }
        }

        private DateTime _deliveredAt;
        public DateTime DeliveredAt
        {
            get => _deliveredAt;
            set
            {
                if (value > DateTime.Now)
                    throw new ArgumentOutOfRangeException(nameof(DeliveredAt), "DeliveredAt cannot be in the future.");
                if (value < SendTimestamp)
                    throw new ArgumentOutOfRangeException(nameof(DeliveredAt), "DeliveredAt cannot be earlier than SendTimestamp.");
                _deliveredAt = value;
            }
        }

        private DateTime? _editedAt;
        public DateTime? EditedAt
        {
            get => _editedAt;
            set
            {
                if (value is DateTime dt)
                {
                    if (dt > DateTime.Now)
                        throw new ArgumentOutOfRangeException(nameof(EditedAt), "EditedAt cannot be in the future.");
                    if (dt < SendTimestamp)
                        throw new ArgumentOutOfRangeException(nameof(EditedAt), "EditedAt cannot be earlier than SendTimestamp.");
                }
                _editedAt = value;
            }
        }

        private DateTime? _deletedAt;
        public DateTime? DeletedAt
        {
            get => _deletedAt;
            set
            {
                if (value is DateTime dt)
                {
                    if (dt > DateTime.Now)
                        throw new ArgumentOutOfRangeException(nameof(DeletedAt), "DeletedAt cannot be in the future.");
                    if (dt < SendTimestamp)
                        throw new ArgumentOutOfRangeException(nameof(DeletedAt), "DeletedAt cannot be earlier than SendTimestamp.");
                }
                _deletedAt = value;
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
                bool exists = _extent.Any(m => !ReferenceEquals(m, this) && m.Id == value);
                if (exists)
                    throw new InvalidOperationException("Message Id must be unique among all Sent messages.");

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
        private static readonly List<Sent> _extent = new();
        public static IReadOnlyCollection<Sent> GetAll() => _extent.AsReadOnly();
        private void AddToExtent() => _extent.Add(this);
        public static void ClearExtent() => _extent.Clear();
        public static void ReAdd(Sent s)
        {
            if (_extent.Any(x => x.Id == s.Id && !ReferenceEquals(x, s)))
                throw new InvalidOperationException("Duplicate Id found during load of Sent messages.");
            _extent.Add(s);
        }
        
        public Sent(DateTime sendTimestamp, DateTime deliveredAt, DateTime? editedAt, DateTime? deletedAt)
        {
            SendTimestamp = sendTimestamp;
            DeliveredAt = deliveredAt;
            EditedAt = editedAt;
            DeletedAt = deletedAt;
            SetSize(0); 
            
            AddToExtent();
        }

        private Sent() { } // for XML serialization
    }
}