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
    public class IssueReissueBurnTest
    {     
        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }

        [TestMethod]
        public void TestIssueReissueBurnTransactions()
        {
            var node = new Node();          

            Asset asset = node.IssueAsset(Accounts.Alice, "testAsset", "asset for c# issue testing", 2, 6, true);
            Assert.IsNotNull(asset);         
            
            Thread.Sleep(15000);
            var quantityIssue = node.GetBalance(Accounts.Alice.Address, asset);
            string reissue = node.ReissueAsset(Accounts.Alice, asset, 1, true);
            Assert.IsNotNull(reissue);

            Thread.Sleep(20000);
            var quantityReissue = node.GetBalance(Accounts.Alice.Address, asset);
            Assert.AreNotEqual(quantityIssue, quantityReissue);
            Assert.AreEqual(quantityReissue, 3);

            string burn = node.BurnAsset(Accounts.Alice, asset, 3);
            Assert.IsNotNull(burn);

            Thread.Sleep(20000);
            var quantityBurn = node.GetBalance(Accounts.Alice.Address, asset);
            Assert.AreEqual(quantityBurn, 0);

        }
    }
}
