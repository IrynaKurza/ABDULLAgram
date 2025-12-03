using System;
using System.Collections.Generic;
using System.Linq;
using ABDULLAgram.Chats;
using ABDULLAgram.Users;

namespace ABDULLAgram.Attachments
{
    [Serializable]
    public class Text : Messages.Message
    {
        public const int MaximumLength = 2000;

        public bool ContainsLink { get; set; }

        private string _text = "";
        public string TextContent
        {
            get => _text;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(TextContent));
                if (value.Length > MaximumLength)
                    throw new ArgumentOutOfRangeException(nameof(TextContent), $"Text cannot exceed {MaximumLength} characters.");
                _text = value;
            }
        }

        public int Length => TextContent.Length;

        public override string Id
        {
            get => base.Id;
            set
            {
                bool exists = _extent.Any(t => !ReferenceEquals(t, this) && t.Id == value);
                if (exists)
                    throw new InvalidOperationException("Text Id must be unique among all Text messages.");

                base.Id = value;
            }
        }

        private static readonly List<Text> _extent = new();
        public static IReadOnlyCollection<Text> GetAll() => _extent.AsReadOnly();
        private void AddToExtent() => _extent.Add(this);
        public static void ClearExtent() => _extent.Clear();
        public static void ReAdd(Text t)
        {
            if (_extent.Any(x => x.Id == t.Id && !ReferenceEquals(x, t)))
                throw new InvalidOperationException("Duplicate Id found during load of Text messages.");
            _extent.Add(t);
        }

        private readonly HashSet<User> _mentionedUsers = new();
        public IReadOnlyCollection<User> MentionedUsers => _mentionedUsers.ToList().AsReadOnly();

        public void AddMentionedUser(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (_mentionedUsers.Contains(user))
                throw new InvalidOperationException("User is already mentioned in this text.");

            _mentionedUsers.Add(user);
            user.AddMentionedInText(this);
        }

        public void RemoveMentionedUser(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (!_mentionedUsers.Contains(user))
                throw new InvalidOperationException("User is not mentioned in this text.");

            _mentionedUsers.Remove(user);
            user.RemoveMentionedInText(this);
        }
        
        // Constructors
        public Text(User sender, Chat chat, string textContent, bool containsLink)
            : base(sender, chat)
        {
            TextContent = textContent;
            ContainsLink = containsLink;
            SetSize(textContent.Length);

            AddToExtent();
        }

        private Text() { }
    }
}
