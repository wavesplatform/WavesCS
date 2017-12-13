using System;
using WavesCS.Main;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS;

namespace WavesCSTests
{
    [TestClass]
    public class NodeTest
    {
        private static readonly long AMOUNT = 1_00000000L;
        private static readonly long FEE = 100_000;
        private static readonly String WBTC = "Fmg13HEHJHuZYbtJq8Da8wifJENq8uBxDuWoP9pVe2Qe";

        private static readonly PrivateKeyAccount alice =
            new PrivateKeyAccount("CMLwxbMZJMztyTJ6Zkos66cgU7DybfFJfyJtTVpme54t", Account.TESTNET);
        private static readonly PrivateKeyAccount bob =
            new PrivateKeyAccount("25Um7fKYkySZnweUEVAn9RLtxN5xHRd7iqpqYSMNQEeT", Account.TESTNET);

        [TestMethod]
        public void TestGetters()
        {
            Node node = new Node();
            Assert.IsTrue(node.GetHeight() > 0);
            Assert.IsTrue(node.GetBalance(bob.Address) >= 0);
            Assert.IsTrue(node.GetBalance(bob.Address, 100) >= 0);
            Assert.IsTrue(node.GetBalance(bob.Address, WBTC) >= 0);
        }

        [TestMethod]
        public void TestTransfer()
        {
            Node node = new Node();
            String transactionId = node.Transfer(alice, bob.Address, AMOUNT, FEE, "Hi Bob!");
            Assert.IsNotNull(transactionId);

            // transfer back so that Alice's balance is not drained
            transactionId = node.Transfer(bob, alice.Address, AMOUNT, FEE, "Thanks, Alice");
            Assert.IsNotNull(transactionId);
        }

        [TestMethod]
        public void TestMatcher()
        {
            Node matcher = new Node("https://testnode2.wavesnodes.com");
            String matcherKey = "4oP8SPd7LiUo8xsokSTiyZjwg4rojdyXqWEq7NTwWsSU";
            long timestamp = Utils.CurrentTimestamp();

            OrderBook orders = matcher.GetOrderBook(null, WBTC);
            Assert.IsNotNull(orders);            

            String orderId = matcher.CreateOrder(alice, matcherKey, "", WBTC,
               new Order("sell").type, 1, 1_00000000, timestamp + 3_600_000, 500_000);
            Assert.IsNotNull(orderId);

            String status = matcher.GetOrderStatus(orderId, "", WBTC);
            Assert.AreEqual("Accepted", status);
            matcher.CancelOrder(alice, "", WBTC, orderId, 400_000);
        }
    }
}
