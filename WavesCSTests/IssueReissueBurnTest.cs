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
            node.WaitForTransactionConfirmation(asset.Id);

            var quantityIssue = node.GetBalance(Accounts.Alice.Address, asset);
            string response = node.ReissueAsset(Accounts.Alice, asset, 1, true);
            Assert.IsNotNull(response);
            node.WaitForTransactionBroadcastResponseConfirmation(response);

            var quantityReissue = node.GetBalance(Accounts.Alice.Address, asset);
            Assert.AreNotEqual(quantityIssue, quantityReissue);
            Assert.AreEqual(quantityReissue, 3);

            response = node.BurnAsset(Accounts.Alice, asset, 3);
            Assert.IsNotNull(response);
            node.WaitForTransactionBroadcastResponseConfirmation(response);

            var quantityBurn = node.GetBalance(Accounts.Alice.Address, asset);
            Assert.AreEqual(quantityBurn, 0);
        }
    }
}
