namespace ABDULLAgram.Messages
{
    [Serializable]
    public abstract class Message
    {
        public const long MaximumSize = 10L * 1024 * 1024 * 1024;

        // Common Id attribute with validation
        private string _id = Guid.NewGuid().ToString();
        public virtual string Id
        {
            get => _id;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Id cannot be empty.");
                _id = value;
            }
        }

        // Common MessageSize attribute
        protected long _messageSize;
        public long MessageSize => _messageSize;

        protected void SetSize(long bytes)
        {
            if (bytes < 0) 
                throw new ArgumentOutOfRangeException(nameof(bytes), "Message size cannot be negative.");
            if (bytes > MaximumSize) 
                throw new ArgumentOutOfRangeException(nameof(bytes), "Message size cannot exceed 10GB.");
            _messageSize = bytes;
        }
    }
}