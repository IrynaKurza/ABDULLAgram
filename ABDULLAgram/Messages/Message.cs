using ABDULLAgram.Chats;
using ABDULLAgram.Users;

namespace ABDULLAgram.Messages
{
    [Serializable]
    public abstract class Message
    {
        // ============================================================
        // ASSIGNMENT 7: STATE INHERITANCE VIA COMPOSITION
        // Inner classes hold state-specific data (Draft vs Sent)
        // Discriminator enum determines which inner class is active
        // Dynamic constraint: Draft → Sent allowed, Sent → Draft FORBIDDEN
        // ============================================================
        
        private class DraftData
        {
            private DateTime _lastSaveTimestamp;
            public DateTime LastSaveTimestamp
            {
                get => _lastSaveTimestamp;
                set
                {
                    if (value > DateTime.Now)
                        throw new ArgumentOutOfRangeException(nameof(LastSaveTimestamp), 
                            "LastSaveTimestamp cannot be in the future.");
                    _lastSaveTimestamp = value;
                }
            }
            
            public DraftData()
            {
                _lastSaveTimestamp = DateTime.Now;
            }
            
            public DraftData(DateTime lastSaveTimestamp)
            {
                LastSaveTimestamp = lastSaveTimestamp;
            }
        }
        
        private class SentData
        {
            private DateTime _sendTimestamp;
            public DateTime SendTimestamp
            {
                get => _sendTimestamp;
                set
                {
                    if (value > DateTime.Now)
                        throw new ArgumentOutOfRangeException(nameof(SendTimestamp), 
                            "SendTimestamp cannot be in the future.");
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
                        throw new ArgumentOutOfRangeException(nameof(DeliveredAt), 
                            "DeliveredAt cannot be in the future.");
                    if (value < SendTimestamp)
                        throw new ArgumentOutOfRangeException(nameof(DeliveredAt), 
                            "DeliveredAt cannot be earlier than SendTimestamp.");
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
                            throw new ArgumentOutOfRangeException(nameof(EditedAt), 
                                "EditedAt cannot be in the future.");
                        if (dt < SendTimestamp)
                            throw new ArgumentOutOfRangeException(nameof(EditedAt), 
                                "EditedAt cannot be earlier than SendTimestamp.");
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
                            throw new ArgumentOutOfRangeException(nameof(DeletedAt), 
                                "DeletedAt cannot be in the future.");
                        if (dt < SendTimestamp)
                            throw new ArgumentOutOfRangeException(nameof(DeletedAt), 
                                "DeletedAt cannot be earlier than SendTimestamp.");
                    }
                    _deletedAt = value;
                }
            }
            
            public SentData(DateTime sendTimestamp)
            {
                SendTimestamp = sendTimestamp;
                DeliveredAt = sendTimestamp;
            }
            
            public SentData(DateTime sendTimestamp, DateTime deliveredAt, DateTime? editedAt, DateTime? deletedAt)
            {
                SendTimestamp = sendTimestamp;
                DeliveredAt = deliveredAt;
                EditedAt = editedAt;
                DeletedAt = deletedAt;
            }
        }
        
        // Discriminator field - determines which inner class is used
        public MessageStateType StateType { get; set; }
        
        // Convenience properties for state checking
        public bool IsDraft => StateType == MessageStateType.Draft;
        public bool IsSent => StateType == MessageStateType.Sent;
        
        // State data holders (only one is non-null at a time)
        private DraftData? _draftData;
        private SentData? _sentData;
        
        // ============================================================
        // STATE-SPECIFIC PROPERTY ACCESSORS
        // ============================================================
        
        // Draft-specific: LastSaveTimestamp
        public DateTime LastSaveTimestamp
        {
            get
            {
                if (StateType != MessageStateType.Draft)
                    throw new InvalidOperationException("LastSaveTimestamp is only available for Draft messages.");
                return _draftData!.LastSaveTimestamp;
            }
            set
            {
                if (StateType != MessageStateType.Draft)
                    throw new InvalidOperationException("LastSaveTimestamp is only available for Draft messages.");
                _draftData!.LastSaveTimestamp = value;
            }
        }
        
        // Sent-specific properties
        public DateTime SendTimestamp
        {
            get
            {
                if (StateType != MessageStateType.Sent)
                    throw new InvalidOperationException("SendTimestamp is only available for Sent messages.");
                return _sentData!.SendTimestamp;
            }
        }
        
        public DateTime DeliveredAt
        {
            get
            {
                if (StateType != MessageStateType.Sent)
                    throw new InvalidOperationException("DeliveredAt is only available for Sent messages.");
                return _sentData!.DeliveredAt;
            }
            set
            {
                if (StateType != MessageStateType.Sent)
                    throw new InvalidOperationException("DeliveredAt is only available for Sent messages.");
                _sentData!.DeliveredAt = value;
            }
        }
        
        public DateTime? EditedAt
        {
            get
            {
                if (StateType != MessageStateType.Sent)
                    throw new InvalidOperationException("EditedAt is only available for Sent messages.");
                return _sentData!.EditedAt;
            }
            set
            {
                if (StateType != MessageStateType.Sent)
                    throw new InvalidOperationException("EditedAt is only available for Sent messages.");
                _sentData!.EditedAt = value;
            }
        }
        
        public DateTime? DeletedAt
        {
            get
            {
                if (StateType != MessageStateType.Sent)
                    throw new InvalidOperationException("DeletedAt is only available for Sent messages.");
                return _sentData!.DeletedAt;
            }
            set
            {
                if (StateType != MessageStateType.Sent)
                    throw new InvalidOperationException("DeletedAt is only available for Sent messages.");
                _sentData!.DeletedAt = value;
            }
        }
        
        // ============================================================
        // STATE INITIALIZATION (called by subclass constructors)
        // ============================================================
        
        public void InitializeAsDraft(DateTime? lastSaveTimestamp = null)
        {
            StateType = MessageStateType.Draft;
            _draftData = lastSaveTimestamp.HasValue 
                ? new DraftData(lastSaveTimestamp.Value) 
                : new DraftData();
            _sentData = null;
        }
        
        public void InitializeAsSent(DateTime sendTimestamp, DateTime deliveredAt, 
            DateTime? editedAt = null, DateTime? deletedAt = null)
        {
            StateType = MessageStateType.Sent;
            _sentData = new SentData(sendTimestamp, deliveredAt, editedAt, deletedAt);
            _draftData = null;
        }
        
        // ============================================================
        // STATE TRANSITIONS
        // ============================================================
        
        /// <summary>
        /// Send the message - transitions from Draft to Sent.
        /// One-way constraint: Draft → Sent is allowed.
        /// </summary>
        public virtual void Send()
        {
            if (StateType == MessageStateType.Sent)
                throw new InvalidOperationException("Message is already sent. Cannot send again.");
            
            // Transition from Draft to Sent
            var now = DateTime.Now;
            _sentData = new SentData(now);
            _draftData = null;
            StateType = MessageStateType.Sent;
        }
        
        /// <summary>
        /// Attempt to convert to Draft - FORBIDDEN for sent messages.
        /// One-way constraint: Sent → Draft is NOT allowed.
        /// </summary>
        public void ConvertToDraft()
        {
            if (StateType == MessageStateType.Sent)
                throw new InvalidOperationException(
                    "Cannot convert a sent message back to draft. This transition is forbidden.");
            
            // Already draft - no-op
        }
        
        /// <summary>
        /// Save the draft - updates lastSaveTimestamp.
        /// Only valid for Draft state.
        /// </summary>
        public void SaveDraft()
        {
            if (StateType != MessageStateType.Draft)
                throw new InvalidOperationException("Only draft messages can be saved as draft.");
            
            _draftData!.LastSaveTimestamp = DateTime.Now;
        }
        
        /// <summary>
        /// Edit the message - behavior differs by state.
        /// Draft: updates lastSaveTimestamp
        /// Sent: updates editedAt timestamp
        /// </summary>
        public void EditMessage()
        {
            if (StateType == MessageStateType.Draft)
            {
                _draftData!.LastSaveTimestamp = DateTime.Now;
            }
            else
            {
                if (_sentData!.DeletedAt.HasValue)
                    throw new InvalidOperationException("Cannot edit a deleted message.");
                _sentData.EditedAt = DateTime.Now;
            }
        }
        
        /// <summary>
        /// Mark the message as deleted.
        /// Only valid for Sent state.
        /// </summary>
        public void MarkAsDeletedState()
        {
            if (StateType != MessageStateType.Sent)
                throw new InvalidOperationException("Only sent messages can be marked as deleted.");
            
            if (_sentData!.DeletedAt.HasValue)
                throw new InvalidOperationException("Message is already deleted.");
            
            _sentData.DeletedAt = DateTime.Now;
        }
        
        // ============================================================
        // SENT-SPECIFIC ASSOCIATION: ReadByUsers (only for Sent messages)
        // ============================================================
        
        private readonly HashSet<User> _readByUsers = new();
        public IReadOnlyCollection<User> ReadByUsers
        {
            get
            {
                if (StateType != MessageStateType.Sent)
                    throw new InvalidOperationException("ReadByUsers is only available for Sent messages.");
                return _readByUsers.ToList().AsReadOnly();
            }
        }
        
        public void MarkAsRead(User user)
        {
            if (StateType != MessageStateType.Sent)
                throw new InvalidOperationException("Only sent messages can be marked as read.");
            
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            
            if (_readByUsers.Contains(user))
                return;
            
            _readByUsers.Add(user);
            user.AddReadMessageInternal(this);
        }
        
        // ============================================================
        // BASIC ATTRIBUTES (unchanged)
        // ============================================================
        
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