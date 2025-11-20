namespace ABDULLAgram.Chats
{
    public class Private : Chat
    {
        private int _maxParticipants = 2;
        public int MaxParticipants
        {
            get => _maxParticipants;
            set
            {
                if (value != 2)
                    throw new ArgumentOutOfRangeException(nameof(MaxParticipants), "private chats must have exactly 2 participants");
                _maxParticipants = value;
            }
        }
    }
}