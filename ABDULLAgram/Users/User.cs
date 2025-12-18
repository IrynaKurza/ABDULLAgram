using ABDULLAgram.Chats;
using ABDULLAgram.Support;
using ABDULLAgram.Attachments;
using ABDULLAgram.Messages;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ABDULLAgram.Users
{
    [Serializable]
    public class User
    {
        // ============================================================
        // ASSIGNMENT 7: USER TYPE INHERITANCE VIA COMPOSITION
        // ============================================================
        
        private class RegularData
        {
            private int _adFrequency;
            public int AdFrequency
            {
                get => _adFrequency;
                set
                {
                    if (value < 0)
                        throw new ArgumentOutOfRangeException(nameof(AdFrequency), "AdFrequency cannot be negative.");
                    _adFrequency = value;
                }
            }
            
            public const int MaxStickerPacksSaved = 10;
            
            public RegularData(int adFrequency)
            {
                AdFrequency = adFrequency;
            }
        }
        
        private class PremiumData
        {
            private DateTime _premiumStartDate;
            public DateTime PremiumStartDate
            {
                get => _premiumStartDate;
                set
                {
                    if (value > _premiumEndDate)
                        throw new ArgumentException("Premium start date must be before end date.");
                    _premiumStartDate = value;
                }
            }
            
            private DateTime _premiumEndDate;
            public DateTime PremiumEndDate
            {
                get => _premiumEndDate;
                set
                {
                    if (value <= _premiumStartDate)
                        throw new ArgumentException("Premium end date must be after start date.");
                    _premiumEndDate = value;
                }
            }
            
            public PremiumData(DateTime startDate, DateTime endDate)
            {
                if (endDate <= startDate)
                    throw new ArgumentException("Premium end date must be after start date.");
                    
                _premiumStartDate = startDate;
                _premiumEndDate = endDate;
            }
            
            public int CalculateDaysUntilDue()
            {
                return Math.Max(0, (PremiumEndDate - DateTime.Now).Days);
            }
            
            public void CancelSubscription()
            {
                PremiumEndDate = DateTime.Now;
            }
        }
        
        public UserType UserType { get; set; }
        public bool IsRegular => UserType == UserType.Regular;
        public bool IsPremium => UserType == UserType.Premium;
        
        [XmlIgnore]
        private RegularData? _regularData;
        [XmlIgnore]
        private PremiumData? _premiumData;
        
        // ============================================================
        // SERIALIZATION
        // ============================================================
        
        public int? SerializedAdFrequency
        {
            get => _regularData?.AdFrequency;
            set
            {
                if (value.HasValue && UserType == UserType.Regular)
                {
                    if (_regularData == null)
                        _regularData = new RegularData(value.Value);
                    else
                        _regularData.AdFrequency = value.Value;
                }
            }
        }
        
        public DateTime? SerializedPremiumStart
        {
            get => _premiumData?.PremiumStartDate;
            set
            {
                if (value.HasValue && UserType == UserType.Premium && SerializedPremiumEnd.HasValue)
                {
                    _premiumData = new PremiumData(value.Value, SerializedPremiumEnd.Value);
                }
            }
        }
        
        public DateTime? SerializedPremiumEnd
        {
            get => _premiumData?.PremiumEndDate;
            set
            {
                if (value.HasValue && UserType == UserType.Premium)
                {
                    if (SerializedPremiumStart.HasValue)
                    {
                        _premiumData = new PremiumData(SerializedPremiumStart.Value, value.Value);
                    }
                }
            }
        }
        
        // ============================================================
        // TYPE-SPECIFIC PROPERTIES
        // ============================================================
        
        public int AdFrequency
        {
            get
            {
                if (UserType != UserType.Regular)
                    throw new InvalidOperationException("AdFrequency is only available for Regular users.");
                return _regularData!.AdFrequency;
            }
            set
            {
                if (UserType != UserType.Regular)
                    throw new InvalidOperationException("AdFrequency is only available for Regular users.");
                _regularData!.AdFrequency = value;
            }
        }
        
        public DateTime PremiumStartDate
        {
            get
            {
                if (UserType != UserType.Premium)
                    throw new InvalidOperationException("PremiumStartDate is only available for Premium users.");
                return _premiumData!.PremiumStartDate;
            }
        }
        
        public DateTime PremiumEndDate
        {
            get
            {
                if (UserType != UserType.Premium)
                    throw new InvalidOperationException("PremiumEndDate is only available for Premium users.");
                return _premiumData!.PremiumEndDate;
            }
        }
        
        public int MaxSavedStickerpacks
        {
            get
            {
                if (UserType == UserType.Regular)
                    return RegularData.MaxStickerPacksSaved;
                else
                    return int.MaxValue;
            }
        }
        
        // ============================================================
        // TYPE INITIALIZATION
        // ============================================================
        
        public void InitializeAsRegular(int adFrequency)
        {
            UserType = UserType.Regular;
            _regularData = new RegularData(adFrequency);
            _premiumData = null;
        }
        
        public void InitializeAsPremium(DateTime startDate, DateTime endDate)
        {
            UserType = UserType.Premium;
            _premiumData = new PremiumData(startDate, endDate);
            _regularData = null;
        }
        
        public void UpgradeToPremium(DateTime start, DateTime end)
        {
            if (UserType == UserType.Premium)
                throw new InvalidOperationException("User is already Premium.");
            
            _premiumData = new PremiumData(start, end);
            _regularData = null;
            UserType = UserType.Premium;
        }
        
        public int CalculateDaysUntilDue()
        {
            if (UserType != UserType.Premium)
                throw new InvalidOperationException("Only Premium users have subscription expiry.");
            
            return _premiumData!.CalculateDaysUntilDue();
        }
        
        public void CancelSubscription()
        {
            if (UserType != UserType.Premium)
                throw new InvalidOperationException("Only Premium users can cancel subscription.");
            
            _premiumData!.CancelSubscription();
        }
        
        // ============================================================
        // EXTENT
        // ============================================================
        
        private static readonly List<User> _extent = new();
        public static IReadOnlyCollection<User> GetAll() => _extent.AsReadOnly();
        
        private void AddToExtent()
        {
            if (_extent.Any(u => u.PhoneNumber == PhoneNumber))
                throw new InvalidOperationException($"A user with phone number '{PhoneNumber}' already exists.");
            
            _extent.Add(this);
        }
        
        public static void ClearExtent() => _extent.Clear();
        
        public static void ReAdd(User user)
        {
            if (_extent.Any(u => u.PhoneNumber == user.PhoneNumber && !ReferenceEquals(u, user)))
                throw new InvalidOperationException("Duplicate PhoneNumber found during load of Users.");
            
            _extent.Add(user);
        }
        
        // ============================================================
        // BASIC ATTRIBUTES
        // ============================================================
        
        private string _username = "";
        public string Username
        {
            get => _username;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Username cannot be empty.");
                _username = value;
            }
        }
        
        private string _phoneNumber = "";
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("PhoneNumber cannot be empty.");
                
                if (!string.IsNullOrWhiteSpace(_phoneNumber) && _phoneNumber != value)
                {
                    foreach (var chat in _joinedChats.ToList())
                    {
                        chat.UpdateMemberPhoneNumber(_phoneNumber, value);
                    }
                }
                
                _phoneNumber = value;
            }
        }
        
        private DateTime? _lastSeenAt;
        public DateTime? LastSeenAt
        {
            get => _lastSeenAt;
            set
            {
                if (value is { } dt && dt > DateTime.Now)
                    throw new ArgumentOutOfRangeException(nameof(LastSeenAt), "LastSeenAt cannot be in the future.");
                _lastSeenAt = value;
            }
        }
        
        public bool IsOnline { get; set; }
        
        // ============================================================
        // CONSTRUCTORS
        // ============================================================
        
        public User(string username, string phoneNumber, bool isOnline)
        {
            Username = username;
            PhoneNumber = phoneNumber;
            IsOnline = isOnline;
            
            AddToExtent();
        }
        
        public User() { }
        
        // ============================================================
        // REFLEX ASSOCIATION
        // ============================================================
        
        private List<User> _blockedUsers = new();
        private List<User> _blockedBy = new();
        
        public void BlockUser(User user)
        {
            if (user == this)
                throw new InvalidOperationException("Cannot block yourself.");
            if (_blockedUsers.Contains(user)) return;
            
            _blockedUsers.Add(user);
            user._blockedBy.Add(this);
        }
        
        public void UnblockUser(User user)
        {
            if (!_blockedUsers.Contains(user)) return;
            
            _blockedUsers.Remove(user);
            user._blockedBy.Remove(this);
        }
        
        public IReadOnlyCollection<User> GetBlockedUsers() => _blockedUsers.AsReadOnly();
        public IReadOnlyCollection<User> GetBlockedBy() => _blockedBy.AsReadOnly();
        
        // ============================================================
        // QUALIFIED ASSOCIATION: User ↔ Chat
        // ============================================================
        
        private HashSet<Chat> _joinedChats = new HashSet<Chat>();
        public IReadOnlyCollection<Chat> JoinedChats => _joinedChats.ToList().AsReadOnly();
        
        public void JoinChat(Chat chat)
        {
            if (chat == null)
                throw new ArgumentNullException(nameof(chat), "Chat cannot be null.");
            
            if (_joinedChats.Contains(chat))
                throw new InvalidOperationException("User is already a member of this chat.");
            
            _joinedChats.Add(chat);
            chat.AddMemberInternal(this);
        }
        
        public void LeaveChat(Chat chat)
        {
            if (chat == null)
                throw new ArgumentNullException(nameof(chat), "Chat cannot be null.");
            
            if (!_joinedChats.Contains(chat))
                throw new InvalidOperationException("User is not a member of this chat.");
            
            _joinedChats.Remove(chat);
            chat.RemoveMemberInternal(PhoneNumber);
        }
        
        internal void AddChatInternal(Chat chat)
        {
            _joinedChats.Add(chat);
        }
        
        internal void RemoveChatInternal(Chat chat)
        {
            _joinedChats.Remove(chat);
        }
        
        // ============================================================
        // BASIC ASSOCIATION: User ↔ Message
        // ============================================================
        
        private readonly HashSet<Message> _sentMessages = new();
        public IReadOnlyCollection<Message> SentMessages => _sentMessages.ToList().AsReadOnly();
        
        public void AddMessage(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            
            if (_sentMessages.Contains(message))
                return;
            
            _sentMessages.Add(message);
            message.SetSenderInternal(this);
        }
        
        public void RemoveMessage(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            
            if (!_sentMessages.Contains(message))
                return;
            
            _sentMessages.Remove(message);
            message.ClearSenderInternal();
        }
        
        internal void AddMessageInternal(Message message)
        {
            _sentMessages.Add(message);
        }
        
        internal void RemoveMessageInternal(Message message)
        {
            _sentMessages.Remove(message);
        }
        
        // ============================================================
        // BASIC ASSOCIATION: User ↔ Text (mentioned in)
        // ============================================================
        
        private readonly HashSet<Text> _mentionedInTexts = new();
        public IReadOnlyCollection<Text> MentionedInTexts => _mentionedInTexts.ToList().AsReadOnly();
        
        internal void AddMentionedInText(Text text)
        {
            _mentionedInTexts.Add(text);
        }
        
        internal void RemoveMentionedInText(Text text)
        {
            _mentionedInTexts.Remove(text);
        }
        
        // ============================================================
        // BASIC ASSOCIATION: User ↔ Message (read messages)
        // ============================================================
        
        private readonly HashSet<Message> _readMessages = new();
        public IReadOnlyCollection<Message> ReadMessages => _readMessages.ToList().AsReadOnly();
        
        internal void AddReadMessageInternal(Message message)
        {
            _readMessages.Add(message);
        }
        
        // ============================================================
        // COMPOSITION: User → Folder
        // ============================================================
        
        private readonly List<Folder> _ownedFolders = new();
        public IReadOnlyCollection<Folder> OwnedFolders => _ownedFolders.AsReadOnly();
        
        public Folder CreateFolder(string name)
        {
            var folder = new Folder(this, name);
            return folder;
        }
        
        internal void RemoveFolderInternal(Folder folder)
        {
            _ownedFolders.Remove(folder);
        }
        
        public void DeleteUser()
        {
            foreach (var folder in _ownedFolders.ToList())
            {
                folder.Delete();
            }
            
            _extent.Remove(this);
        }
        
        // ============================================================
        // QUALIFIED ASSOCIATION: User ↔ Stickerpack
        // ============================================================
        
        private readonly Dictionary<string, Stickerpack> _savedStickerpacks = new();
        public IReadOnlyCollection<Stickerpack> SavedStickerpacks => _savedStickerpacks.Values.ToList().AsReadOnly();
        
        public void SaveStickerpack(Stickerpack pack)
        {
            if (pack == null)
                throw new ArgumentNullException(nameof(pack));
            
            if (_savedStickerpacks.Count >= MaxSavedStickerpacks)
                throw new InvalidOperationException($"Cannot save more than {MaxSavedStickerpacks} stickerpacks.");
            
            if (_savedStickerpacks.ContainsKey(pack.Name))
                throw new InvalidOperationException("Stickerpack already saved.");
            
            _savedStickerpacks.Add(pack.Name, pack);
            pack.AddSavedByUserInternal(this);
        }
        
        public void UnsaveStickerpack(Stickerpack pack)
        {
            if (pack == null)
                throw new ArgumentNullException(nameof(pack));
            
            if (!_savedStickerpacks.ContainsKey(pack.Name))
                throw new InvalidOperationException("Stickerpack is not saved.");
            
            _savedStickerpacks.Remove(pack.Name);
            pack.RemoveSavedByUserInternal(this);
        }
        
        public Stickerpack? GetSavedStickerpackByName(string name)
        {
            return _savedStickerpacks.GetValueOrDefault(name);
        }
        
        internal void AddSavedStickerpackInternal(Stickerpack pack)
        {
            _savedStickerpacks[pack.Name] = pack;
        }
        
        internal void RemoveSavedStickerpackInternal(Stickerpack pack)
        {
            _savedStickerpacks.Remove(pack.Name);
        }
        
        // ============================================================
        // ASSOCIATION: User manages Stickerpack
        // ============================================================
        
        private readonly List<Stickerpack> _managedStickerpacks = new();
        public IReadOnlyCollection<Stickerpack> ManagedStickerpacks => _managedStickerpacks.AsReadOnly();
        
        internal void AddManagedStickerpackInternal(Stickerpack pack)
        {
            _managedStickerpacks.Add(pack);
        }
        
        internal void RemoveManagedStickerpackInternal(Stickerpack pack)
        {
            _managedStickerpacks.Remove(pack);
        }
        
        // ============================================================
        // MISSING INTERNAL METHODS
        // ============================================================
        
        private readonly HashSet<Chat> _adminOfGroups = new();
        public IReadOnlyCollection<Chat> AdminOfGroups => _adminOfGroups.ToList().AsReadOnly();
        
        internal void AddAdminOfGroupInternal(Chat chat)
        {
            _adminOfGroups.Add(chat);
        }
        
        internal void RemoveAdminOfGroupInternal(Chat chat)
        {
            _adminOfGroups.Remove(chat);
        }
        
        internal void AddStickerpackInternal(Stickerpack pack)
        {
            AddManagedStickerpackInternal(pack);
        }
        
        internal void RemoveStickerpackInternal(Stickerpack pack)
        {
            RemoveManagedStickerpackInternal(pack);
        }
        
        internal void AddFolderInternal(Folder folder)
        {
            _ownedFolders.Add(folder);
        }
    }
}