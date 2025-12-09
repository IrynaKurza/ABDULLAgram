using System;
using ABDULLAgram.Users;

namespace ABDULLAgram.Support
{
    [Serializable]
    public class Folder
    {
        private readonly User _owner;
        
        private Folder() { }

        internal Folder(User owner, string name)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Name = name;                      
            _owner.AddFolderInternal(this);  
        }

        public User Owner => _owner;

        private string _name = "";
        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Folder name cannot be empty.");
                _name = value;
            }
        }
    }
}