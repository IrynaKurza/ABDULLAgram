using System.Xml.Serialization;
using ABDULLAgram.Messages;

namespace ABDULLAgram.Chats
{
    [XmlInclude(typeof(Group))]
    [XmlInclude(typeof(Private))]
    [Serializable]
    public abstract class Chat
    {
        private string _name = "";
        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Chat Name cannot be empty.");
                _name = value;
            }
        }

        private DateTime _createdAt;
        public DateTime CreatedAt
        {
            get => _createdAt;
            set
            {
                if (value > DateTime.Now)
                    throw new ArgumentOutOfRangeException(nameof(CreatedAt), "CreatedAt cannot be in the future.");
                _createdAt = value;
            }
        }

        protected Chat() 
        {
            _createdAt = DateTime.Now;
        }
        
        // 1. Reverse Connection: A Chat knows its history
        private readonly List<Message> _history = new();

        // This represents the {History} association constraint
        public IReadOnlyList<Message> History => _history.AsReadOnly();

        internal void AddMessage(Message message)
        {
            if (!_history.Contains(message))
            {
                _history.Add(message);
            }
        }

        internal void RemoveMessage(Message message)
        {
            if (_history.Contains(message))
            {
                _history.Remove(message);
            }
        }
    }
}