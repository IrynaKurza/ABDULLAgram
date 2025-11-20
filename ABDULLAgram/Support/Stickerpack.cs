namespace ABDULLAgram.Support
{
    public class Stickerpack
    {
        private string _name = "";
        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Stickerpack Name cannot be empty.");
                _name = value;
            }
        }

        public bool IsPremium { get; set; }
    }
}