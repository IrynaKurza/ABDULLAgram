using ABDULLAgram.Attachments;
using ABDULLAgram.Support;
using ABDULLAgram.Users;

namespace ABDULLAgram.Tests.Associations.Qualified
{
    [TestFixture]
    public class StickerpackAggregationTests
    {
        [SetUp]
        public void Setup() => Sticker.ClearExtent();
        
        private class TestUser : Regular
        {
            public TestUser(string name)
                : base(name, "+" + name.GetHashCode(), true, 1)
            {
            }
        }


        // TEST: Add stickers to pack
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

        // TEST: Move stickers between packs
        [Test]
        public void AddSticker_MovesStickerBetweenPacks()
        {
            var ownerA = new TestUser("Owner");
            var packA = new Stickerpack("PackA", ownerA) { IsPremium = false };
            var ownerB = new TestUser("Owner");
            var packB = new Stickerpack("PackB", ownerB) { IsPremium = false };
            var s1 = new Sticker("😀", Sticker.BackgroundTypeEnum.Transparent);
            var s2 = new Sticker("😎", Sticker.BackgroundTypeEnum.Filled);

            packA.AddSticker(s1);
            packA.AddSticker(s2);
            packB.AddSticker(s1);  // Move s1 to packB

            Assert.That(packA.GetStickers().Count, Is.EqualTo(1));
            Assert.That(packB.GetStickers().Contains(s1), Is.True);
            Assert.That(s1.BelongsToPack, Is.EqualTo(packB));
        }

        // TEST: Max 50 stickers per pack
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

        // TEST: Pack must have at least 1 sticker
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
        
        // TEST: QUALIFIED - Get sticker by emojiCode (O(1) lookup)
        [Test]
        public void GetStickerByEmojiCode_ReturnsCorrectSticker()
        {
            var owner = new TestUser("Owner");
            var pack = new Stickerpack("Emojis", owner) { IsPremium = false };
            var sticker1 = new Sticker("😀", Sticker.BackgroundTypeEnum.Transparent);
            var sticker2 = new Sticker("😎", Sticker.BackgroundTypeEnum.Filled);
            pack.AddSticker(sticker1);
            pack.AddSticker(sticker2);

            // Act - O(1) lookup!
            var found = pack.GetStickerByEmojiCode("😎");

            // Assert
            Assert.That(found, Is.EqualTo(sticker2));
        }

        // TEST: QUALIFIED - Can't have duplicate emojiCodes
        [Test]
        public void AddSticker_DuplicateEmojiCode_DoesNotAddTwice()
        {
            var owner = new TestUser("Owner");
            var pack = new Stickerpack("Emojis", owner) { IsPremium = false };
            var sticker1 = new Sticker("😀", Sticker.BackgroundTypeEnum.Transparent);
            var sticker2 = new Sticker("😀", Sticker.BackgroundTypeEnum.Filled);  // Same emoji
            
            pack.AddSticker(sticker1);
            pack.AddSticker(sticker2);  // Should not add (same emojiCode)

            // Only first one added
            Assert.That(pack.GetStickers().Count, Is.EqualTo(1));
        }

        // TEST: Remove sticker removes from pack and clears reference
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