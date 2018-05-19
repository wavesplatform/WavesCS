using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS;

namespace WavesCSTests
{
    [TestClass]
    public class NodeTest
    {
        private static readonly long AMOUNT = 100000000L;
        private static readonly long FEE = 100000;
        private static readonly string WBTC = "Fmg13HEHJHuZYbtJq8Da8wifJENq8uBxDuWoP9pVe2Qe";
        
        
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            Api.DataProcessed += s => TestContext.WriteLine(s);
        }
        
        
        [TestMethod]
        public void TestGetters()
        {
            var node = new Node();
            Assert.IsTrue(node.GetHeight() > 0);
            Assert.IsTrue(node.GetBalance(Accounts.Bob.Address) >= 0);
            Assert.IsTrue(node.GetBalance(Accounts.Bob.Address, 100) >= 0);
            Assert.IsTrue(node.GetBalance(Accounts.Bob.Address, WBTC) >= 0);
        }

        [TestMethod]
        public void TestTransfer()
        {
            var node = new Node();
            
            string transactionId = node.Transfer(Accounts.Alice, Accounts.Bob.Address, AMOUNT, FEE, "Hi Bob!");
            Assert.IsNotNull(transactionId);

            // transfer back so that Alice's balance is not drained
            transactionId = node.Transfer(Accounts.Bob, Accounts.Alice.Address, AMOUNT, FEE, "Thanks, Alice");
            Assert.IsNotNull(transactionId);
        }               

    }
}
