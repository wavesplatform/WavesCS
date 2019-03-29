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
            var node = new Node(Node.TestNetChainId);

            Asset asset = null;
            try
            {
                asset = node.GetAsset("HkNSgxYpBLkzLb2vGYFFDrRT3gD5aoUnFV9eFav5DWpB");

                if (node.GetBalance(Accounts.Alice.Address, asset) < 0.2001m)
                    throw new Exception();
            }
            catch (Exception)
            {
                asset = node.IssueAsset(Accounts.Alice, "testAsset", "asset for c# issue testing", 2, 6, true);
                Assert.IsNotNull(asset);
                node.WaitForTransactionConfirmation(asset.Id);
            }

            var minimalFeeInAssets = 0.0001m;
            string response = node.SponsoredFeeForAsset(Accounts.Alice, asset, minimalFeeInAssets);
            Assert.IsNotNull(response);
            node.WaitForTransactionBroadcastResponseConfirmation(response);

            var amount = 0.2m;

            response = node.Transfer(Accounts.Alice, Accounts.Bob.Address, asset, amount, 0.0001m, asset);
            node.WaitForTransactionBroadcastResponseConfirmation(response);

            var transactionId = response.ParseJsonObject().GetString("id");
            var txInfo = node.GetObject("transactions/info/{0}", transactionId);
            Assert.AreEqual(asset.Id, txInfo["assetId"]);
            Assert.AreEqual(asset.Id, txInfo["feeAssetId"]);
        }     
    }
}
