namespace ABDULLAgram.Attachments
{
   [Serializable]
    public class Resolution
    {
        public int Width { get; set; }
        public int Height { get; set; }

        private Resolution() { } // for XML
        
        public Resolution(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException("Resolution must be positive.");
            Width = width; Height = height;
        }
    }

    [Serializable]
    public class Image 
    {
        // Image attributes
        private Resolution _resolution = new(1,1);
        public Resolution Resolution
        {
            get => _resolution;
            set => _resolution = value ?? throw new ArgumentNullException(nameof(Resolution));
        }

        private string _format = "png";
        public string Format
        {
            get => _format;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Format cannot be empty.");
                _format = value;
            }
        }

        public bool IsEdited { get; set; }
        public bool IsMarked { get; set; }

        // Multi-Value
        private readonly List<string> _variants = new();
        public IReadOnlyList<string> Variants => _variants.AsReadOnly();

        
        // Message attributes 
        private string _id = Guid.NewGuid().ToString();
        public string Id
        {
            get => _id;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Id cannot be empty.");

                bool exists = _extent.Any(i => !ReferenceEquals(i, this) && i.Id == value);
                if (exists)
                    throw new InvalidOperationException("Image Id must be unique among all Image messages.");

                _id = value;
            }
        }

        public const long MaxSizeBytes = 10L * 1024 * 1024 * 1024;

        private long _messageSize;
        public long MessageSize => _messageSize;

        private void SetSize(long bytes)
        {
            if (bytes < 0) throw new ArgumentOutOfRangeException(nameof(bytes));
            if (bytes > MaxSizeBytes) throw new ArgumentOutOfRangeException(nameof(bytes), "Message size cannot exceed 10GB.");
            _messageSize = bytes;
        }

        public void SetVariants(IEnumerable<string> variants)
        {
            if (variants is null) throw new ArgumentNullException(nameof(variants));
            var list = variants.Select(v =>
            {
                if (string.IsNullOrWhiteSpace(v))
                    throw new ArgumentException("Variant id cannot be empty.");
                return v;
            }).ToList();

            if (list.Count < 1 || list.Count > 10)
                throw new ArgumentOutOfRangeException(nameof(variants), "Variants count must be between 1 and 10.");

            _variants.Clear();
            _variants.AddRange(list);
        }

        public void AddVariant(string variant)
        {
            if (string.IsNullOrWhiteSpace(variant))
                throw new ArgumentException("Variant id cannot be empty.");
            if (_variants.Count >= 10)
                throw new InvalidOperationException("Cannot add more than 10 variants.");
            _variants.Add(variant);
        }

        public bool RemoveVariant(string variant) => _variants.Remove(variant);

        // Class Extent
        private static readonly List<Image> _extent = new();
        public static IReadOnlyCollection<Image> GetAll() => _extent.AsReadOnly();
        private void AddToExtent() => _extent.Add(this);
        public static void ClearExtent() => _extent.Clear();
        public static void ReAdd(Image i)
        {
            if (_extent.Any(x => x.Id == i.Id && !ReferenceEquals(x, i)))
                throw new InvalidOperationException("Duplicate Id found during load of Images.");
            _extent.Add(i);
        }
        public Image(Resolution resolution, string format, IEnumerable<string> initialVariants)
        {
            Resolution = resolution;
            Format = format;
            SetVariants(initialVariants); 
            SetSize(0);
            
            AddToExtent();
        }

        private Image() { } // for XML serialization
    }
}