using ABDULLAgram.Chats;
using ABDULLAgram.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using ABDULLAgram.Support;
using ABDULLAgram.Attachments;
using System.Xml.Serialization;
using ABDULLAgram.Messages;

namespace ABDULLAgram.Users
{
    [XmlInclude(typeof(Regular))]
    [XmlInclude(typeof(Premium))]
    [Serializable]
    public abstract class User
    {
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
        public virtual string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("PhoneNumber cannot be empty.");

                // If phone number is changing, update it in all chats
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
                if (value is DateTime dt && dt > DateTime.Now)
                    throw new ArgumentOutOfRangeException(nameof(LastSeenAt), "LastSeenAt cannot be in the future.");
                _lastSeenAt = value;
            }
        }

        public bool IsOnline { get; set; }

        // Reflex Association
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
        // QUALIFIED ASSOCIATION: User ↔ Chat (reverse connection)
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
            chat.RemoveMemberInternal(this.PhoneNumber);
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
        // BASIC ASSOCIATION: User ↔ Stickerpack
        // ============================================================
        
        private HashSet<Stickerpack> _savedStickerpacks = new HashSet<Stickerpack>();
        public IReadOnlyCollection<Stickerpack> SavedStickerpacks => _savedStickerpacks.ToList().AsReadOnly();

        public abstract int MaxSavedStickerpacks { get; }

        public virtual void SaveStickerpack(Stickerpack pack)
        {
            if (pack == null)
                throw new ArgumentNullException(nameof(pack), "Stickerpack cannot be null.");

            if (_savedStickerpacks.Contains(pack))
                throw new InvalidOperationException("This stickerpack is already saved.");

            if (_savedStickerpacks.Count >= MaxSavedStickerpacks)
                throw new InvalidOperationException($"Cannot save more than {MaxSavedStickerpacks} stickerpacks.");

            _savedStickerpacks.Add(pack);
            pack.AddSavedByUserInternal(this);
        }

        public void UnsaveStickerpack(Stickerpack pack)
        {
            if (pack == null)
                throw new ArgumentNullException(nameof(pack), "Stickerpack cannot be null.");

            if (!_savedStickerpacks.Contains(pack))
                throw new InvalidOperationException("This stickerpack is not saved.");

            _savedStickerpacks.Remove(pack);
            pack.RemoveSavedByUserInternal(this);
        }

        internal void AddStickerpackInternal(Stickerpack pack)
        {
            _savedStickerpacks.Add(pack);
        }

        internal void RemoveStickerpackInternal(Stickerpack pack)
        {
            _savedStickerpacks.Remove(pack);
        }

        private readonly HashSet<Folder> _folders = new();
        public IReadOnlyCollection<Folder> Folders => _folders.ToList().AsReadOnly();

        internal void AddFolderInternal(Folder folder)
        {
            if (folder == null) throw new ArgumentNullException(nameof(folder));
            _folders.Add(folder);
        }

        internal void RemoveFolderInternal(Folder folder)
        {
            if (folder == null) throw new ArgumentNullException(nameof(folder));
            _folders.Remove(folder);
        }

        public Folder CreateFolder(string name)
        {
            return new Folder(this, name);
        }

        public void DeleteFolder(Folder folder)
        {
            if (folder == null) throw new ArgumentNullException(nameof(folder));
            if (!_folders.Contains(folder))
                throw new InvalidOperationException("Folder does not belong to this user.");

            _folders.Remove(folder);
        }

        public void DeleteAllFolders()
        {
            foreach (var folder in _folders.ToList())
            {
                _folders.Remove(folder);
            }
        }

        private readonly HashSet<Text> _mentionedInTexts = new();
        public IReadOnlyCollection<Text> MentionedInTexts => _mentionedInTexts.ToList().AsReadOnly();

        internal void AddMentionedInText(Text text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            _mentionedInTexts.Add(text);
        }

        internal void RemoveMentionedInText(Text text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            _mentionedInTexts.Remove(text);
        }

        // Reverse Connection: A User knows about the messages they sent
        private readonly List<Message> _messages = new();
        
        public IReadOnlyList<Message> SentMessages => _messages.AsReadOnly();
        
        internal void AddMessage(Message message)
        {
            if (!_messages.Contains(message))
            {
                _messages.Add(message);
            }
        }
        
        internal void RemoveMessage(Message message)
        {
            if (_messages.Contains(message))
            {
                _messages.Remove(message);
            }
        }
    }
}
