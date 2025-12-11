using ABDULLAgram.Users;

namespace ABDULLAgram.Support
{
    [Serializable]
    public class Folder
    {
        // ============================================================
        // COMPOSITION: User owns Folder (1 user → 0..* folders)
        // Strong ownership: Folder CANNOT exist without a User
        // When Folder is deleted, it's removed from EXTENT (truly deleted)
        // When User is deleted, all their folders are deleted
        // ============================================================
        
        private readonly User _owner;
        
        // ============================================================
        // CLASS EXTENT PATTERN
        // Maintains collection of all Folder instances in memory
        // ============================================================
        
        private static readonly List<Folder> _extent = new();
        public static IReadOnlyCollection<Folder> GetAll() => _extent.AsReadOnly();
        private void AddToExtent() => _extent.Add(this);
        public static void ClearExtent() => _extent.Clear();
        
        // Parameterless constructor for XML serialization
        private Folder() { }

        // INTERNAL CONSTRUCTOR: Only User can create folders via CreateFolder()
        // This enforces composition - folders are always created WITH an owner
        // Factory pattern ensures folder never exists without owner
        internal Folder(User owner, string name)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Name = name;  // Uses property setter for validation
            
            // Add to extent (folder now exists in system)
            AddToExtent();
            
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

        // ============================================================
        // DELETE METHOD
        // COMPOSITION: When folder is deleted, remove from EXTENT
        // Also remove from owner's collection (bidirectional cleanup)
        // ============================================================
        
        public void Delete()
        {
            // Remove from extent (TRUE deletion - folder no longer exists)
            _extent.Remove(this);
            
            // REVERSE CONNECTION: Remove from owner's collection
            // This maintains bidirectional consistency
            if (_owner != null)
            {
                _owner.RemoveFolderInternal(this);
            }
        }
    }
}