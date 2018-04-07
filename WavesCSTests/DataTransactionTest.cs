using System.Collections.Generic;
using WavesCS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WavesCSTests
{
    [TestClass]
    public class DataTransactionTest
    {
        public TestContext TestContext { get; set; }
        
        
        [TestMethod]
        public void TestDataTransaction()
        {
            var node = new Node("http://3.unblock.wavesnodes.com:6869");                        
            
            // 3NQLzWYQi22dX3djSFdAS2yKeuqHNM5vGSS
            var account = PrivateKeyAccount.CreateFromSeed("my seed", 'U');

            var data = new Dictionary<string, object>
            {
                { "test long", 1001L },
                { "test true", true },
                { "test false", false },
                { "test bytes", new byte[] { 1, 2, 3, 4, 5}}                
            };

            var tx = Transaction.MakeDataTransaction(account, data, 100000);
            
            TestContext.WriteLine(tx.Data.ToJson());
            
            TestContext.WriteLine("Response tx id: " + node.Broadcast(tx));
                                  
        }
    }
}
