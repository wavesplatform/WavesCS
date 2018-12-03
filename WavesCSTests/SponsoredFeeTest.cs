using System;
using System.Threading;
using WavesCS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WavesCSTests
{
    [TestClass]
    public class SponsoredFeeTest
    {
        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }

        [TestMethod]
        public void TestSponsoredFeeTransaction()
        {
            var node = new Node(Node.MainNetHost, 'W');

            Asset asset = null;
            try
            {
                asset = Assets.GetById("HkNSgxYpBLkzLb2vGYFFDrRT3gD5aoUnFV9eFav5DWpB", node);

                if (node.GetBalance(Accounts.Alice.Address, asset) < 0.2001m)
                    throw new Exception();
            }
            catch (Exception)
            {
                asset = node.IssueAsset(Accounts.Alice, "testAsset", "asset for c# issue testing", 2, 6, true);
                Assert.IsNotNull(asset);

                Thread.Sleep(15000);
            }

            var minimalFeeInAssets = 0.0001m;
            string transaction = node.SponsoredFeeForAsset(Accounts.Alice, asset, minimalFeeInAssets);
            Assert.IsNotNull(transaction);

            Thread.Sleep(10000);

            var amount = 0.2m;
            var transactionId = node.Transfer(Accounts.Alice, Accounts.Bob.Address, asset, amount, 0.0001m, asset).ParseJsonObject().GetString("id");
            Thread.Sleep(10000);
            var txInfo = node.GetObject("transactions/info/{0}", transactionId);
            
            Assert.AreEqual(asset.Id, txInfo["assetId"]);
            Assert.AreEqual(asset.Id, txInfo["feeAssetId"]);
        }
    }
}
