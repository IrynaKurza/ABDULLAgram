using ABDULLAgram.Users;

namespace ABDULLAgram.Support
{
    public class Stickerpack
    {
        private string _name = "";
        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Stickerpack Name cannot be empty.");
                _name = value;
            }
        }

        public bool IsPremium { get; set; }

        // ==== BASIC ASSOCIATION ====
        // This stickerpack knows which users saved it
        private HashSet<User> _savedByUsers = new HashSet<User>();
        
        // Public getter returns read-only view
        public IReadOnlyCollection<User> SavedByUsers => _savedByUsers.ToList().AsReadOnly();

        // ==== PUBLIC METHOD: Called from outside ====
        // Use this when you want to add a user from the Stickerpack side
        public void AddSavedByUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User cannot be null.");

            if (_savedByUsers.Contains(user))
                throw new InvalidOperationException("This user has already saved this stickerpack.");

            // Add user to our collection
            _savedByUsers.Add(user);

            // REVERSE CONNECTION: Tell the user about this pack
            user.AddStickerpackInternal(this);
        }

        // Remove user from saved by list
        public void RemoveSavedByUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User cannot be null.");

            if (!_savedByUsers.Contains(user))
                throw new InvalidOperationException("This user has not saved this stickerpack.");

            // Remove from our collection
            _savedByUsers.Remove(user);

            // REVERSE CONNECTION: Tell the user to forget this pack
            user.RemoveStickerpackInternal(this);
        }

        // ==== INTERNAL METHODS: Called by User class only ====
        // These just update the collection - no validation, no callbacks
        internal void AddSavedByUserInternal(User user)
        {
            _savedByUsers.Add(user);
        }

        internal void RemoveSavedByUserInternal(User user)
        {
            _savedByUsers.Remove(user);
        }
    }
}