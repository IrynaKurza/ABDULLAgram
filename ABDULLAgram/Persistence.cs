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
        public List<User> Users { get; set; } = new();
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
                Users    = new List<User>(User.GetAll()),
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
                User.ClearExtent();
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

                User.ClearExtent();
                Image.ClearExtent();
                Text.ClearExtent();
                Video.ClearExtent();
                ABDULLAgram.Attachments.File.ClearExtent();
                Sticker.ClearExtent();

                foreach (var user in snap.Users)
                    User.ReAdd(user);
                foreach (var img in snap.Images)
                    Image.ReAdd(img);
                foreach (var txt in snap.Texts)
                    Text.ReAdd(txt);
                foreach (var vid in snap.Videos)
                    Video.ReAdd(vid);
                foreach (var f in snap.Files)
                    ABDULLAgram.Attachments.File.ReAdd(f);
                foreach (var sticker in snap.Stickers)
                    Sticker.ReAdd(sticker);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void DeleteAll(string path = AllPath)
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}