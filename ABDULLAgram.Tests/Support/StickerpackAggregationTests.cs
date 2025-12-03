using ABDULLAgram.Attachments;
using ABDULLAgram.Support;

namespace ABDULLAgram.Tests.Support
{
    [TestFixture]
    public class StickerpackAggregationTests
    {
        [SetUp]
        public void Setup() => Sticker.ClearExtent();

        [Test]
        public void AddSticker_MovesStickerBetweenPacks()
        {
            var packA = new Stickerpack { Name = "PackA" };
            var packB = new Stickerpack { Name = "PackB" };
            var s1 = new Sticker(Sticker.BackgroundTypeEnum.Transparent);
            var s2 = new Sticker(Sticker.BackgroundTypeEnum.Filled);

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
            var pack = new Stickerpack { Name = "BigPack" };
            for (int i = 0; i < 50; i++)
                pack.AddSticker(new Sticker(Sticker.BackgroundTypeEnum.Transparent));

            Assert.Throws<InvalidOperationException>(() =>
                pack.AddSticker(new Sticker(Sticker.BackgroundTypeEnum.Filled)));
        }

        [Test]
        public void RemoveSticker_LastOne_ThrowsException()
        {
            var pack = new Stickerpack { Name = "SinglePack" };
            var sticker = new Sticker(Sticker.BackgroundTypeEnum.Transparent);
            pack.AddSticker(sticker);

            Assert.Throws<InvalidOperationException>(() => pack.RemoveSticker(sticker));
        }
    }
}

