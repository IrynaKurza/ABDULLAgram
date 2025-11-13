using System;
using System.Collections.Generic;
using System.Linq;
using ABDULLAgram.Messages;

namespace ABDULLAgram.Attachments
{
    [Serializable]
    public class Text
    {
        // Text attributes
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

        // Derived attribute
        public int Length => TextContent.Length;

        // Message attributes
        private string _id = Guid.NewGuid().ToString();
        public string Id
        {
            get => _id;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Id cannot be empty.");

                bool exists = _extent.Any(t => !ReferenceEquals(t, this) && t.Id == value);
                if (exists)
                    throw new InvalidOperationException("Text Id must be unique among all Text messages.");

                _id = value;
            }
        }

        private long _messageSize;
        public long MessageSize => _messageSize;

        private void SetSize(long bytes)
        {
            if (bytes < 0) throw new ArgumentOutOfRangeException(nameof(bytes));
            if (bytes > Message.MaximumSize) throw new ArgumentOutOfRangeException(nameof(bytes), "Message size cannot exceed 10GB.");
            _messageSize = bytes;
        }

        // Class Extent
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

        // Constructors
        public Text(string textContent, bool containsLink)
        {
            TextContent = textContent;
            ContainsLink = containsLink;
            SetSize(textContent.Length); // Approximate size based on character count

            AddToExtent();
        }

        private Text() { } // for XML serialization
    }
}