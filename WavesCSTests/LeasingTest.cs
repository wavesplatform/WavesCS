using System;
using System.Runtime.Remoting.Lifetime;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS;

namespace WavesCSTests
{
    [TestClass]
    public class LeasingTest
    {
        public TestContext TestContext { get; set; }
        
        [TestMethod]
        public void Test()
        {
            var node = new Node();

            var leaseTx = new LeaseTransaction(Accounts.Bob.PublicKey, Accounts.Alice.Address, 0.5m);            
            Assert.AreEqual(0.001m, leaseTx.Fee);
            leaseTx.Sign(Accounts.Bob);                        
            var response = node.Broadcast(leaseTx.GetJsonWithSignature());
            Console.WriteLine(response);
            Assert.IsFalse(string.IsNullOrEmpty(response));

            var leasingId = response.GetJsonObject().GetString("id");
            
            Thread.Sleep(10000);
            
            var cancelTx = new CancelLeasingTransaction(Accounts.Bob.PublicKey, leasingId);            
            Assert.AreEqual(0.001m, cancelTx.Fee);
            cancelTx.Sign(Accounts.Bob);  
            Console.WriteLine(cancelTx.GetJsonWithSignature());
            response = node.Broadcast(cancelTx.GetJsonWithSignature());            
            Assert.IsFalse(string.IsNullOrEmpty(response));
            Console.WriteLine(response);
        }
    }
}