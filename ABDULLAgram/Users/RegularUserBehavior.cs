namespace ABDULLAgram.Users
{
    public class RegularUserBehavior : IUserTypeBehavior
    {
        public UserType Type => UserType.Regular;

        public int AdFrequency { get; private set; }

        public const int MaxStickerPacksSaved = 10;
        public int MaxSavedStickerpacks => MaxStickerPacksSaved;

        public RegularUserBehavior(int adFrequency)
        {
            if (adFrequency < 0)
                throw new ArgumentOutOfRangeException(nameof(adFrequency));

            AdFrequency = adFrequency;
        }

        public void ValidateOnAttach(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
        }
    }
}