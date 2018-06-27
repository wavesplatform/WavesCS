using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            var node = new Node();
            var issueTx = new IssueTransaction(Accounts.Alice.PublicKey, "testAsset", "asset for c# issue testing", 2, 6, false);
            issueTx.Sign(Accounts.Alice);
            string response = node.Broadcast(issueTx.GetJsonWithSignature()); 
            Assert.IsFalse(string.IsNullOrEmpty(response));

            Thread.Sleep(10000);

            string assetId = response.ParseJsonObject().GetString("assetId");
            Asset asset = Assets.GetById(assetId, node);
            var minimalFeeInAssets = 0.0001m;
            var sponsoredTx = new SponsoredFeeTransaction(Accounts.Alice.PublicKey, asset, minimalFeeInAssets);
            sponsoredTx.Sign(Accounts.Alice);
            var responseTwo = node.Broadcast(sponsoredTx.GetJsonWithSignature());
            Assert.IsFalse(string.IsNullOrEmpty(responseTwo));

        }
    }
}
