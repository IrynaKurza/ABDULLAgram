namespace ABDULLAgram.Users
{
    public interface IUserTypeBehavior
    {
        UserType Type { get; }

        int MaxSavedStickerpacks { get; }

        void ValidateOnAttach(User user);
    }
}