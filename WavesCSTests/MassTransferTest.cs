using System.Collections.Generic;
using WavesCS;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace WavesCSTests
{
    [TestClass]
    public class MassTransferTest
    {
        public TestContext TestContext { get; set; }
        private static readonly long FEE = 200000;


        [TestMethod]
        public void TestMassTransferTransaction()
        {
            var node = new Node();
           
            var recipients = new List<Transactions.MassTransferRecipient>
            {
                new Transactions.MassTransferRecipient(Accounts.Alice.Address, 1000000),
                new Transactions.MassTransferRecipient(Accounts.Bob.Address, 2000000),                
            };

            var tx = Transactions.MakeMassTransferTransaction(Accounts.Alice, recipients, null, FEE, null, "Shut up & take my money");

            var txJson = tx.ToJson();
            TestContext.WriteLine(txJson);

            var txRx = node.Broadcast(tx);
            TestContext.WriteLine("Response tx id: " + txRx);                                  
        }
    }
}
