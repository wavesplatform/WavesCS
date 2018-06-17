﻿using System;
using System.IO;
using System.Text;
using WavesCS;
using Newtonsoft.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;
using System.Collections.Generic;

namespace WavesCSTests
{
    [TestClass]
    public class TransactionTest
    {
        private static readonly decimal AMOUNT = 105m;
        private static readonly decimal FEE = 0.001m;

        private static readonly JsonSerializer serializer = new JsonSerializer();
        
        public TestContext TestContext { get; set; }
        

        [TestMethod]
        public void SmokeTest()
        {
            // doesn't validate transactions, just checks that all methods run to completion, no buffer overflows occur etc
            var account = PrivateKeyAccount.CreateFromPrivateKey("CMLwxbMZJMztyTJ6Zkos66cgU7DybfFJfyJtTVpme54t", AddressEncoding.TestNet);
            var recipient = "3N9gDFq8tKFhBDBTQxR3zqvtpXjw5wW3syA";
            var asset = Assets.MONERO;
            var transactionId = "TransactionTransactionTransactio";

            var recipients = new List<MassTransferItem>
            {
                new MassTransferItem(recipient, AMOUNT),
                new MassTransferItem(recipient, AMOUNT)
            };

            Dump("alias", new AliasTransaction(account.PublicKey, DateTime.UtcNow, "daphnie", AddressEncoding.TestNet, FEE));
            Dump("burn", new BurnTransaction(account.PublicKey, DateTime.UtcNow, asset, AMOUNT, FEE));
            Dump("issue", new IssueTransaction(account.PublicKey, DateTime.UtcNow, "Pure Gold", "Gold backed asset", AMOUNT, 8, true, FEE));
            Dump("reissue", new ReissueTransaction(account.PublicKey, DateTime.UtcNow, asset, AMOUNT, false, FEE));
            Dump("lease", new LeaseTransaction(account.PublicKey, DateTime.UtcNow, recipient, AMOUNT, FEE));
            Dump("lease cancel", new CancelLeasingTransaction(account.PublicKey, DateTime.UtcNow, transactionId, FEE));
            Dump("xfer", new TransferTransaction(account.PublicKey, DateTime.UtcNow, recipient, asset, AMOUNT, "Shut up & take my money"));
            Dump("massxfer", new MassTransferTransaction(account.PublicKey, DateTime.UtcNow, asset, recipients, "Shut up & take my money", FEE));
        }

        private void Dump(String header, Transaction transaction)
        {
            TestContext.WriteLine("*** " + header + " ***");

            var sb = new StringBuilder();
            var sw = new StringWriter(sb);
            serializer.Serialize(sw, transaction);
            var json = sb.ToString();
            TestContext.WriteLine("Transaction data: " + json);

            TestContext.WriteLine("");
        }
    }
}

