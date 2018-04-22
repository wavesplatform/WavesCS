using System;
using WavesCS;
using System.Web.Script.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCSTests
{
    [TestClass]
    public class TransactionTest
    {
        private static readonly long AMOUNT = 100000000L;
        private static readonly long FEE = 100000;

        private static JavaScriptSerializer serializer = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
        
        public TestContext TestContext { get; set; }
        

        [TestMethod]
        public void SmokeTest()
        {
            // doesn't validate transactions, just checks that all methods run to completion, no buffer overflows occur etc
            var account = PrivateKeyAccount.CreateFromPrivateKey("CMLwxbMZJMztyTJ6Zkos66cgU7DybfFJfyJtTVpme54t", AddressEncoding.TestNet);
            var recipient = "3N9gDFq8tKFhBDBTQxR3zqvtpXjw5wW3syA";
            var assetId = "AssetAssetAssetAssetAssetAssetAs";
            var transactionId = "TransactionTransactionTransactio";

            Dump("alias", Transactions.MakeAliasTransaction(account, "daphnie", AddressEncoding.TestNet, FEE));
            Dump("burn", Transactions.MakeBurnTransaction(account, assetId, AMOUNT, FEE));
            Dump("issue", Transactions.MakeIssueTransaction(account, "Pure Gold", "Gold backed asset", AMOUNT, 8, true, FEE));
            Dump("reissue", Transactions.MakeReissueTransaction(account, assetId, AMOUNT, false, FEE));
            Dump("lease", Transactions.MakeLeaseTransaction(account, recipient, AMOUNT, FEE));
            Dump("lease cancel", Transactions.MakeLeaseCancelTransaction(account, transactionId, FEE));
            Dump("xfer", Transactions.MakeTransferTransaction(account, recipient, AMOUNT, null, FEE, null, "Shut up & take my money"));
        }

        private void Dump(String header, DictionaryObject transaction)
        {
            TestContext.WriteLine("*** " + header + " ***");
            TestContext.WriteLine("Transaction data: " + serializer.Serialize(transaction));
            TestContext.WriteLine("");
        }
    }
}

