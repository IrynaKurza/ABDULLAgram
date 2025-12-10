using ABDULLAgram.Chats;
using ABDULLAgram.Users;

namespace ABDULLAgram.Attachments
{
    [Serializable]
    public class Video : Messages.Message
    {
        // Video-specific attributes
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

        // Override Id to add uniqueness check (like Regular does with PhoneNumber)
        public override string Id
        {
            get => base.Id;
            set
            {
                bool exists = _extent.Any(v => !ReferenceEquals(v, this) && v.Id == value);
                if (exists)
                    throw new InvalidOperationException("Video Id must be unique among all Video messages.");
                
                base.Id = value; // Calls parent validation
            }
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
        public Video(User sender, Chat chat, string resolution, double durationSec, bool isStreamingOptimized)
            : base(sender, chat)
        {
            Resolution = resolution;
            DurationSec = durationSec;
            IsStreamingOptimized = isStreamingOptimized;
            SetSize(0);

            AddToExtent();
        }

        private Video() { } // for XML serialization
        protected override void RemoveFromExtent()
        {
            _extent.Remove(this);
        }
    }
}