using ABDULLAgram.Chats;
using ABDULLAgram.Users;

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
        
        private User _sender;
        private Chat _targetChat;

        // Association to User
        public User Sender
        {
            get => _sender;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(Sender), "A Message must have a Sender.");

                // Reverse Connection Logic
                if (_sender != value)
                {
                    if (_sender != null)
                    {
                        _sender.RemoveMessage(this);
                    }

                    _sender = value;

                    _sender.AddMessage(this);
                }
            }
        }

        // Association to Chat
        public Chat TargetChat
        {
            get => _targetChat;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(TargetChat), "A Message must belong to a Chat.");

                // Reverse Connection Logic
                if (_targetChat != value)
                {
                    if (_targetChat != null)
                    {
                        _targetChat.RemoveMessage(this);
                    }

                    _targetChat = value;

                    _targetChat.AddMessage(this);
                }
            }
        }
        
        protected Message(User sender, Chat chat)
        {
            if (sender is null) throw new ArgumentNullException(nameof(sender));
            if (chat is null) throw new ArgumentNullException(nameof(chat));

            Sender = sender;
            TargetChat = chat;
        }

        protected Message() { } 
        
        public virtual void Delete()
        {
            if (_sender != null)
            {
                _sender.RemoveMessage(this);
                _sender = null;
            }

            if (_targetChat != null)
            {
                _targetChat.RemoveMessage(this);
                _targetChat = null;
            }
        }
    }
}