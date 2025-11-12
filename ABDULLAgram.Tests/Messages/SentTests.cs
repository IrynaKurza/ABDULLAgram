namespace ABDULLAgram.Tests.Messages
{
    [TestFixture]
    public class SentValidationTests
    {
        [SetUp] 
        public void Setup() => ABDULLAgram.Messages.Sent.ClearExtent();

        [Test]
        public void Duplicate_Id_Throws()
        {
            var s1 = new ABDULLAgram.Messages.Sent(DateTime.Now, DateTime.Now, null, null);
            var s2 = new ABDULLAgram.Messages.Sent(DateTime.Now, DateTime.Now, null, null);
            
            Assert.Throws<InvalidOperationException>(() =>
                s2.Id = s1.Id);
        }
    }
}