using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            //var issueTx = new IssueTransaction(Accounts.Alice.PublicKey, "testAsset3", "asset for c# issue testing", 100, 8, false);
         

            //issueTx.Sign(Accounts.Alice);
            //var jsonobj = issueTx.GetJson().ToString();
            //string response = node.Broadcast(issueTx.GetJsonWithSignature());
            //Console.WriteLine(response);
            //Assert.IsFalse(string.IsNullOrEmpty(response));

            //var txId = response.ParseJsonObject().GetString("id");

            Asset asset = Assets.GetById("3TUubhCKKxwTKgYRM4SrwPGYafyAp4Z4ZK98wyY4XWCt", node);
            Decimal minimalFeeInAssets = 100000;
            var sponsoredTx = new SponsoredFeeTransaction(Accounts.Alice.PublicKey, asset, minimalFeeInAssets);
            sponsoredTx.Sign(Accounts.Alice);
            var responseTwo = node.Broadcast(sponsoredTx.GetJsonWithSignature());

        }
    }
}
