using System;
using System.Collections.Generic;
using System.Linq;
using ABDULLAgram.Support;

namespace ABDULLAgram.Users
{
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
    }
}