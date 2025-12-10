using ABDULLAgram.Users;

namespace ABDULLAgram.Support
{
    [Serializable]
    public class Folder
    {
        // ============================================================
        // COMPOSITION: User owns Folder (1 user → 0..* folders)
        // Strong ownership: Folder CANNOT exist without a User
        // When User is deleted, all Folders are deleted too
        // ============================================================
        
        private readonly User _owner;
        
        // Parameterless constructor for XML serialization
        private Folder() { }

        // INTERNAL CONSTRUCTOR: Only User can create folders via CreateFolder()
        // This enforces composition - folders are always created WITH an owner
        // Factory pattern ensures folder never exists without owner
        internal Folder(User owner, string name)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Name = name;  // Uses property setter for validation
            
            // REVERSE CONNECTION: Register this folder with the owner immediately
            // This establishes the bidirectional link
            _owner.AddFolderInternal(this);  
        }

        // Read-only property: Owner can never change (composition rule)
        // Once created with an owner, folder belongs to that owner forever
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