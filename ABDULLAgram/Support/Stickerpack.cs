using ABDULLAgram.Attachments;

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

        // Aggregation
        private List<Sticker> _stickers = new();

        public void AddSticker(Sticker sticker)
        {
            if (_stickers.Count >= 50)
                throw new InvalidOperationException("Stickerpack cannot have more than 50 stickers.");
            if (_stickers.Contains(sticker)) return;
            sticker.BelongsToPack?.RemoveSticker(sticker);
            _stickers.Add(sticker);
            sticker.BelongsToPack = this;
        }

        public void RemoveSticker(Sticker sticker)
        {
            if (!_stickers.Contains(sticker)) return;
            if (_stickers.Count <= 1)
                throw new InvalidOperationException("Stickerpack must have at least 1 sticker.");
            _stickers.Remove(sticker);
            sticker.BelongsToPack = null;
        }

        public IReadOnlyCollection<Sticker> GetStickers() => _stickers.AsReadOnly();
    }
}