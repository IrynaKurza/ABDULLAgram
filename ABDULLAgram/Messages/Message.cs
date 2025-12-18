using ABDULLAgram.Chats;
using ABDULLAgram.Users;
using System.Xml.Serialization;

namespace ABDULLAgram.Messages
{
    [Serializable]
    [XmlInclude(typeof(Attachments.Text))]
    [XmlInclude(typeof(Attachments.Image))]
    [XmlInclude(typeof(Attachments.Sticker))]
    [XmlInclude(typeof(Attachments.Video))]
    [XmlInclude(typeof(Attachments.File))]
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
        
        // ============================================================
        // COMPOSITION: Message has Draft OR Sent (XOR)
        // ============================================================
        
        private Draft? _draft;
        private Sent? _sent;

        public Draft? Draft => _draft;
        public Sent? Sent => _sent;

        public bool IsDraft => _draft != null;
        public bool IsSent => _sent != null;

        // Called by Draft.SendMessage to transition state
        public void PromoteToSent(Sent sent)
        {
            if (sent == null) throw new ArgumentNullException(nameof(sent));
            if (_sent != null) throw new InvalidOperationException("Message is already sent.");
            
            _draft = null; // Remove reference to draft (it will be deleted by Draft class)
            _sent = sent;
        }

        // ============================================================
        // ASSOCIATIONS
        // ============================================================

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

        // ============================================================
        // CONSTRUCTOR & DESTRUCTOR
        // ============================================================

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
            
            // Default state is Draft
            _draft = new Draft(DateTime.Now);
        }

        protected Message() { }

        protected abstract void RemoveFromExtent();

        public virtual void Delete()
        {
            _sender?.RemoveMessageInternal(this);
            _targetChat?.RemoveMessageInternal(this);

            _sender = null;
            _targetChat = null;

            // Cleanup composition parts
            _draft?.Delete();
            _sent?.Delete();

            RemoveFromExtent();
        }

    }
}