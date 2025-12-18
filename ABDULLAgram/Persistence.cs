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
        // Note: Sent and Draft are now states within Message (composition pattern)
        // Messages are stored via their concrete types (Text, Image, Video, etc.)
        public List<Image> Images { get; set; } = new();
        public List<Text> Texts { get; set; } = new();
        public List<Video> Videos { get; set; } = new();
        public List<ABDULLAgram.Attachments.File> Files { get; set; } = new();
        public List<Sticker> Stickers { get; set; } = new();
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
                Images   = new List<Image>(Image.GetAll()),
                Texts    = new List<Text>(Text.GetAll()),
                Videos   = new List<Video>(Video.GetAll()),
                Files    = new List<ABDULLAgram.Attachments.File>(ABDULLAgram.Attachments.File.GetAll()),
                Stickers = new List<Sticker>(Sticker.GetAll())
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
                Image.ClearExtent();
                Text.ClearExtent();
                Video.ClearExtent();
                ABDULLAgram.Attachments.File.ClearExtent();
                Sticker.ClearExtent();
                return false;
            }

            try
            {
                var ser = new XmlSerializer(typeof(DataSnapshot));
                using var fs = new FileStream(path, FileMode.Open);
                var snap = (DataSnapshot)ser.Deserialize(fs);

                Regular.ClearExtent();
                Premium.ClearExtent();
                Image.ClearExtent();
                Text.ClearExtent();
                Video.ClearExtent();
                ABDULLAgram.Attachments.File.ClearExtent();
                Sticker.ClearExtent();

                foreach (var r in snap.Regulars) Regular.ReAdd(r);
                foreach (var p in snap.Premiums) Premium.ReAdd(p);
                foreach (var i in snap.Images)   Image.ReAdd(i);
                foreach (var t in snap.Texts)    Text.ReAdd(t);
                foreach (var v in snap.Videos)   Video.ReAdd(v);
                foreach (var f in snap.Files)    ABDULLAgram.Attachments.File.ReAdd(f);
                foreach (var st in snap.Stickers) Sticker.ReAdd(st);

                return true;
            }
            catch
            {
                Regular.ClearExtent();
                Premium.ClearExtent();
                Image.ClearExtent();
                Text.ClearExtent();
                Video.ClearExtent();
                ABDULLAgram.Attachments.File.ClearExtent();
                Sticker.ClearExtent();
                return false;
            }
        }
        
        public static void DeleteAll(string path = AllPath)
        {
            if (File.Exists(path)) File.Delete(path);
            Regular.ClearExtent();
            Premium.ClearExtent();
            Image.ClearExtent();
            Text.ClearExtent();
            Video.ClearExtent();
            ABDULLAgram.Attachments.File.ClearExtent();
            Sticker.ClearExtent();
        }
    }
}