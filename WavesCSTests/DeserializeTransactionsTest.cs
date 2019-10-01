using System.Linq;
using WavesCS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;



namespace WavesCSTests
{
    [TestClass]
    public class DeserializeTransactionsTest
    {

        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }

        [TestMethod]
        public void TestGetTransactions()
        {
            var node = new Node(Node.MainNetChainId);

            var limit = 100;
            var address = "3PBmsJXAcgnH9cu81oyW8abNh9jsaNzFQKJ";

            var transactions = node.GetTransactions(address, limit);
            Assert.AreEqual(transactions.Count(), limit);
        }

        [TestMethod]
        public void TestGetTransactionsOfType()
        {
            var node = new Node(Node.MainNetChainId);

            var limit = 10;
            var address = "3PBmsJXAcgnH9cu81oyW8abNh9jsaNzFQKJ";

            var issueTransactions = node.GetTransactions<IssueTransaction>(address, limit);
            Assert.AreEqual(issueTransactions.Count(), issueTransactions.OfType<IssueTransaction>().Count());

            var transferTransactions = node.GetTransactions<TransferTransaction>(address, limit);
            Assert.AreEqual(transferTransactions.Count(), transferTransactions.OfType<TransferTransaction>().Count());

            var reissueTransactions = node.GetTransactions<ReissueTransaction>(address, limit);
            Assert.AreEqual(reissueTransactions.Count(), reissueTransactions.OfType<ReissueTransaction>().Count());

            var burnTransactions = node.GetTransactions<BurnTransaction>(address, limit);
            Assert.AreEqual(burnTransactions.Count(), burnTransactions.OfType<BurnTransaction>().Count());

            var exchangeTransactions = node.GetTransactions<ExchangeTransaction>(address, limit);
            Assert.AreEqual(exchangeTransactions.Count(), exchangeTransactions.OfType<ExchangeTransaction>().Count());

            var leaseTransactions = node.GetTransactions<LeaseTransaction>(address, limit);
            Assert.AreEqual(leaseTransactions.Count(), leaseTransactions.OfType<LeaseTransaction>().Count());

            var leaseCancelTransactions = node.GetTransactions<CancelLeasingTransaction>(address, limit);
            Assert.AreEqual(leaseCancelTransactions.Count(), leaseCancelTransactions.OfType<CancelLeasingTransaction>().Count());

            var aliasTransactions = node.GetTransactions<AliasTransaction>(address, limit);
            Assert.AreEqual(aliasTransactions.Count(), aliasTransactions.OfType<AliasTransaction>().Count());

            var massTransferTransactions = node.GetTransactions<MassTransferTransaction>(address, limit);
            Assert.AreEqual(massTransferTransactions.Count(), massTransferTransactions.OfType<MassTransferTransaction>().Count());

            var dataTransactions = node.GetTransactions<DataTransaction>(address, limit);
            Assert.AreEqual(dataTransactions.Count(), dataTransactions.OfType<DataTransaction>().Count());

            var setScriptTransactions = node.GetTransactions<SetScriptTransaction>(address, limit);
            Assert.AreEqual(setScriptTransactions.Count(), setScriptTransactions.OfType<SetScriptTransaction>().Count());

            var sponsoredFeeTransactions = node.GetTransactions<SponsoredFeeTransaction>(address, limit);
            Assert.AreEqual(sponsoredFeeTransactions.Count(), sponsoredFeeTransactions.OfType<SponsoredFeeTransaction>().Count());
        }

        [TestMethod]
        public void TestIssueTransactionV1Deserialize()
        {
            var node = new Node(Node.MainNetChainId);

            var transactionId = "37nfgadHFw92hNqzyHFZXmGFo5Wmct6Eik1Y2AdYW1Aq";
            var tx = node.GetTransactionById(transactionId);

            Assert.IsInstanceOfType(tx, typeof(IssueTransaction));
            Assert.AreEqual(tx.GenerateId(), transactionId);

            var issueTx = (IssueTransaction)tx;
            Assert.AreEqual(issueTx.Sender, "3PKhW99rBCeQFurBibu7KMjYLf7GTRudYj7");
            Assert.AreEqual(issueTx.SenderPublicKey.ToBase58(), "7rSpgoCWeAPp3dKdBq5PExGJ63DM91MpDLHkdZqVzmVV");
            Assert.AreEqual(issueTx.Fee, Assets.WAVES.LongToAmount(100000000));
            Assert.AreEqual(issueTx.Timestamp.ToLong(), 1534264221840);
            Assert.AreEqual(issueTx.Proofs[0].ToBase58(), "FmMXxtjfYp2CPaT7gTjq7qkMrpagvkJcpbPtST7bjdozGCRGMzyFgryUPCwuV9wMWrLhW4fp6dn5i3TJ43KrcXE");
            Assert.AreEqual(issueTx.Asset.Id, "37nfgadHFw92hNqzyHFZXmGFo5Wmct6Eik1Y2AdYW1Aq");
            Assert.AreEqual(issueTx.Name, "MoX.");
            Assert.AreEqual(issueTx.Quantity, issueTx.Asset.LongToAmount(1840000000000000));
            Assert.IsTrue(issueTx.Reissuable);
            Assert.AreEqual(issueTx.Decimals, 8);
            Assert.AreEqual(issueTx.Description, "MoX is a fork of the anonymous currency Monero. Visit our site http://getmox.org");
        }

        [TestMethod]
        public void TestIssueTransactionV2Deserialize()
        {
            var node = new Node(Node.MainNetChainId);

            var transactionId = "5b3UGNZXX5srkHUQWnEWEiaVtnNWmg5aLo5uZMdjWupH";
            var tx = node.GetTransactionById(transactionId);

            Assert.IsInstanceOfType(tx, typeof(IssueTransaction));
            Assert.AreEqual(tx.GenerateId(), transactionId);

            var issueTx = (IssueTransaction)tx;
            Assert.AreEqual(issueTx.Sender, "3PBLabMJwedfZMXmp4CxhmKLBH5LXRHQ91i");
            Assert.AreEqual(issueTx.SenderPublicKey.ToBase58(), "ETZck8jSUHvcmPDhAeGJHVcD2mvzW4wsLCJdcXYd6x1t");
            Assert.AreEqual(issueTx.Fee, Assets.WAVES.LongToAmount(100000000));
            Assert.AreEqual(issueTx.Timestamp.ToLong(), 1548931510484);
            Assert.AreEqual(issueTx.Proofs[0].ToBase58(), "24E84jHcgaXpRf9Ua1QYxK1wuG2ZvmGRgacyKydQ4KJA6bJefSJsgLMjdxcEcPjKVHFwAntDQ6fmNkW6rVZ9Uu11");
            Assert.AreEqual(issueTx.Asset.Id, "5b3UGNZXX5srkHUQWnEWEiaVtnNWmg5aLo5uZMdjWupH");
            Assert.AreEqual(issueTx.Name, "Surge");
            Assert.AreEqual(issueTx.Quantity, issueTx.Asset.LongToAmount(1000000000000000));
            Assert.IsFalse(issueTx.Reissuable);
            Assert.AreEqual(issueTx.Decimals, 8);
            Assert.AreEqual(issueTx.Script.ToBase64(), "base64:AQQAAAALc3RhcnRIZWlnaHQAAAAAAAAU/UUEAAAACnN0YXJ0UHJpY2UAAAAAAAABhqAEAAAACGludGVydmFsCQAAaAAAAAIAAAAAAAAAABgAAAAAAAAAADwEAAAAA2V4cAkAAGgAAAACCQAAaAAAAAIAAAAAAAAAAGQAAAAAAAAAADwAAAAAAAAAA+gEAAAAByRtYXRjaDAFAAAAAnR4AwkAAAEAAAACBQAAAAckbWF0Y2gwAgAAABNFeGNoYW5nZVRyYW5zYWN0aW9uBAAAAAFlBQAAAAckbWF0Y2gwBAAAAARkYXlzCQAAaQAAAAIJAABlAAAAAgUAAAAGaGVpZ2h0BQAAAAtzdGFydEhlaWdodAUAAAAIaW50ZXJ2YWwDAwMJAABnAAAAAggFAAAAAWUAAAAFcHJpY2UJAABoAAAAAgUAAAAKc3RhcnRQcmljZQkAAGQAAAACAAAAAAAAAAABCQAAaAAAAAIFAAAABGRheXMFAAAABGRheXMJAQAAAAEhAAAAAQkBAAAACWlzRGVmaW5lZAAAAAEICAgFAAAAAWUAAAAJc2VsbE9yZGVyAAAACWFzc2V0UGFpcgAAAApwcmljZUFzc2V0BwkAAGcAAAACBQAAAANleHAJAABlAAAAAggIBQAAAAFlAAAACXNlbGxPcmRlcgAAAApleHBpcmF0aW9uCAgFAAAAAWUAAAAJc2VsbE9yZGVyAAAACXRpbWVzdGFtcAcJAABnAAAAAgUAAAADZXhwCQAAZQAAAAIICAUAAAABZQAAAAhidXlPcmRlcgAAAApleHBpcmF0aW9uCAgFAAAAAWUAAAAIYnV5T3JkZXIAAAAJdGltZXN0YW1wBwMDCQAAAQAAAAIFAAAAByRtYXRjaDACAAAAGVNldEFzc2V0U2NyaXB0VHJhbnNhY3Rpb24GCQAAAQAAAAIFAAAAByRtYXRjaDACAAAAD0J1cm5UcmFuc2FjdGlvbgQAAAACdHgFAAAAByRtYXRjaDAGB9CrDWk=");
            Assert.AreEqual(issueTx.Description, "Surge is a Smart Asset. Its price is increased every day: it is at least 0.001 * (1 + days * days) WAVES. DEX orders should have expiration less than 100 minutes.");
        }

        [TestMethod]
        public void TestTransferTransactionDeserialize()
        {
            var node = new Node(Node.MainNetChainId);

            var transactionId = "9jpwy6aYJRFnFWoArQxLywMSF8GRGyW42JT1KzHJD9sL";
            var tx = node.GetTransactionById(transactionId);

            Assert.IsInstanceOfType(tx, typeof(TransferTransaction));
            Assert.AreEqual(tx.GenerateId(), transactionId);

            var transferTx = (TransferTransaction)tx;
            Assert.AreEqual(transferTx.Sender, "3PBmsJXAcgnH9cu81oyW8abNh9jsaNzFQKJ");
            Assert.AreEqual(transferTx.SenderPublicKey.ToBase58(), "6gs6QPtujkQ6SbagvHMzXyGMjtS2UrseATxCnn84TDFC");
            Assert.AreEqual(transferTx.Fee, Assets.WAVES.LongToAmount(100000));
            Assert.AreEqual(transferTx.Timestamp.ToLong(), 1534497330297);
            Assert.AreEqual(transferTx.Proofs[0].ToBase58(), "47vBFvnu2jcYEUEvhVSJcsZcC1LmiDC31joS8PuftT2CpvrT5nm9gxjxuP4MFeogBgDhStzxKndQbh6P4ejaqtUs");
            Assert.AreEqual(transferTx.Recipient, "3PPsHUKZ8WLU7sLm9sV5Sb75RNKnMinqU58");

            Asset asset = node.GetAsset("4uK8i4ThRGbehENwa6MxyLtxAjAo1Rj9fduborGExarC");
            Assert.AreEqual(transferTx.Asset, asset);
            Assert.AreEqual(transferTx.Amount, asset.LongToAmount(1097948));

            Assert.AreEqual(transferTx.Attachment.ToBase58(), "");
        }

        [TestMethod]
        public void TestReissueTransactionDeserialize()
        {
            var node = new Node(Node.MainNetChainId);

            var transactionId = "HqtDBXsbz3ztNHheF3DstVKhsEYf3rtA31tPa784hiyx";
            var tx = node.GetTransactionById(transactionId);

            Assert.IsInstanceOfType(tx, typeof(ReissueTransaction));
            Assert.AreEqual(tx.GenerateId(), transactionId);

            var reissueTx = (ReissueTransaction)tx;
            Assert.AreEqual(reissueTx.Sender, "3PKhW99rBCeQFurBibu7KMjYLf7GTRudYj7");
            Assert.AreEqual(reissueTx.SenderPublicKey.ToBase58(), "7rSpgoCWeAPp3dKdBq5PExGJ63DM91MpDLHkdZqVzmVV");
            Assert.AreEqual(reissueTx.Fee, Assets.WAVES.LongToAmount(100000000));
            Assert.AreEqual(reissueTx.Timestamp.ToLong(), 1534335650023);
            Assert.AreEqual(reissueTx.Proofs[0].ToBase58(), "26U7gP4YRAB1YMR7nzSS2wc5CeWKMPWwep6xAFrpLRXhbAWkpszEgdZaFvjYdRGucLVioD1JzpvduHuuyM88fXPM");

            Asset asset = node.GetAsset("37nfgadHFw92hNqzyHFZXmGFo5Wmct6Eik1Y2AdYW1Aq");
            Assert.AreEqual(reissueTx.Asset, asset);
            Assert.AreEqual(reissueTx.Quantity, asset.LongToAmount(1838160000000000000));

            Assert.IsTrue(reissueTx.Reissuable);
        }

        [TestMethod]
        public void TestBurnTransactionDeserialize()
        {
            var node = new Node(Node.MainNetChainId);

            var transactionId = "HXhyvS9f5oQ18QAEeyRg6E9FHvAnLVWGSTGMyamgMe4n";
            var tx = node.GetTransactionById(transactionId);

            Assert.IsInstanceOfType(tx, typeof(BurnTransaction));
            Assert.AreEqual(tx.GenerateId(), transactionId);

            var burnTx = (BurnTransaction)tx;
            Assert.AreEqual(burnTx.Sender, "3PBmsJXAcgnH9cu81oyW8abNh9jsaNzFQKJ");
            Assert.AreEqual(burnTx.SenderPublicKey.ToBase58(), "6gs6QPtujkQ6SbagvHMzXyGMjtS2UrseATxCnn84TDFC");
            Assert.AreEqual(burnTx.Fee, Assets.WAVES.LongToAmount(100000));
            Assert.AreEqual(burnTx.Timestamp.ToLong(), 1534497330455);
            Assert.AreEqual(burnTx.Proofs[0].ToBase58(), "4UzYadVf4Gz9udZL5eZ1SHYVXF3XhjcCi46mBc6aP5zCuvxtS41sNUzBeTirggjVuU9P3cYzwNh1gDjDRQYZVY1t");

            Asset asset = node.GetAsset("4uK8i4ThRGbehENwa6MxyLtxAjAo1Rj9fduborGExarC");
            Assert.AreEqual(burnTx.Asset, asset);
            Assert.AreEqual(asset.AmountToLong(burnTx.Quantity), 274487);
        }

        [TestMethod]
        public void TestLeaseTransactionDeserialize()
        {
            var node = new Node(Node.MainNetChainId);

            var transactionId = "8feDmqySpSLJxfPYmnKCb99jf2g7oFGEY5Lu8DofqBCU";
            var tx = node.GetTransactionById(transactionId);

            Assert.IsInstanceOfType(tx, typeof(LeaseTransaction));
            Assert.AreEqual(tx.GenerateId(), transactionId);

            var leaseTx = (LeaseTransaction)tx;
            Assert.AreEqual(leaseTx.Sender, "3PNHKfDxU6PML1yHhU55gBn5jWkMvqFPYPP");
            Assert.AreEqual(leaseTx.SenderPublicKey.ToBase58(), "2hLjbCQT96cEynvXcRugwAqAnvxKLUscqonnUbYDcvmy");
            Assert.AreEqual(leaseTx.Fee, Assets.WAVES.LongToAmount(100000));
            Assert.AreEqual(leaseTx.Timestamp.ToLong(), 1534418507297);
            Assert.AreEqual(leaseTx.Proofs[0].ToBase58(), "2bPuQJncTh4eeARfB4PJ4L1tYJSAKkaNRyJhjfovSKjyUCNcaSEzqBNGY9r6MoEt4M1a1KwabKDa8xo6yhPf3t7L");
            Assert.AreEqual(Assets.WAVES.AmountToLong(leaseTx.Amount), 1658046538);
            Assert.AreEqual(leaseTx.Recipient, "3P23fi1qfVw6RVDn4CH2a5nNouEtWNQ4THs");
            Assert.IsTrue(leaseTx.IsActive);
        }

        [TestMethod]
        public void TestCancelLeasingTransactionDeserialize()
        {
            var node = new Node(Node.MainNetChainId);

            var transactionId = "8LkSyfgyuekjCherhjmhKR1gbYKJPAhfKSYLetSN93YW";
            var tx = node.GetTransactionById(transactionId);

            Assert.IsInstanceOfType(tx, typeof(CancelLeasingTransaction));
            Assert.AreEqual(tx.GenerateId(), transactionId);

            var cancelLeaseTx = (CancelLeasingTransaction)tx;
            Assert.AreEqual(cancelLeaseTx.Sender, "3PL3HscfDpAd6LFYYGcNKeUPg25tkpS4qeq");
            Assert.AreEqual(cancelLeaseTx.SenderPublicKey.ToBase58(), "2Gp8D5v9Edjmr2kEYQz6oN4JRQmzMFhLhKYLPD8ZSKRL");
            Assert.AreEqual(cancelLeaseTx.Fee, Assets.WAVES.LongToAmount(100000));
            Assert.AreEqual(cancelLeaseTx.Timestamp.ToLong(), 1534418379080);
            Assert.AreEqual(cancelLeaseTx.Proofs[0].ToBase58(), "5DDoFWGpKcQsuoMLHbGTEevzX1KtkPrvSgSZdxVqtZdqm2CnVJa3yVQAmFFErAzhZcxYEibESjp6jhMgjzsZV2cQ");
            Assert.AreEqual(cancelLeaseTx.LeaseId, "YjrfVmtCKo9P5K9EwK86kUFjtoEhaRjC4HpTf7hZeN8");
        }

        [TestMethod]
        public void TestAliasTransactionDeserialize()
        {
            var node = new Node(Node.MainNetChainId);

            var transactionId = "5JeRnELGEsT1bTZgbNETeJ6rVqhRvrbpMz82nh3qynpH";
            var tx = node.GetTransactionById(transactionId);

            Assert.IsInstanceOfType(tx, typeof(AliasTransaction));
            Assert.AreEqual(tx.GenerateId(), transactionId);

            var aliasIx = (AliasTransaction)tx;
            Assert.AreEqual(aliasIx.Sender, "3PPk7HZgyHqiQhpAgfGBjYnwHZ77D2kx5bL");
            Assert.AreEqual(aliasIx.SenderPublicKey.ToBase58(), "ACAHdSXAXzxnmn1oX8zBn6okKmojonLPU1SYD8hdZwki");
            Assert.AreEqual(aliasIx.Fee, Assets.WAVES.LongToAmount(100000));
            Assert.AreEqual(aliasIx.Timestamp.ToLong(), 1534490699982);
            Assert.AreEqual(aliasIx.Proofs[0].ToBase58(), "uTUCJbwAp8orFnT1g2MywBYZjCZDTGCTmFYN6aGWVzBbMVECRnF7zitB5mmLb8HfKiVogYHZ4iyjY2UJ3Jty9BM");
            Assert.AreEqual(aliasIx.Alias, "goldtoken");
        }

        [TestMethod]
        public void TestMassTransferTransactionDeserialize()
        {
            var node = new Node(Node.MainNetChainId);

            var transactionId = "EKPLV5vjsa2T8ijpjNNumikDrM1r6Yi6MXpNsbsdPM8i";
            var tx = node.GetTransactionById(transactionId);

            Assert.IsInstanceOfType(tx, typeof(MassTransferTransaction));
            Assert.AreEqual(tx.GenerateId(), transactionId);

            var massTransferTx = (MassTransferTransaction)tx;
            Assert.AreEqual(massTransferTx.Sender, "3P5CcDGigUuiHUaAstEj6Yv25xdnP4UQz1F");
            Assert.AreEqual(massTransferTx.SenderPublicKey.ToBase58(), "2i8drMSLgFTVSX3sitPsL6n98YXwsi8tGQbaMSrFrWMr");
            Assert.AreEqual(massTransferTx.Fee, Assets.WAVES.LongToAmount(4900000));
            Assert.AreEqual(massTransferTx.Timestamp.ToLong(), 1534263320261);
            Assert.AreEqual(massTransferTx.Proofs[0].ToBase58(), "V9U4CCkcn5Br73ZHGtgFbDWFF9fYjHFcxiynEgEci5s2WkNEVr4h7mDBB9hqLMvRfkLLLaF6KyNehguJuBMer2a");

            Asset asset = node.GetAsset("9GGTr8sRMbyb8wWi6dcJGDQR5qChdJxJqgzreMTAf716");
            Assert.AreEqual(massTransferTx.Asset, asset);

            Assert.AreEqual(massTransferTx.Attachment.ToBase58(), "eS1N");

            Assert.AreEqual(massTransferTx.Transfers.Count(), 96);
            Assert.AreEqual(massTransferTx.Transfers.Count(transfer => asset.AmountToLong(transfer.Amount) != 250033889900), 0);
            Assert.AreEqual(massTransferTx.Transfers.First().Recipient, "3PNiWsgFYxCoJCyeQsfop4sNcEbpRxrDBue");
            Assert.AreEqual(massTransferTx.Transfers.Last().Recipient, "3PKJYvRMS92vF2zBRoJrhkxj8gMBftUUi4d");
        }

        [TestMethod]
        public void TestDataTransactionDeserialize()
        {
            var node = new Node(Node.MainNetChainId);

            var transactionId = "BDBZWsarzShKoqmYUUiuFYZ3zewjvap6Laa7ctEutaP6";
            var tx = node.GetTransactionById(transactionId);

            Assert.IsInstanceOfType(tx, typeof(DataTransaction));
            Assert.AreEqual(tx.GenerateId(), transactionId);

            var dataTx = (DataTransaction)tx;
            Assert.AreEqual(dataTx.Sender, "3PCAB4sHXgvtu5NPoen6EXR5yaNbvsEA8Fj");
            Assert.AreEqual(dataTx.SenderPublicKey.ToBase58(), "2M25DqL2W4rGFLCFadgATboS8EPqyWAN3DjH12AH5Kdr");
            Assert.AreEqual(dataTx.Fee, Assets.WAVES.LongToAmount(100000));
            Assert.AreEqual(dataTx.Timestamp.ToLong(), 1534158656635);
            Assert.AreEqual(dataTx.Proofs[0].ToBase58(), "amM4aCTPxH6v5Lmw5j2zP4TYwJyJQjez5PPWd5aNK9UCTy7QPdUrXhayyGRMSo9i21ZRokMa9aE7qQ3YVp2SCfA");

            Assert.AreEqual(dataTx.Entries["test"], 11L);
            Assert.AreEqual(dataTx.Entries["neg"], -9223372036854775808);
            Assert.AreEqual(dataTx.Entries["max"], 9223372036854775807);
            Assert.AreEqual(dataTx.Entries["somestring"], "Some silly string");
            Assert.AreEqual(dataTx.Entries["longstring"], "ver long long long long long long long long long long long long long long long long long long long long long long long long long long long long long long long long long long long long long long long long long long long long long long long long long long long long string");
            Assert.AreEqual(dataTx.Entries["testboolF"], false);
            Assert.AreEqual(dataTx.Entries["testboolT"], true);
            Assert.AreEqual(((byte[])dataTx.Entries["testbytes"]).ToBase64(), "base64:VGVzdAVGVzdAVGVzdAVGVzdAVGVzdAVGVzdAVGVzdAVGVzdAVGVzdAVGVzdAVGVzdAVGVzdAVGVzdAVGVzdAVGVzdAVGVzdAVGVzdA==");
        }

        [TestMethod]
        public void TestSetScriptTransactionDeserialize()
        {
            var node = new Node(Node.MainNetChainId);

            var transactionId = "8Nwjd2tcQWff3S9WAhBa7vLRNpNnigWqrTbahvyfMVrU";
            var tx = node.GetTransactionById(transactionId);

            Assert.IsInstanceOfType(tx, typeof(SetScriptTransaction));
            Assert.AreEqual(tx.GenerateId(), transactionId);

            var setScriptTx = (SetScriptTransaction)tx;
            Assert.AreEqual(setScriptTx.Sender, "3PBSduYkK7GQxVFWkKWMq8GQkVdAGX71hTx");
            Assert.AreEqual(setScriptTx.SenderPublicKey.ToBase58(), "3LZmDK7vuSBsDmFLxJ4qihZynUz8JF9e88dNu5fsus5p");
            Assert.AreEqual(setScriptTx.Fee, Assets.WAVES.LongToAmount(2082496));
            Assert.AreEqual(setScriptTx.Timestamp.ToLong(), 1537973512182);
            Assert.AreEqual(setScriptTx.Proofs[0].ToBase58(), "V45jPG1nuEnwaYb9jTKQCJpRskJQvtkBcnZ45WjZUbVdNTi1KijVikJkDfMNcEdSBF8oGDYZiWpVTdLSn76mV57");
            Assert.AreEqual(setScriptTx.Script.ToBase64(), "base64:AQQAAAAEaW5hbAIAAAAESW5hbAQAAAAFZWxlbmECAAAAB0xlbnVza2EEAAAABGxvdmUCAAAAC0luYWxMZW51c2thCQAAAAAAAAIJAAEsAAAAAgUAAAAEaW5hbAUAAAAFZWxlbmEFAAAABGxvdmV4ZFt5");
        }

        [TestMethod]
        public void TestSponsoredFeeTransactionDeserialize()
        {
            var node = new Node(Node.MainNetChainId);

            var transactionId = "7EL2XEGP1By427BeLcHPYeVnBzGsXen4egMAwQpWGBVR";
            var tx = node.GetTransactionById(transactionId);

            Assert.IsInstanceOfType(tx, typeof(SponsoredFeeTransaction));
            Assert.AreEqual(tx.GenerateId(), transactionId);

            var sponsoredFeeTx = (SponsoredFeeTransaction)tx;
            Assert.AreEqual(sponsoredFeeTx.Sender, "3PHrS6VNPRtUD8MHkfkmELavL8JnGtSq5sx");
            Assert.AreEqual(sponsoredFeeTx.SenderPublicKey.ToBase58(), "5v5D5pqzKGBejtvtEeyDJXG28iQwMViu1uuetEcyQp9v");
            Assert.AreEqual(sponsoredFeeTx.Fee, Assets.WAVES.LongToAmount(100000000));
            Assert.AreEqual(sponsoredFeeTx.Timestamp.ToLong(), 1534448057070);
            Assert.AreEqual(sponsoredFeeTx.Proofs[0].ToBase58(), "3Q4JS4ujrGxAqp8LMXR9zZJC4tJ7YHiTo4SvMgrPhufo2UtR5x9JAaCGDjEr7qWXFDPJk7vWL8eapQkS45Dx1kcb");

            Asset asset = node.GetAsset("FN76goSi7hQn6gQ8aezKVwyDvhkWx5ekXbP3sNLWqavN");
            Assert.AreEqual(sponsoredFeeTx.Asset, asset);
            Assert.AreEqual(sponsoredFeeTx.MinimalFeeInAssets, asset.LongToAmount(10));
        }

        [TestMethod]
        public void TestInvokeScriptTransactionDeserialize()
        {
            var node = new Node(Node.MainNetChainId);

            var transactionId = "BCGQqfydM2PesQN2Wk5xbQqAqt2Vvoqzjdp2MjigEDYJ";
            var tx = node.GetTransactionById(transactionId);

            Assert.IsInstanceOfType(tx, typeof(InvokeScriptTransaction));
            Assert.AreEqual(tx.GenerateId(), transactionId);

            var invokeScriptTx = (InvokeScriptTransaction)tx;

            Assert.AreEqual(invokeScriptTx.Sender, "3PD1Wpo278Rut5jyd9wDfgzASB884HBbvSP");
            Assert.AreEqual(invokeScriptTx.SenderPublicKey.ToBase58(), "CJMd6zAswMrh4m7DkFqoAELQGtuBUAKSqeswdB7sax5h");
            Assert.AreEqual(invokeScriptTx.FeeAsset, Assets.WAVES);
            Assert.AreEqual(invokeScriptTx.Fee, invokeScriptTx.FeeAsset.LongToAmount(500000));
            Assert.AreEqual(invokeScriptTx.Timestamp.ToLong(), 1563792908168L);
            Assert.AreEqual(invokeScriptTx.Proofs.Length, 1);
            Assert.AreEqual(invokeScriptTx.Proofs[0].ToBase58(), "5pzcZH7fuYCVtdBLddLns3u1MPAH9ddgkRUt3UmGdDnWL1HTp2fUqPaEMQ2mwSHVLZi4xhgLyBmMaAG6FqCASkhG");
            Assert.AreEqual(invokeScriptTx.Version, 1);
            Assert.AreEqual(invokeScriptTx.DappAddress, "3P4DoFmdwJ4bA5qtviTSTVgY1uXsueuSgy9");
            Assert.AreEqual(invokeScriptTx.FunctionHeader, "bet");
            Assert.AreEqual(invokeScriptTx.FunctionCallArguments.Count, 1);
            Assert.AreEqual(invokeScriptTx.FunctionCallArguments[0], "92");
            Assert.AreEqual(invokeScriptTx.Payment.Count, 1);
        }

        [TestMethod]
        public void TestExchangeTransactionV1Deserialize()
        {
            var node = new Node(Node.MainNetChainId);

            var transactionId = "G4wGLw9XtnScqk5eWoLb7r3GXEf1FFg4CMmSX7du1wmg";
            var tx = node.GetTransactionById(transactionId);

            Assert.IsInstanceOfType(tx, typeof(ExchangeTransaction));
            Assert.AreEqual(tx.GenerateId(), transactionId);

            var exchangeTx = (ExchangeTransaction)tx;

            Assert.AreEqual(exchangeTx.Sender, "3PJaDyprvekvPXPuAtxrapacuDJopgJRaU3");
            Assert.AreEqual(exchangeTx.SenderPublicKey.ToBase58(), "7kPFrHDiGw1rCm7LPszuECwWYL3dMf6iMifLRDJQZMzy");
            Assert.AreEqual(exchangeTx.Fee, Assets.WAVES.LongToAmount(300000));
            Assert.AreEqual(exchangeTx.Timestamp.ToLong(), 1534759535561);
            Assert.AreEqual(exchangeTx.Proofs[0].ToBase58(), "5KiaSpEdYePbHkVGVNFUWeJhwyimiQ4iXenV4hpz1curVwk3ag91mWzhb6rzCFH64LiQLi4tbmKfJAjGrX5ico5b");

            var priceAsset = exchangeTx.BuyOrder.PriceAsset;
            var amountAsset = exchangeTx.BuyOrder.AmountAsset;

            Assert.AreEqual(exchangeTx.BuyOrder.Id, "9JoHuni4tyU4qV7xrK1DcRn9U6RVsdiJh94nqvCBzvT5");
            Assert.AreEqual(exchangeTx.BuyOrder.SenderPublicKey.ToBase58(), "6gs6QPtujkQ6SbagvHMzXyGMjtS2UrseATxCnn84TDFC");
            Assert.AreEqual(exchangeTx.BuyOrder.MatcherPublicKey.ToBase58(), "7kPFrHDiGw1rCm7LPszuECwWYL3dMf6iMifLRDJQZMzy");
            Assert.AreEqual(exchangeTx.BuyOrder.AmountAsset.Id, "4uK8i4ThRGbehENwa6MxyLtxAjAo1Rj9fduborGExarC");
            Assert.AreEqual(exchangeTx.BuyOrder.PriceAsset, Assets.WAVES);
            Assert.AreEqual(exchangeTx.BuyOrder.Side, OrderSide.Buy);
            Assert.AreEqual(exchangeTx.BuyOrder.Price, Asset.LongToPrice(amountAsset, priceAsset, 3920710000000));
            Assert.AreEqual(exchangeTx.BuyOrder.Amount, exchangeTx.BuyOrder.AmountAsset.LongToAmount(38028));
            Assert.AreEqual(exchangeTx.BuyOrder.Timestamp.ToLong(), 1534759534978);
            Assert.AreEqual(exchangeTx.BuyOrder.Expiration.ToLong(), 1534845934978);
            Assert.AreEqual(exchangeTx.BuyOrder.MatcherFee, Assets.WAVES.LongToAmount(300000));

            Assert.AreEqual(exchangeTx.SellOrder.Id, "985YULuysPMwJXSH8JzcvTJ1VBezRuSDXPcwG566nmQz");
            Assert.AreEqual(exchangeTx.SellOrder.SenderPublicKey.ToBase58(), "FYdPsmUnT8DoMEDCrUYfT1WGExfs48nn7EGuX5HozV3T");
            Assert.AreEqual(exchangeTx.SellOrder.MatcherPublicKey.ToBase58(), "7kPFrHDiGw1rCm7LPszuECwWYL3dMf6iMifLRDJQZMzy");
            Assert.AreEqual(exchangeTx.SellOrder.AmountAsset.Id, "4uK8i4ThRGbehENwa6MxyLtxAjAo1Rj9fduborGExarC");
            Assert.AreEqual(exchangeTx.SellOrder.PriceAsset, Assets.WAVES);
            Assert.AreEqual(exchangeTx.SellOrder.Side, OrderSide.Sell);
            Assert.AreEqual(exchangeTx.SellOrder.Price, Asset.LongToPrice(amountAsset, priceAsset, 3920710000000));
            Assert.AreEqual(exchangeTx.SellOrder.Amount, amountAsset.LongToAmount(500000));
            Assert.AreEqual(exchangeTx.SellOrder.Timestamp.ToLong(), 1534681075225);
            Assert.AreEqual(exchangeTx.SellOrder.Expiration.ToLong(), 1537272775177);
            Assert.AreEqual(exchangeTx.SellOrder.MatcherFee, Assets.WAVES.LongToAmount(300000));

            Assert.AreEqual(exchangeTx.Price, Asset.LongToPrice(amountAsset, priceAsset, 3920710000000));
            Assert.AreEqual(exchangeTx.Amount, amountAsset.LongToAmount(32986));
            Assert.AreEqual(exchangeTx.BuyMatcherFee, Assets.WAVES.LongToAmount(260224));
            Assert.AreEqual(exchangeTx.SellMatcherFee, Assets.WAVES.LongToAmount(19791));
        }

        [TestMethod]
        public void TestSetAssetScriptTransactionDeserialize()
        {
            var node = new Node(Node.TestNetChainId);

            var transactionId = "CL1xVnX93Wyq8c6N2X5ER8UkR49uZJofGwx9YVB7Deny";
            var tx = node.GetTransactionById(transactionId);
            tx.ChainId = node.ChainId;

            Assert.IsInstanceOfType(tx, typeof(SetAssetScriptTransaction));
            Assert.AreEqual(tx.GenerateId(), transactionId);

            var setAssetScriptTx = (SetAssetScriptTransaction)tx;

            Assert.AreEqual(setAssetScriptTx.Sender, "3N8S8VUbDJuqtUP8wAraWhMMeqcYHH5EWcF");
            Assert.AreEqual(setAssetScriptTx.SenderPublicKey.ToBase58(), "GLCqFngy1TYE4y2GW9pEZPLCbbZFaDG9qPTsTArBURF7");
            Assert.AreEqual(setAssetScriptTx.Fee, Assets.WAVES.LongToAmount(100000000));
            Assert.AreEqual(setAssetScriptTx.Timestamp.ToLong(), 1543834873148);
            Assert.AreEqual(setAssetScriptTx.Proofs[0].ToBase58(), "5kFVUfUNTyCRhuPVyLez8B7oihE4o4so1kpZxxJ3RTmHQBEv4wKLBdNzhWPaf8q3Gx9aV3Wg213xz67xXq2LsEY9");

            Assert.AreEqual(setAssetScriptTx.Asset.Id, "FLWiU3i5TUroZqTJZkweGVFjQALuo7dHCFM6jj4N8shK");
            Assert.AreEqual(setAssetScriptTx.Script.ToBase64(), "base64:AQfeYll6");
        }

        [TestMethod]
        public void TestExchangeTransactionV2Deserialize()
        {
            var node = new Node(Node.MainNetChainId);

            var transactionId = "9VJCXTdLqtsfvk1d68G5MT237ezQ4g9nuQhWZXR47vi9";
            var tx = node.GetTransactionById(transactionId);

            Assert.IsInstanceOfType(tx, typeof(ExchangeTransaction));
            Assert.AreEqual(tx.GenerateId(), transactionId);

            var exchangeTx = (ExchangeTransaction)tx;

            Assert.AreEqual(exchangeTx.Sender, "3PJaDyprvekvPXPuAtxrapacuDJopgJRaU3");
            Assert.AreEqual(exchangeTx.SenderPublicKey.ToBase58(), "7kPFrHDiGw1rCm7LPszuECwWYL3dMf6iMifLRDJQZMzy");
            Assert.AreEqual(exchangeTx.Fee, Assets.WAVES.LongToAmount(300000));
            Assert.AreEqual(exchangeTx.Timestamp.ToLong(), 1548660872389);
            Assert.AreEqual(exchangeTx.Proofs[0].ToBase58(), "3sKMGryDW1iEb94jdkzNNxHem59bW8NmUdCLKWWbyFC4qEZegKu8MVVrcvrewJkD4bjjg43SK1yhfQB1w9Vcka9p");

            var priceAsset = exchangeTx.BuyOrder.PriceAsset;
            var amountAsset = exchangeTx.BuyOrder.AmountAsset;

            Assert.AreEqual(exchangeTx.BuyOrder.Id, "Ho6Y16AKDrySs5VTa983kjg3yCx32iDzDHpDJ5iabXka");
            Assert.AreEqual(exchangeTx.BuyOrder.SenderPublicKey.ToBase58(), "FMc1iASTGwTC1tDwiKtrVHtdMkrVJ1S3rEBQifEdHnT2");
            Assert.AreEqual(exchangeTx.BuyOrder.MatcherPublicKey.ToBase58(), "7kPFrHDiGw1rCm7LPszuECwWYL3dMf6iMifLRDJQZMzy");
            Assert.AreEqual(exchangeTx.BuyOrder.AmountAsset.Id, "BrjUWjndUanm5VsJkbUip8VRYy6LWJePtxya3FNv4TQa");
            Assert.AreEqual(exchangeTx.BuyOrder.PriceAsset, Assets.WAVES);
            Assert.AreEqual(exchangeTx.BuyOrder.Side, OrderSide.Buy);
            Assert.AreEqual(exchangeTx.BuyOrder.Price, Asset.LongToPrice(amountAsset, priceAsset, 1799925005));
            Assert.AreEqual(exchangeTx.BuyOrder.Amount, exchangeTx.BuyOrder.AmountAsset.LongToAmount(150000000));
            Assert.AreEqual(exchangeTx.BuyOrder.Timestamp.ToLong(), 1548660872383);
            Assert.AreEqual(exchangeTx.BuyOrder.Expiration.ToLong(), 1551252872383);
            Assert.AreEqual(exchangeTx.BuyOrder.MatcherFee, Assets.WAVES.LongToAmount(300000));
            Assert.AreEqual(exchangeTx.BuyOrder.Proofs[0].ToBase58(), "YNPdPqEUGRW42bFyGqJ8VLHHBYnpukna3NSin26ERZargGEboAhjygenY67gKNgvP5nm5ZV8VGZW3bNtejSKGEa");

            Assert.AreEqual(exchangeTx.SellOrder.Id, "46qfvEVp4xEs6nxitCQH48ufB8sLRv7tsPnXsN96eUP4");
            Assert.AreEqual(exchangeTx.SellOrder.SenderPublicKey.ToBase58(), "75LAGRqae58sABeFza8fxSdq5o6zXJWRnCVbVCVjvPu5");
            Assert.AreEqual(exchangeTx.SellOrder.MatcherPublicKey.ToBase58(), "7kPFrHDiGw1rCm7LPszuECwWYL3dMf6iMifLRDJQZMzy");
            Assert.AreEqual(exchangeTx.SellOrder.AmountAsset.Id, "BrjUWjndUanm5VsJkbUip8VRYy6LWJePtxya3FNv4TQa");
            Assert.AreEqual(exchangeTx.SellOrder.PriceAsset, Assets.WAVES);
            Assert.AreEqual(exchangeTx.SellOrder.Side, OrderSide.Sell);
            Assert.AreEqual(exchangeTx.SellOrder.Price, Asset.LongToPrice(amountAsset, priceAsset, 1799925005));
            Assert.AreEqual(exchangeTx.SellOrder.Amount, amountAsset.LongToAmount(724649261));
            Assert.AreEqual(exchangeTx.SellOrder.Timestamp.ToLong(), 1548660849308);
            Assert.AreEqual(exchangeTx.SellOrder.Expiration.ToLong(), 1548661449308);
            Assert.AreEqual(exchangeTx.SellOrder.MatcherFee, Assets.WAVES.LongToAmount(300000));
            Assert.AreEqual(exchangeTx.SellOrder.Proofs[0].ToBase58(), "42RxAFJXJs98GoJE11bdjwDPRrqKzsvqBmG6cYoVn5B8e314u3D3DJ54vN9527em8syqKUHbpHWwenHUdfEcBta5");

            Assert.AreEqual(exchangeTx.Price, Asset.LongToPrice(amountAsset, priceAsset, 1799925005));
            Assert.AreEqual(exchangeTx.Amount, amountAsset.LongToAmount(150000000));
            Assert.AreEqual(exchangeTx.BuyMatcherFee, Assets.WAVES.LongToAmount(300000));
            Assert.AreEqual(exchangeTx.SellMatcherFee, Assets.WAVES.LongToAmount(62099));
        }
    }
}
