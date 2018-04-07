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
        private static readonly String WBTC = "Fmg13HEHJHuZYbtJq8Da8wifJENq8uBxDuWoP9pVe2Qe";

        private static readonly PrivateKeyAccount alice = PrivateKeyAccount.CreateFromPrivateKey("CMLwxbMZJMztyTJ6Zkos66cgU7DybfFJfyJtTVpme54t", AddressEncoding.TestNet);
        private static readonly PrivateKeyAccount bob = PrivateKeyAccount.CreateFromPrivateKey("25Um7fKYkySZnweUEVAn9RLtxN5xHRd7iqpqYSMNQEeT", AddressEncoding.TestNet);
        
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestGetters()
        {
            var node = new Node();
            Assert.IsTrue(node.GetHeight() > 0);
//            Assert.IsTrue(node.GetBalance(bob.Address) >= 0);
//            Assert.IsTrue(node.GetBalance(bob.Address, 100) >= 0);
//            Assert.IsTrue(node.GetBalance(bob.Address, WBTC) >= 0);
        }

        [TestMethod]
        public void TestTransfer()
        {
            var node = new Node();
            string transactionId = node.Transfer(alice, bob.Address, AMOUNT, FEE, "Hi Bob!");
            Assert.IsNotNull(transactionId);

            // transfer back so that Alice's balance is not drained
            transactionId = node.Transfer(bob, alice.Address, AMOUNT, FEE, "Thanks, Alice");
            Assert.IsNotNull(transactionId);
        }               

        [TestMethod]
        public void TestMatcher()
        {
            var matcher = new Node("https://testnet1.wavesnodes.com");
            string matcherKey = "CRxqEuxhdZBEHX42MU4FfyJxuHmbDBTaHMhM3Uki7pLw";
            long timestamp = Utils.CurrentTimestamp();

            var orderBook = matcher.GetOrderBook(null, WBTC);
            Assert.IsNotNull(orderBook);            

            string orderId = matcher.CreateOrder(alice, matcherKey, "", WBTC,
               new Order("sell").Type, 1, 100000000, timestamp + 3600000, 500000);
            Assert.IsNotNull(orderId);

            string status = matcher.GetOrderStatus(orderId, "", WBTC);
            Assert.AreEqual("Accepted", status);
         
            matcher.CancelOrder(alice, "", WBTC, orderId, 400000);
        }
    }
}
