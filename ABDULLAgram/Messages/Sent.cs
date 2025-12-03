using ABDULLAgram.Chats;
using ABDULLAgram.Users;

namespace ABDULLAgram.Messages
{
    [Serializable]
    public class Sent : Message
    {
        // Sent-specific attributes
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

        // Override Id to add uniqueness check (like Regular does with PhoneNumber)
        public override string Id
        {
            get => base.Id;
            set
            {
                bool exists = _extent.Any(m => !ReferenceEquals(m, this) && m.Id == value);
                if (exists)
                    throw new InvalidOperationException("Sent Id must be unique among all Sent messages.");
                
                base.Id = value; // Calls parent validation
            }
        }

        // Class Extent
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

        // Constructors
        public Sent(User sender, Chat chat, DateTime sendTimestamp, DateTime deliveredAt, DateTime? editedAt, DateTime? deletedAt)
            : base(sender, chat)
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