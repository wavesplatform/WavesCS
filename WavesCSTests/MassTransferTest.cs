using System.Collections.Generic;
using WavesCS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static WavesCS.Transactions;

namespace WavesCSTests
{
    [TestClass]
    public class MassTransferTest
    {
        public TestContext TestContext { get; set; }
        private static readonly long AMOUNT = 100000000L;
        private static readonly long FEE = 200000;


        [TestMethod]
        public void TestMassTransferTransaction()
        {
            var node = new Node();

            var account = PrivateKeyAccount.CreateFromPrivateKey("CMLwxbMZJMztyTJ6Zkos66cgU7DybfFJfyJtTVpme54t", AddressEncoding.TestNet);
            var recipientA = "3N9gDFq8tKFhBDBTQxR3zqvtpXjw5wW3syA";
            var recipientB = "3N9gDFq8tKFhBDBTQxR3zqvtpXjw5wW3syA";

            List<MassTransferRecipient> recipients = new List<MassTransferRecipient>();
            recipients.Add(new MassTransferRecipient(recipientA, AMOUNT));
            recipients.Add(new MassTransferRecipient(recipientB, AMOUNT));

            var tx = Transactions.MakeMassTransferTransaction(account, recipients, null, FEE, null, "Shut up & take my money");

            var txJson = tx.ToJson();
            TestContext.WriteLine(txJson);

            var txRx = node.Broadcast(tx);
            TestContext.WriteLine("Response tx id: " + txRx);
                                  
        }
    }
}
