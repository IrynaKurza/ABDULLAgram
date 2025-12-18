using ABDULLAgram.Chats;
using ABDULLAgram.Users;

namespace ABDULLAgram.Messages
{
    [Serializable]
    public class Sent
    {
        // ============================================================
        // COMPONENT ATTRIBUTES
        // ============================================================
        
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

        // Component ID for Extent
        private string _id = Guid.NewGuid().ToString();
        public string Id 
        { 
            get => _id; 
            set 
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Id cannot be empty.");
                if (_extent.Any(s => s.Id == value && !ReferenceEquals(s, this)))
                    throw new InvalidOperationException("Sent Id must be unique.");
                _id = value;
            }
        }

        // ============================================================
        // CLASS EXTENT
        // ============================================================

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

        public void Delete()
        {
            _extent.Remove(this);
        }

        // ============================================================
        // CONSTRUCTORS
        // ============================================================

        public Sent(DateTime sendTimestamp, DateTime deliveredAt, DateTime? editedAt, DateTime? deletedAt)
        {
            SendTimestamp = sendTimestamp;
            DeliveredAt = deliveredAt;
            EditedAt = editedAt;
            DeletedAt = deletedAt;
            
            AddToExtent();
        }

        private Sent() { } // XML serialization

        // ============================================================
        // ASSOCIATION: Sent (0..*) — read by — User (0..*)
        // ============================================================

        private readonly HashSet<User> _readByUsers = new();
        public IReadOnlyCollection<User> ReadByUsers => _readByUsers.ToList().AsReadOnly();
        
        public void MarkAsRead(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (_readByUsers.Contains(user))
                return;

            _readByUsers.Add(user);

            // REVERSE CONNECTION (internal)
            user.AddReadMessageInternal(this);
        }
    }
}