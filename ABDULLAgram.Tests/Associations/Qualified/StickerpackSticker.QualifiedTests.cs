using ABDULLAgram.Attachments;
using ABDULLAgram.Support;
using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Associations.Qualified
{
    [TestFixture]
    public class StickerpackAggregationTests
    {
        [SetUp]
        public void Setup()
        {
            Sticker.ClearExtent();
            User.ClearExtent();
        }
        
        private class TestUser : User
        {
            public TestUser(string name)
                : base(name, "+" + name.GetHashCode(), true)
            {
                InitializeAsRegular(1);
            }
        }

        [Test]
        public void AddSticker_AddsToPackWithEmojiCode()
        {
            var owner = new TestUser("Owner");
            var pack = new Stickerpack("PackA", owner) { IsPremium = false };
            var sticker = new Sticker("😀", Sticker.BackgroundTypeEnum.Transparent);
            
            pack.AddSticker(sticker);

            Assert.That(pack.GetStickers().Count, Is.EqualTo(1));
            Assert.That(pack.GetStickers().Contains(sticker), Is.True);
            Assert.That(sticker.BelongsToPack, Is.EqualTo(pack));
            Assert.That(sticker.EmojiCode, Is.EqualTo("😀"));
        }

        [Test]
        public void AddSticker_MovesStickerBetweenPacks()
        {
            var ownerA = new TestUser("OwnerA");
            var packA = new Stickerpack("PackA", ownerA) { IsPremium = false };
            var ownerB = new TestUser("OwnerB");
            var packB = new Stickerpack("PackB", ownerB) { IsPremium = false };
            var s1 = new Sticker("😀", Sticker.BackgroundTypeEnum.Transparent);
            var s2 = new Sticker("😎", Sticker.BackgroundTypeEnum.Filled);

            packA.AddSticker(s1);
            packA.AddSticker(s2);
            packB.AddSticker(s1);

            Assert.That(packA.GetStickers().Count, Is.EqualTo(1));
            Assert.That(packB.GetStickers().Contains(s1), Is.True);
            Assert.That(s1.BelongsToPack, Is.EqualTo(packB));
        }

        [Test]
        public void AddSticker_Over50_ThrowsException()
        {
            var owner = new TestUser("Owner");
            var pack = new Stickerpack("BigPack", owner) { IsPremium = false };
            
            for (int i = 0; i < 50; i++)
                pack.AddSticker(new Sticker($"emoji_{i}", Sticker.BackgroundTypeEnum.Transparent));

            Assert.Throws<InvalidOperationException>(() =>
                pack.AddSticker(new Sticker("emoji_51", Sticker.BackgroundTypeEnum.Filled)));
        }

        [Test]
        public void RemoveSticker_LastOne_ThrowsException()
        {
            var owner = new TestUser("Owner");
            var pack = new Stickerpack("SinglePack", owner) { IsPremium = false };
            var sticker = new Sticker("😀", Sticker.BackgroundTypeEnum.Transparent);
            pack.AddSticker(sticker);

            Assert.Throws<InvalidOperationException>(() => 
                pack.RemoveSticker("😀"));
        }
        
        [Test]
        public void GetStickerByEmojiCode_ReturnsCorrectSticker()
        {
            var owner = new TestUser("Owner");
            var pack = new Stickerpack("Emojis", owner) { IsPremium = false };
            var sticker1 = new Sticker("😀", Sticker.BackgroundTypeEnum.Transparent);
            var sticker2 = new Sticker("😎", Sticker.BackgroundTypeEnum.Filled);
            pack.AddSticker(sticker1);
            pack.AddSticker(sticker2);

            var found = pack.GetStickerByEmojiCode("😎");

            Assert.That(found, Is.EqualTo(sticker2));
        }

        [Test]
        public void AddSticker_DuplicateEmojiCode_DoesNotAddTwice()
        {
            var owner = new TestUser("Owner");
            var pack = new Stickerpack("Emojis", owner) { IsPremium = false };
            var sticker1 = new Sticker("😀", Sticker.BackgroundTypeEnum.Transparent);
            var sticker2 = new Sticker("😀", Sticker.BackgroundTypeEnum.Filled);
            
            pack.AddSticker(sticker1);
            pack.AddSticker(sticker2);

            Assert.That(pack.GetStickers().Count, Is.EqualTo(1));
        }

        [Test]
        public void RemoveSticker_RemovesFromPackAndClearsReference()
        {
            var owner = new TestUser("Owner");
            var pack = new Stickerpack("Pack", owner) { IsPremium = false };
            var sticker1 = new Sticker("😀", Sticker.BackgroundTypeEnum.Transparent);
            var sticker2 = new Sticker("😎", Sticker.BackgroundTypeEnum.Filled);
            pack.AddSticker(sticker1);
            pack.AddSticker(sticker2);

            pack.RemoveSticker("😀");

            Assert.That(pack.GetStickers().Count, Is.EqualTo(1));
            Assert.That(pack.GetStickerByEmojiCode("😀"), Is.Null);
        }
    }
}