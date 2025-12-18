namespace ABDULLAgram.Users
{
    public class PremiumUserBehavior : IUserTypeBehavior
    {
        public UserType Type => UserType.Premium;

        public DateTime PremiumStartDate { get; private set; }
        public DateTime PremiumEndDate { get; private set; }

        public int MaxSavedStickerpacks => int.MaxValue;

        public PremiumUserBehavior(DateTime start, DateTime end)
        {
            if (end <= start)
                throw new ArgumentException("Premium end date must be after start date.");

            PremiumStartDate = start;
            PremiumEndDate = end;
        }

        public int CalculateDaysUntilDue()
        {
            return Math.Max(0, (PremiumEndDate - DateTime.Now).Days);
        }

        public void CancelSubscription()
        {
            PremiumEndDate = DateTime.Now;
        }

        public void ValidateOnAttach(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
        }
    }
}