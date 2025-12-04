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
        private HashSet<User> _savedByUsers = new HashSet<User>();
        public IReadOnlyCollection<User> SavedByUsers => _savedByUsers.ToList().AsReadOnly();

        // ==== PUBLIC METHOD: Called from outside ====
        public void AddSavedByUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User cannot be null.");

            if (_savedByUsers.Contains(user))
                throw new InvalidOperationException("This user has already saved this stickerpack.");

            _savedByUsers.Add(user);
            user.AddStickerpackInternal(this);
        }

        // Remove user from saved by list
        public void RemoveSavedByUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User cannot be null.");

            if (!_savedByUsers.Contains(user))
                throw new InvalidOperationException("This user has not saved this stickerpack.");

            _savedByUsers.Remove(user);
            user.RemoveStickerpackInternal(this);
        }

        // ==== INTERNAL METHODS: Called by User class only ====
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