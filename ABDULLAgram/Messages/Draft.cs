using ABDULLAgram.Chats;
using ABDULLAgram.Users;

namespace ABDULLAgram.Messages
{
    [Serializable]
    public class Draft
    {
        private DateTime _lastSaveTimestamp;
        public DateTime LastSaveTimestamp
        {
            get => _lastSaveTimestamp;
            set
            {
                if (value > DateTime.Now)
                    throw new ArgumentOutOfRangeException(nameof(LastSaveTimestamp), "LastSaveTimestamp cannot be in the future.");
                _lastSaveTimestamp = value;
            }
        }

        private string _id = Guid.NewGuid().ToString();
        public string Id 
        { 
            get => _id; 
            set 
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Id cannot be empty.");
                if (_extent.Any(d => d.Id == value && !ReferenceEquals(d, this)))
                    throw new InvalidOperationException("Draft Id must be unique.");
                _id = value;
            }
        }
        
        public void SendMessage(Message message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            
            if (message.Draft != this)
                throw new InvalidOperationException("This draft does not belong to the provided message.");

            // 1. Create Sent component (Automatic creation)
            var now = DateTime.Now;
            var sent = new Sent(now, now, null, null);

            // 2. Set reference on Message (Transition state)
            // This will assign _sent and set _draft to null in the Message
            message.PromoteToSent(sent);

            // 3. Delete Draft from extent (Cleanup)
            this.Delete();
        }
        private static readonly List<Draft> _extent = new();
        public static IReadOnlyCollection<Draft> GetAll() => _extent.AsReadOnly();
        private void AddToExtent() => _extent.Add(this);
        public static void ClearExtent() => _extent.Clear();
        public static void ReAdd(Draft d)
        {
            if (_extent.Any(x => x.Id == d.Id && !ReferenceEquals(x, d)))
                throw new InvalidOperationException("Duplicate Id found during load of Drafts.");
            _extent.Add(d);
        }

        public void Delete()
        {
            _extent.Remove(this);
        }
        
        public Draft(DateTime lastSaveTimestamp)
        {
            LastSaveTimestamp = lastSaveTimestamp;
            AddToExtent();
        }

        private Draft() { } // XML
    }
}