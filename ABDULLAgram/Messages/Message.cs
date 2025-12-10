using ABDULLAgram.Chats;
using ABDULLAgram.Users;

namespace ABDULLAgram.Messages
{
    [Serializable]
    public abstract class Message
    {
        public const long MaximumSize = 10L * 1024 * 1024 * 1024;

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

        // ============================================================
        // BASIC ASSOCIATION: Message → User (many-to-one)
        // Each message has exactly one sender
        // User can send multiple messages
        // ============================================================
        
        private User _sender;
        
        public User Sender
        {
            get => _sender;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(Sender), "A Message must have a Sender.");

                // REVERSE CONNECTION LOGIC: Handle sender changes
                // If message is reassigned to different user, update both sides
                if (_sender != value)
                {
                    // Remove from old sender's list
                    if (_sender != null)
                    {
                        _sender.RemoveMessage(this);
                    }

                    _sender = value;

                    // Add to new sender's list
                    _sender.AddMessage(this);
                }
            }
        }

        // ============================================================
        // BASIC ASSOCIATION: Message → Chat (many-to-one)
        // Each message belongs to exactly one chat
        // Chat can have multiple messages (history)
        // ============================================================
        
        private Chat _targetChat;
        
        public Chat TargetChat
        {
            get => _targetChat;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(TargetChat), "A Message must belong to a Chat.");

                // REVERSE CONNECTION LOGIC: Handle chat changes
                // If message is moved to different chat, update history in both chats
                if (_targetChat != value)
                {
                    // Remove from old chat's history
                    if (_targetChat != null)
                    {
                        _targetChat.RemoveMessage(this);
                    }

                    _targetChat = value;

                    // Add to new chat's history
                    _targetChat.AddMessage(this);
                }
            }
        }

        // PROTECTED CONSTRUCTOR: Only subclasses can create messages
        // Automatically establishes bidirectional associations
        protected Message(User sender, Chat chat)
        {
            if (sender is null) throw new ArgumentNullException(nameof(sender));
            if (chat is null) throw new ArgumentNullException(nameof(chat));

            // Using property setters triggers reverse connection logic
            Sender = sender;
            TargetChat = chat;
        }

        protected Message() { }

        // DELETE METHOD: Removes all bidirectional connections before deletion
        public virtual void Delete()
        {
            // Remove from sender's message list
            if (_sender != null)
            {
                _sender.RemoveMessage(this);
                _sender = null;
            }

            // Remove from chat's history
            if (_targetChat != null)
            {
                _targetChat.RemoveMessage(this);
                _targetChat = null;
            }
        }
    }
}