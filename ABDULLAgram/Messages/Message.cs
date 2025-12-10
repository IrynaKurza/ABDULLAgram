using ABDULLAgram.Chats;
using ABDULLAgram.Users;

namespace ABDULLAgram.Messages
{
    [Serializable]
    public abstract class Message
    {
        public const long MaximumSize = 10L * 1024 * 1024 * 1024;
        private string _id = Guid.NewGuid().ToString();
        public virtual string Id { get => _id; set { if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Id cannot be empty."); _id = value; } }
        protected long _messageSize;
        public long MessageSize => _messageSize;
        protected void SetSize(long bytes) { if (bytes < 0) throw new ArgumentOutOfRangeException(nameof(bytes), "Message size cannot be negative."); if (bytes > MaximumSize) throw new ArgumentOutOfRangeException(nameof(bytes), "Message size cannot exceed 10GB."); _messageSize = bytes; }

        // ============================================================
        // BASIC ASSOCIATION: Message → User (many-to-one)
        // ============================================================
        
        private User _sender;
        
        public User Sender
        {
            get => _sender;
            set
            {
                // REVERSE CONNECTION LOGIC
                if (_sender != value)
                {
                    if (_sender != null)
                    {
                        _sender.RemoveMessage(this);
                    }

                    _sender = value;

                    if (_sender != null)
                    {
                        _sender.AddMessage(this);
                    }
                }
            }
        }

        // ============================================================
        // BASIC ASSOCIATION: Message → Chat (many-to-one)
        // ============================================================
        
        private Chat _targetChat;
        
        public Chat TargetChat
        {
            get => _targetChat;
            set
            {
                // REVERSE CONNECTION LOGIC
                if (_targetChat != value)
                {
                    if (_targetChat != null)
                    {
                        _targetChat.RemoveMessage(this);
                    }

                    _targetChat = value;

                    if (_targetChat != null)
                    {
                        _targetChat.AddMessage(this);
                    }
                }
            }
        }

        // PROTECTED CONSTRUCTOR
        protected Message(User sender, Chat chat)
        {
            if (sender is null) throw new ArgumentNullException(nameof(sender));
            if (chat is null) throw new ArgumentNullException(nameof(chat));

            if (chat.GetMemberByPhoneNumber(sender.PhoneNumber) != sender)
            {
                throw new InvalidOperationException("Sender must be a member of the chat to send a message.");
            }

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