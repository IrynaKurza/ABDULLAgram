namespace ABDULLAgram.Attachments
{
    public class Image
    {
        public string Resolution { get; set; }  
        public string Format { get; set; }
        public bool IsEdited { get; set; }
        public bool IsMarked { get; set; }
        public List<Image> Images { get; set; } = new();
    }
}