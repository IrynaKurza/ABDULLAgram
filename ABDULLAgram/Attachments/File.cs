using ABDULLAgram.Chats;
using ABDULLAgram.Users;

namespace ABDULLAgram.Attachments
{
    [Serializable]
    public class File : Messages.Message
    {
        // File-specific attributes
        private string _fileName = "";
        public string FileName
        {
            get => _fileName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("FileName cannot be empty.");
                _fileName = value;
            }
        }

        private string _fileExtension = "";
        public string FileExtension
        {
            get => _fileExtension;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("FileExtension cannot be empty.");
                _fileExtension = value;
            }
        }

        public bool IsEncrypted { get; set; }

        // Override Id to add uniqueness check (like Regular does with PhoneNumber)
        public override string Id
        {
            get => base.Id;
            set
            {
                bool exists = _extent.Any(f => !ReferenceEquals(f, this) && f.Id == value);
                if (exists)
                    throw new InvalidOperationException("File Id must be unique among all File messages.");
                
                base.Id = value; // Calls parent validation
            }
        }

        // Class Extent
        private static readonly List<File> _extent = new();
        public static IReadOnlyCollection<File> GetAll() => _extent.AsReadOnly();
        private void AddToExtent() => _extent.Add(this);
        public static void ClearExtent() => _extent.Clear();
        public static void ReAdd(File f)
        {
            if (_extent.Any(x => x.Id == f.Id && !ReferenceEquals(x, f)))
                throw new InvalidOperationException("Duplicate Id found during load of File messages.");
            _extent.Add(f);
        }

        // Constructors
        public File(User sender, Chat chat, string fileName, string fileExtension, bool isEncrypted)
            : base(sender, chat)
        {
            FileName = fileName;
            FileExtension = fileExtension;
            IsEncrypted = isEncrypted;
            SetSize(0);

            AddToExtent();
        }

        private File() { } // for XML serialization
        protected override void RemoveFromExtent()
        {
            _extent.Remove(this);
        }
    }
}