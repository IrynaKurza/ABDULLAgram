using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
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
        public List<Sent> Sents { get; set; } = new();
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
                Sents    = new List<Sent>(Sent.GetAll()),
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
                Sent.ClearExtent();
                Image.ClearExtent();
                return false;
            }

            try
            {
                var ser = new XmlSerializer(typeof(DataSnapshot));
                using var fs = new FileStream(path, FileMode.Open);
                var snap = (DataSnapshot)ser.Deserialize(fs);

                Regular.ClearExtent();
                Sent.ClearExtent();
                Image.ClearExtent();

                foreach (var r in snap.Regulars) Regular.ReAdd(r);  // тут спрацює перевірка унікальності
                foreach (var s in snap.Sents)    Sent.ReAdd(s);
                foreach (var i in snap.Images)   Image.ReAdd(i);

                return true;
            }
            catch
            {
                Regular.ClearExtent();
                Sent.ClearExtent();
                Image.ClearExtent();
                return false;
            }
        }
        
        public static void DeleteAll(string path = AllPath)
        {
            if (File.Exists(path)) File.Delete(path);
            Regular.ClearExtent();
            Sent.ClearExtent();
            Image.ClearExtent();
        }
    }
}