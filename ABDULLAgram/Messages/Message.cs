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

        private User? _sender;
        private Chat? _targetChat;
        
        internal void SetSenderInternal(User user)
        {
            _sender = user;
        }
        
        internal void ClearSenderInternal()
        {
            _sender = null;
        }

        public User? Sender
        {
            get => _sender;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(Sender));

                if (_sender == value)
                    return;

                _sender?.RemoveMessageInternal(this);
                _sender = value;
                _sender.AddMessageInternal(this);
            }
        }

        
        internal void SetTargetChatInternal(Chat chat)
        {
            _targetChat = chat;
        }

        internal void ClearTargetChatInternal()
        {
            _targetChat = null;
        }
        
        public Chat? TargetChat
        {
            get => _targetChat;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(TargetChat));

                if (_targetChat == value)
                    return;

                _targetChat?.RemoveMessageInternal(this);
                _targetChat = value;
                _targetChat.AddMessageInternal(this);
            }
        }


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

        protected abstract void RemoveFromExtent();

        public virtual void Delete()
        {
            _sender?.RemoveMessageInternal(this);
            _targetChat?.RemoveMessageInternal(this);

            _sender = null;
            _targetChat = null;

            RemoveFromExtent();
        }

    }
}