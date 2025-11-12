namespace ABDULLAgram.Messages
{
    public abstract class Message
    {
        public abstract string Id { get; set; }
        public abstract long MessageSize { get; } // read-only, derived in children
        public const long MaximumSize = 10L * 1024 * 1024 * 1024; //10GB
    }
}