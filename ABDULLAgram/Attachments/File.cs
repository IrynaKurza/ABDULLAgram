using System;
using System.Collections.Generic;
using System.Linq;
using ABDULLAgram.Messages;

namespace ABDULLAgram.Attachments
{
    [Serializable]
    public class File
    {
        // File attributes
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

        // Message attributes
        private string _id = Guid.NewGuid().ToString();
        public string Id
        {
            get => _id;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Id cannot be empty.");

                bool exists = _extent.Any(f => !ReferenceEquals(f, this) && f.Id == value);
                if (exists)
                    throw new InvalidOperationException("File Id must be unique among all File messages.");

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
        public File(string fileName, string fileExtension, bool isEncrypted)
        {
            FileName = fileName;
            FileExtension = fileExtension;
            IsEncrypted = isEncrypted;
            SetSize(0);

            AddToExtent();
        }

        private File() { } // for XML serialization
    }
}