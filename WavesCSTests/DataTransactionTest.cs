using System;
using System.Collections.Generic;
using WavesCS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS.Txs;

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
            
            var data = new Dictionary<string, object>
            {
                { "test long", 1001L },
                { "test true", true },
                { "test false", false },
                { "test bytes", new byte[] { 1, 2, 3, 4, 5}},
                { "test string", "Hello, Waves!"},
                { "test russian", "Привет" }                
            };

            var tx = new DataTransaction(Accounts.Alice.PublicKey, data);
            tx.Sign(Accounts.Alice);
            Console.WriteLine(tx.GetBody().Length);
            
            TestContext.WriteLine("Response tx id: " + node.Broadcast(tx.GetJsonWithSignature()));                                  
        }
    }
}
