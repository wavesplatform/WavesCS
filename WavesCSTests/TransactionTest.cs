using System;
using WavesCS;
using System.Web.Script.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WavesCSTests
{
    [TestClass]
    public class TransactionTest
    {
        private static readonly long AMOUNT = 1_00000000L;
        private static readonly long FEE = 100_000;
        private static JavaScriptSerializer serializer = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };

        private TestContext testContextInstance;

        /// <summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        [TestMethod]
        public void SmokeTest()
        {
            // doesn't validate transactions, just checks that all methods run to completion, no buffer overflows occur etc
            PrivateKeyAccount account = PrivateKeyAccount.CreateFromPrivateKey("CMLwxbMZJMztyTJ6Zkos66cgU7DybfFJfyJtTVpme54t", AddressScheme.TestNet);
            String recipient = "3N9gDFq8tKFhBDBTQxR3zqvtpXjw5wW3syA";
            String assetId = "AssetAssetAssetAssetAssetAssetAs";
            String TransactionId = "TransactionTransactionTransactio";

            Dump("alias", Transaction.MakeAliasTransaction(account, "daphnie", AddressScheme.TestNet, FEE));
            Dump("burn", Transaction.MakeBurnTransaction(account, assetId, AMOUNT, FEE));
            Dump("issue", Transaction.MakeIssueTransaction(account, "Pure Gold", "Gold backed asset", AMOUNT, 8, true, FEE));
            Dump("reissue", Transaction.MakeReissueTransaction(account, assetId, AMOUNT, false, FEE));
            Dump("lease", Transaction.MakeLeaseTransaction(account, recipient, AMOUNT, FEE));
            Dump("lease cancel", Transaction.MakeLeaseCancelTransaction(account, TransactionId, FEE));
            Dump("xfer", Transaction.MakeTransferTransaction(account, recipient, AMOUNT, null, FEE, null, "Shut up & take my money"));
        }

        private void Dump(String header, Transaction transaction)
        {
            TestContext.WriteLine("*** " + header + " ***");
            TestContext.WriteLine("Transaction data: " + serializer.Serialize(transaction.Data));
            TestContext.WriteLine("");
        }
    }
}

