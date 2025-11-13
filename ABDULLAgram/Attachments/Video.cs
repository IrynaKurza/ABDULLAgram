using System;
using System.Collections.Generic;
using System.Linq;
using ABDULLAgram.Messages;

namespace ABDULLAgram.Attachments
{
    [Serializable]
    public class Video
    {
        // Video attributes
        private string _resolution = "1920x1080";
        public string Resolution
        {
            get => _resolution;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Resolution cannot be empty.");
                _resolution = value;
            }
        }

        private double _durationSec;
        public double DurationSec
        {
            get => _durationSec;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(DurationSec), "Duration cannot be negative.");
                _durationSec = value;
            }
        }

        public bool IsStreamingOptimized { get; set; }

        // Message attributes
        private string _id = Guid.NewGuid().ToString();
        public string Id
        {
            get => _id;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Id cannot be empty.");

                bool exists = _extent.Any(v => !ReferenceEquals(v, this) && v.Id == value);
                if (exists)
                    throw new InvalidOperationException("Video Id must be unique among all Video messages.");

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
        private static readonly List<Video> _extent = new();
        public static IReadOnlyCollection<Video> GetAll() => _extent.AsReadOnly();
        private void AddToExtent() => _extent.Add(this);
        public static void ClearExtent() => _extent.Clear();
        public static void ReAdd(Video v)
        {
            if (_extent.Any(x => x.Id == v.Id && !ReferenceEquals(x, v)))
                throw new InvalidOperationException("Duplicate Id found during load of Video messages.");
            _extent.Add(v);
        }

        // Constructors
        public Video(string resolution, double durationSec, bool isStreamingOptimized)
        {
            Resolution = resolution;
            DurationSec = durationSec;
            IsStreamingOptimized = isStreamingOptimized;
            SetSize(0);

            AddToExtent();
        }

        private Video() { } // for XML serialization
    }
}