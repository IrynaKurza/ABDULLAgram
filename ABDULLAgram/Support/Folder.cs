using ABDULLAgram.Chats;
using ABDULLAgram.Users;

namespace ABDULLAgram.Support
{
    [Serializable]
    public class Folder
    {
        // ============================================================
        // COMPOSITION: User owns Folder (1 user → 0..* folders)
        // Folder CANNOT exist without a User
        // When Folder is deleted, it's removed from EXTENT (truly deleted)
        // When User is deleted, all their folders are deleted
        // ============================================================
        
        private readonly User _owner;
        
        // ============================================================
        // CLASS EXTENT PATTERN
        // Maintains collection of all Folder instances in memory
        // ============================================================
        
        private static readonly List<Folder> Extent = new();
        public static IReadOnlyCollection<Folder> GetAll() => Extent.AsReadOnly();
        private void AddToExtent() => Extent.Add(this);
        public static void ClearExtent() => Extent.Clear();
        
        // Parameterless constructor for XML serialization
        private Folder() { }

        // INTERNAL CONSTRUCTOR: Only User can create folders via CreateFolder()
        // This enforces composition - folders are always created WITH an owner
        // Factory pattern ensures folder never exists without owner
        internal Folder(User owner, string name)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Name = name;  // Uses property setter for validation
            AddToExtent();
            
            // REVERSE CONNECTION: Register this folder with the owner immediately
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
        // Also remove from owner's collection
        // ============================================================
        
        public void Delete()
        {
            // Break aggregation links (Folder ↔ Chat)
            foreach (var chat in _chats.ToList())
            {
                chat.RemoveFromFolderInternal(this);
            }
            _chats.Clear();

            // Remove from extent
            Extent.Remove(this);

            // Remove from owner's collection (composition)
            _owner.RemoveFolderInternal(this);
        }

        
        // ============================================================
        // AGGREGATION: Folder ↔ Chat (many-to-many)
        // Folder CONTAINS chats, but does NOT own them
        // ============================================================

        private readonly HashSet<Chat> _chats = new();
        public IReadOnlyCollection<Chat> Chats => _chats.ToList().AsReadOnly();

        public const int MaxChatsPerFolder = 100;
        
        public void AddChat(Chat chat)
        {
            if (chat == null)
                throw new ArgumentNullException(nameof(chat));

            if (_chats.Contains(chat))
                return;

            if (_chats.Count >= MaxChatsPerFolder)
                throw new InvalidOperationException("A folder cannot contain more than 100 chats.");

            _chats.Add(chat);

            // REVERSE CONNECTION (internal)
            chat.AddToFolderInternal(this);
        }

        public void RemoveChat(Chat chat)
        {
            if (chat == null)
                throw new ArgumentNullException(nameof(chat));

            if (!_chats.Contains(chat))
                return;

            _chats.Remove(chat);

            // REVERSE CONNECTION (internal)
            chat.RemoveFromFolderInternal(this);
        }

    }
}