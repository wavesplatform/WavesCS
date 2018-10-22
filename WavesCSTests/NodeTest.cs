using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS;

namespace WavesCSTests
{
    [TestClass]
    public class NodeTest
    {        
        private static readonly Asset WBTC = new Asset("Fmg13HEHJHuZYbtJq8Da8wifJENq8uBxDuWoP9pVe2Qe", "WBTC", 8);

        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }        
        
        [TestMethod]
        public void TestGetters()
        {
            var node = new Node();
            Assert.IsTrue(node.GetHeight() > 0);
            Assert.IsTrue(node.GetBalance(Accounts.Bob.Address) >= 0);
            Assert.IsTrue(node.GetBalance(Accounts.Bob.Address, 100) >= 0);
            Assert.IsTrue(node.GetBalance(Accounts.Bob.Address, WBTC) >= 0);
            Assert.IsTrue(node.GetUnconfirmedPoolSize() >= 0);            
        }

        [TestMethod]
        public void TestGetAsset()
        {
            var node = new Node(Node.MainNetHost);
            var assetId = "725Yv9oceWsB4GsYwyy4A52kEwyVrL5avubkeChSnL46";

            var asset = node.GetAsset(assetId);

            Assert.AreEqual(assetId, asset.Id);
            Assert.AreEqual("EFYT", asset.Name);
            Assert.AreEqual(8, asset.Decimals);
            
            Assert.AreEqual(200000, asset.AmountToLong(0.002m));
            Assert.AreEqual(0.03m, asset.LongToAmount(3000000));
        }
        
        [TestMethod]
        public void TestAssetBalances()
        {
            Http.Tracing = false;
            
            var node = new Node(Node.MainNetHost);
 
            var portfolio = node.GetAssetBalances("3PPF1JfQLJLVd6v4ewmuDbjDLcxBCUe5GSu");
            
            Assert.IsTrue(portfolio.Count > 0);

            foreach (var pair in portfolio)
            {
                Console.WriteLine("Asset: {0}, balance: {1}", pair.Key.Name, pair.Value);
            }
        }

        [TestMethod]
        public void TestGetTransactionsByAddress()
        {
            var node = new Node();
            var transactions = node.GetTransactionsByAddress(Accounts.Alice.Address, 10);
            
            Assert.IsTrue(transactions.Count() == 10);
            Assert.IsTrue(transactions.All(t => t.GetInt("type") < 20));
            Assert.IsTrue(transactions.All(t => t.GetString("sender").Length > 30));
        }

        [TestMethod]
        public void TestTransfer()
        {
            var node = new Node();
            
            string transactionId = node.Transfer(Accounts.Alice, Accounts.Bob.Address, Assets.WAVES, 0.2m, "Hi Bob!");
            Assert.IsNotNull(transactionId);

            // transfer back so that Alice's balance is not drained
            transactionId = node.Transfer(Accounts.Bob, Accounts.Alice.Address, Assets.WAVES, 0.2m, "Thanks, Alice");
            Assert.IsNotNull(transactionId);
        }        

        [TestMethod]
        public void BatchBroadcastTest()
        {
            var node = new Node();

            var transactons = new[]
            {
                new TransferTransaction(Accounts.Alice.PublicKey, Accounts.Bob.Address, Assets.WAVES, 0.3m).Sign(Accounts.Alice),
                new TransferTransaction(Accounts.Bob.PublicKey, Accounts.Alice.Address, Assets.WAVES, 0.3m).Sign(Accounts.Bob),
            };
            
            var result = node.BatchBroadcast(transactons);
            Assert.IsNotNull(result);
        }
    }
}
