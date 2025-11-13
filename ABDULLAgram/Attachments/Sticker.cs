using System;
using System.Collections.Generic;
using System.Linq;
using ABDULLAgram.Messages;

namespace ABDULLAgram.Attachments
{
    [Serializable]
    public class Sticker
    {
        public enum BackgroundTypeEnum { Transparent, Filled }

        // Sticker attributes
        public BackgroundTypeEnum BackgroundType { get; set; }

        // Message attributes
        private string _id = Guid.NewGuid().ToString();
        public string Id
        {
            get => _id;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Id cannot be empty.");

                bool exists = _extent.Any(s => !ReferenceEquals(s, this) && s.Id == value);
                if (exists)
                    throw new InvalidOperationException("Sticker Id must be unique among all Sticker messages.");

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
        private static readonly List<Sticker> _extent = new();
        public static IReadOnlyCollection<Sticker> GetAll() => _extent.AsReadOnly();
        private void AddToExtent() => _extent.Add(this);
        public static void ClearExtent() => _extent.Clear();
        public static void ReAdd(Sticker s)
        {
            if (_extent.Any(x => x.Id == s.Id && !ReferenceEquals(x, s)))
                throw new InvalidOperationException("Duplicate Id found during load of Sticker messages.");
            _extent.Add(s);
        }

        // Constructors
        public Sticker(BackgroundTypeEnum backgroundType)
        {
            BackgroundType = backgroundType;
            SetSize(0);

            AddToExtent();
        }

        private Sticker() { } // for XML serialization
    }
}