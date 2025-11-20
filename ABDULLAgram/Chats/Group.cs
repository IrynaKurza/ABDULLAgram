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
    }
}