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
            Api.Tracing = true;
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
        public void TestTransfer()
        {
            var node = new Node();
            
            string transactionId = node.Transfer(Accounts.Alice, Accounts.Bob.Address, Assets.WAVES, 0.2m, "Hi Bob!");
            Assert.IsNotNull(transactionId);

            // transfer back so that Alice's balance is not drained
            transactionId = node.Transfer(Accounts.Bob, Accounts.Alice.Address, Assets.WAVES, 0.2m, "Thanks, Alice");
            Assert.IsNotNull(transactionId);
        }
    }
}
