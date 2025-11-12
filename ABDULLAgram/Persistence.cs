using System.Xml.Serialization;
using ABDULLAgram.Users;
using ABDULLAgram.Attachments;
using ABDULLAgram.Messages;
using File = System.IO.File;

namespace ABDULLAgram
{
    [Serializable]
    public class DataSnapshot
    {
        public List<Regular> Regulars { get; set; } = new();
        public List<Premium> Premiums { get; set; } = new();
        public List<Sent> Sents { get; set; } = new();
        public List<Draft> Drafts { get; set; } = new();
        public List<Image> Images { get; set; } = new();
    }

    public static class Persistence
    {
        private const string AllPath = "abdullagram_all.xml";

        public static void SaveAll(string path = AllPath)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                throw new DirectoryNotFoundException($"Directory does not exist: {dir}");
            
            var snap = new DataSnapshot
            {
                Regulars = new List<Regular>(Regular.GetAll()),
                Premiums = new List<Premium>(Premium.GetAll()),
                Sents    = new List<Sent>(Sent.GetAll()),
                Drafts   = new List<Draft>(Draft.GetAll()),
                Images   = new List<Image>(Image.GetAll())
            };

            var serializer = new XmlSerializer(typeof(DataSnapshot));
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            serializer.Serialize(fs, snap);
        }

        public static bool LoadAll(string path = AllPath)
        {
            if (!File.Exists(path))
            {
                Regular.ClearExtent();
                Premium.ClearExtent();
                Sent.ClearExtent();
                Draft.ClearExtent();
                Image.ClearExtent();
                return false;
            }

            try
            {
                var ser = new XmlSerializer(typeof(DataSnapshot));
                using var fs = new FileStream(path, FileMode.Open);
                var snap = (DataSnapshot)ser.Deserialize(fs);

                Regular.ClearExtent();
                Premium.ClearExtent();
                Sent.ClearExtent();
                Draft.ClearExtent();
                Image.ClearExtent();

                foreach (var r in snap.Regulars) Regular.ReAdd(r);
                foreach (var p in snap.Premiums) Premium.ReAdd(p);
                foreach (var s in snap.Sents)    Sent.ReAdd(s);
                foreach (var d in snap.Drafts)   Draft.ReAdd(d);
                foreach (var i in snap.Images)   Image.ReAdd(i);

                return true;
            }
            catch
            {
                Regular.ClearExtent();
                Premium.ClearExtent();
                Sent.ClearExtent();
                Draft.ClearExtent();
                Image.ClearExtent();
                return false;
            }
        }
        
        public static void DeleteAll(string path = AllPath)
        {
            if (File.Exists(path)) File.Delete(path);
            Regular.ClearExtent();
            Premium.ClearExtent();
            Sent.ClearExtent();
            Draft.ClearExtent();
            Image.ClearExtent();
        }
    }
}