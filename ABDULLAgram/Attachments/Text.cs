namespace ABDULLAgram.Attachments
{
    public class Text
    {
        public int MaximumLength { get; set; } = 2000;
        public int ContainsLink { get; set; }
        public string TextContent { get; set; }
        public int Length => TextContent?.Length ?? 0;
    }
}