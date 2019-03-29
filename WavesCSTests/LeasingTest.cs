using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS;

namespace WavesCSTests
{
    [TestClass]
    public class LeasingTest
    {
        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }
        
        [TestMethod]
        public void TestLeasing()
        {
            var node = new Node();

            var leaseTx = new LeaseTransaction(node.ChainId, Accounts.Bob.PublicKey, Accounts.Alice.Address, 0.5m);            
            Assert.AreEqual(0.001m, leaseTx.Fee);
            leaseTx.Sign(Accounts.Bob);

            var response = node.BroadcastAndWait(leaseTx.GetJsonWithSignature());
            Console.WriteLine(response);
            Assert.IsFalse(string.IsNullOrEmpty(response));

            var leasingId = response.ParseJsonObject().GetString("id");
            
            var cancelTx = new CancelLeasingTransaction(node.ChainId, Accounts.Bob.PublicKey, leasingId);            
            Assert.AreEqual(0.001m, cancelTx.Fee);
            cancelTx.Sign(Accounts.Bob);  
            Console.WriteLine(cancelTx.GetJsonWithSignature());
            response = node.BroadcastAndWait(cancelTx.GetJsonWithSignature());            
            Assert.IsFalse(string.IsNullOrEmpty(response));
            Console.WriteLine(response);
        }
    }
}