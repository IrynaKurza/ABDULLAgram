namespace ABDULLAgram.Attachments
{
    [Serializable]
    public class Text : Messages.Message
    {
        // Text-specific attributes
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

        // Override Id to add uniqueness check (like Regular does with PhoneNumber)
        public override string Id
        {
            get => base.Id;
            set
            {
                bool exists = _extent.Any(t => !ReferenceEquals(t, this) && t.Id == value);
                if (exists)
                    throw new InvalidOperationException("Text Id must be unique among all Text messages.");
                
                base.Id = value; // Calls parent validation
            }
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
            SetSize(textContent.Length);

            AddToExtent();
        }

        private Text() { } // for XML serialization
    }
}