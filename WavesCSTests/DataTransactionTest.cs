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
            var node = new Node();                        
            
            var account = PrivateKeyAccount.CreateFromSeed("general rose scissors hybrid clutch method era habit client caught toward actress pilot infant theme", AddressEncoding.TestNet);

            var data = new Dictionary<string, object>
            {
                { "test long", 1001L },
                { "test true", true },
                { "test false", false },
                { "test bytes", new byte[] { 1, 2, 3, 4, 5}}                
            };

            var tx = Transactions.MakeDataTransaction(account, data, 100000);
            var txJson = tx.ToJson();
            TestContext.WriteLine(txJson);
            
            TestContext.WriteLine("Response tx id: " + node.Broadcast(tx));
                                  
        }
    }
}
