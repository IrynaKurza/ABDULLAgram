using System;
using System.Collections.Generic;

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

        private DateTime _serverReadAt;
        public DateTime ServerReadAt
        {
            get => _serverReadAt;
            set
            {
                if (value > DateTime.Now)
                    throw new ArgumentOutOfRangeException(nameof(ServerReadAt), "ServerReadAt cannot be in the future.");
                if (value < SendTimestamp)
                    throw new ArgumentOutOfRangeException(nameof(ServerReadAt), "ServerReadAt cannot be earlier than SendTimestamp.");
                _serverReadAt = value;
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

        public bool IsEditedMessage { get; set; }
        
        // Message attributes
        private string _id = Guid.NewGuid().ToString();
        public string Id
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
        public const long MaxSizeBytes = 10L * 1024 * 1024 * 1024;

        private long _messageSize;
        public long MessageSize => _messageSize;        

        private void SetSize(long bytes)                
        {
            if (bytes < 0) throw new ArgumentOutOfRangeException(nameof(bytes));
            if (bytes > MaxSizeBytes) throw new ArgumentOutOfRangeException(nameof(bytes), "Message size cannot exceed 10GB.");
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
        public Sent(DateTime sendTimestamp, DateTime serverReadAt, DateTime? deletedAt, bool isEdited)
        {
            SendTimestamp = sendTimestamp;
            ServerReadAt = serverReadAt;
            DeletedAt = deletedAt;
            IsEditedMessage = isEdited;
            SetSize(0); 
            
            AddToExtent();
        }

        private Sent() { } // for XML serialization
    }
}