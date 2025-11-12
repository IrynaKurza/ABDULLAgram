namespace ABDULLAgram.Users
{
    public class Premium : User
    {
        public DateTime PremiumStartDate { get; set; }
        public DateTime PremiumEndDate { get; set; }
        public int DaysUntilNextPayment { get; set; }
        public int RemainingDays => (PremiumEndDate - DateTime.Now).Days;
    }
}