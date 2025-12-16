using ABDULLAgram.Users;

namespace ABDULLAgram.Chats
{
    public class Group : Chat
    {
        private int _maxParticipants = 100;
        public int MaxParticipants
        {
            get => _maxParticipants;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(MaxParticipants), "MaxParticipants must be greater than zero.");
                _maxParticipants = value;
            }
        }

        private string _description = "";
        public string Description
        {
            get => _description;
            set
            {
                // Description can be empty, but shouldn't be null
                if (value is null)
                    throw new ArgumentNullException(nameof(Description), "Description cannot be null.");
                _description = value;
            }
        }
        
        // ============================================================
        // BASIC ASSOCIATION: User (1) — admin of — Group (0..*)
        // Each Group has exactly ONE admin
        // ============================================================

        private User? _admin;
        public User Admin
        {
            get => _admin ?? throw new InvalidOperationException("Group must have an admin.");
        }

        public void SetAdmin(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (_admin == user)
                return;

            // Remove reverse connection from old admin
            _admin?.RemoveAdminOfGroupInternal(this);

            _admin = user;

            // Reverse connection (internal)
            user.AddAdminOfGroupInternal(this);
        }
        
        public void KickMember(User requester, string phoneNumber)
        {
            if (requester == null)
                throw new ArgumentNullException(nameof(requester));

            if (_admin != requester)
                throw new InvalidOperationException("Only admin can kick members.");

            RemoveMember(phoneNumber);
        }

    }
}