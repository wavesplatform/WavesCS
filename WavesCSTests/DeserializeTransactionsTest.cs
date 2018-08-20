using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WavesCS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace WavesCSTests
{
    [TestClass]
    public class DeserializeTransactionsTest
    {
        [TestMethod]
        public void TestListTransactions()
        {
           
            Node node = new Node(Node.MainNetHost);

            var limit = 100;
            var address = "3PBmsJXAcgnH9cu81oyW8abNh9jsaNzFQKJ";
            var transactions = node.ListTransactions(address, limit);
            
            Assert.AreEqual(transactions.Count(), limit);
        }

        [TestMethod]
        public void TestIssueTransactionDeserialize()
        {
            var node = new Node(Node.MainNetHost);
            var tx = node.GetTransactionById("37nfgadHFw92hNqzyHFZXmGFo5Wmct6Eik1Y2AdYW1Aq");

            Assert.IsInstanceOfType(tx, typeof(IssueTransaction));

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
        public void TestTransferTransactionDeserialize()
        {
            var node = new Node(Node.MainNetHost);
            var tx = node.GetTransactionById("9jpwy6aYJRFnFWoArQxLywMSF8GRGyW42JT1KzHJD9sL");

            Assert.IsInstanceOfType(tx, typeof(TransferTransaction));

            var transferTx = (TransferTransaction)tx;
            Assert.AreEqual(transferTx.Sender, "3PBmsJXAcgnH9cu81oyW8abNh9jsaNzFQKJ");
            Assert.AreEqual(transferTx.SenderPublicKey.ToBase58(), "6gs6QPtujkQ6SbagvHMzXyGMjtS2UrseATxCnn84TDFC");
            Assert.AreEqual(transferTx.Fee, Assets.WAVES.LongToAmount(100000));
            Assert.AreEqual(transferTx.Timestamp.ToLong(), 1534497330297);
            Assert.AreEqual(transferTx.Proofs[0].ToBase58(), "47vBFvnu2jcYEUEvhVSJcsZcC1LmiDC31joS8PuftT2CpvrT5nm9gxjxuP4MFeogBgDhStzxKndQbh6P4ejaqtUs");
            Assert.AreEqual(transferTx.Recipient, "3PPsHUKZ8WLU7sLm9sV5Sb75RNKnMinqU58");

            Asset asset = Assets.GetById("4uK8i4ThRGbehENwa6MxyLtxAjAo1Rj9fduborGExarC");
            Assert.AreEqual(transferTx.Asset, asset);
            Assert.AreEqual(transferTx.Amount, asset.LongToAmount(1097948));

            Assert.AreEqual(transferTx.Attachment.ToBase58(), "");
        }

        [TestMethod]
        public void TestReissueTransactionDeserialize()
        {
            var node = new Node(Node.MainNetHost);
            var tx = node.GetTransactionById("HqtDBXsbz3ztNHheF3DstVKhsEYf3rtA31tPa784hiyx");

            Assert.IsInstanceOfType(tx, typeof(ReissueTransaction));

            var reissueTx = (ReissueTransaction)tx;
            Assert.AreEqual(reissueTx.Sender, "3PKhW99rBCeQFurBibu7KMjYLf7GTRudYj7");
            Assert.AreEqual(reissueTx.SenderPublicKey.ToBase58(), "7rSpgoCWeAPp3dKdBq5PExGJ63DM91MpDLHkdZqVzmVV");
            Assert.AreEqual(reissueTx.Fee, Assets.WAVES.LongToAmount(100000000));
            Assert.AreEqual(reissueTx.Timestamp.ToLong(), 1534335650023);
            Assert.AreEqual(reissueTx.Proofs[0].ToBase58(), "26U7gP4YRAB1YMR7nzSS2wc5CeWKMPWwep6xAFrpLRXhbAWkpszEgdZaFvjYdRGucLVioD1JzpvduHuuyM88fXPM");

            Asset asset = Assets.GetById("37nfgadHFw92hNqzyHFZXmGFo5Wmct6Eik1Y2AdYW1Aq");
            Assert.AreEqual(reissueTx.Asset, asset);
            Assert.AreEqual(reissueTx.Quantity, asset.LongToAmount(1838160000000000000));

            Assert.IsTrue(reissueTx.Reissuable);
        }

        [TestMethod]
        public void TestBurnTransactionDeserialize()
        {
            var node = new Node(Node.MainNetHost);
            var tx = node.GetTransactionById("HXhyvS9f5oQ18QAEeyRg6E9FHvAnLVWGSTGMyamgMe4n");

            Assert.IsInstanceOfType(tx, typeof(BurnTransaction));

            var burnTx = (BurnTransaction)tx;
            Assert.AreEqual(burnTx.Sender, "3PBmsJXAcgnH9cu81oyW8abNh9jsaNzFQKJ");
            Assert.AreEqual(burnTx.SenderPublicKey.ToBase58(), "6gs6QPtujkQ6SbagvHMzXyGMjtS2UrseATxCnn84TDFC");
            Assert.AreEqual(burnTx.Fee, Assets.WAVES.LongToAmount(100000));
            Assert.AreEqual(burnTx.Timestamp.ToLong(), 1534497330455);
            Assert.AreEqual(burnTx.Proofs[0].ToBase58(), "4UzYadVf4Gz9udZL5eZ1SHYVXF3XhjcCi46mBc6aP5zCuvxtS41sNUzBeTirggjVuU9P3cYzwNh1gDjDRQYZVY1t");

            Asset asset = Assets.GetById("4uK8i4ThRGbehENwa6MxyLtxAjAo1Rj9fduborGExarC");
            Assert.AreEqual(burnTx.Asset, asset);
            Assert.AreEqual(asset.AmountToLong(burnTx.Quantity), 274487);
        }

        [TestMethod]
        public void TestLeaseTransactionDeserialize()
        {
            var node = new Node(Node.MainNetHost);
            var tx = node.GetTransactionById("8feDmqySpSLJxfPYmnKCb99jf2g7oFGEY5Lu8DofqBCU");

            Assert.IsInstanceOfType(tx, typeof(LeaseTransaction));

            var leaseTx = (LeaseTransaction)tx;
            Assert.AreEqual(leaseTx.Sender, "3PNHKfDxU6PML1yHhU55gBn5jWkMvqFPYPP");
            Assert.AreEqual(leaseTx.SenderPublicKey.ToBase58(), "2hLjbCQT96cEynvXcRugwAqAnvxKLUscqonnUbYDcvmy");
            Assert.AreEqual(leaseTx.Fee, Assets.WAVES.LongToAmount(100000));
            Assert.AreEqual(leaseTx.Timestamp.ToLong(), 1534418507297);
            Assert.AreEqual(leaseTx.Proofs[0].ToBase58(), "2bPuQJncTh4eeARfB4PJ4L1tYJSAKkaNRyJhjfovSKjyUCNcaSEzqBNGY9r6MoEt4M1a1KwabKDa8xo6yhPf3t7L");
            Assert.AreEqual(Assets.WAVES.AmountToLong(leaseTx.Amount), 1658046538);
            Assert.AreEqual(leaseTx.Recipient, "3P23fi1qfVw6RVDn4CH2a5nNouEtWNQ4THs");
        }

        [TestMethod]
        public void TestCancelLeasingTransactionDeserialize()
        {
            var node = new Node(Node.MainNetHost);
            var tx = node.GetTransactionById("8LkSyfgyuekjCherhjmhKR1gbYKJPAhfKSYLetSN93YW");

            Assert.IsInstanceOfType(tx, typeof(CancelLeasingTransaction));

            var cancelLeaseTx = (CancelLeasingTransaction)tx;
            Assert.AreEqual(cancelLeaseTx.Sender, "3PL3HscfDpAd6LFYYGcNKeUPg25tkpS4qeq");
            Assert.AreEqual(cancelLeaseTx.SenderPublicKey.ToBase58(), "2Gp8D5v9Edjmr2kEYQz6oN4JRQmzMFhLhKYLPD8ZSKRL");
            Assert.AreEqual(cancelLeaseTx.Fee, Assets.WAVES.LongToAmount(100000));
            Assert.AreEqual(cancelLeaseTx.Timestamp.ToLong(), 1534418379080);
            Assert.AreEqual(cancelLeaseTx.Proofs[0].ToBase58(), "5DDoFWGpKcQsuoMLHbGTEevzX1KtkPrvSgSZdxVqtZdqm2CnVJa3yVQAmFFErAzhZcxYEibESjp6jhMgjzsZV2cQ");
            Assert.AreEqual(cancelLeaseTx.TransactionId, "YjrfVmtCKo9P5K9EwK86kUFjtoEhaRjC4HpTf7hZeN8");
        }

        [TestMethod]
        public void TestAliasTransactionDeserialize()
        {
            var node = new Node(Node.MainNetHost);
            var tx = node.GetTransactionById("5JeRnELGEsT1bTZgbNETeJ6rVqhRvrbpMz82nh3qynpH");

            Assert.IsInstanceOfType(tx, typeof(AliasTransaction));

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
            var node = new Node(Node.MainNetHost);
            var tx = node.GetTransactionById("EKPLV5vjsa2T8ijpjNNumikDrM1r6Yi6MXpNsbsdPM8i");

            Assert.IsInstanceOfType(tx, typeof(MassTransferTransaction));

            var massTransferTx = (MassTransferTransaction)tx;
            Assert.AreEqual(massTransferTx.Sender, "3P5CcDGigUuiHUaAstEj6Yv25xdnP4UQz1F");
            Assert.AreEqual(massTransferTx.SenderPublicKey.ToBase58(), "2i8drMSLgFTVSX3sitPsL6n98YXwsi8tGQbaMSrFrWMr");
            Assert.AreEqual(massTransferTx.Fee, Assets.WAVES.LongToAmount(4900000));
            Assert.AreEqual(massTransferTx.Timestamp.ToLong(), 1534263320261);
            Assert.AreEqual(massTransferTx.Proofs[0].ToBase58(), "V9U4CCkcn5Br73ZHGtgFbDWFF9fYjHFcxiynEgEci5s2WkNEVr4h7mDBB9hqLMvRfkLLLaF6KyNehguJuBMer2a");

            Asset asset = Assets.GetById("9GGTr8sRMbyb8wWi6dcJGDQR5qChdJxJqgzreMTAf716");
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
            var node = new Node(Node.MainNetHost);
            var tx = node.GetTransactionById("BDBZWsarzShKoqmYUUiuFYZ3zewjvap6Laa7ctEutaP6");

            Assert.IsInstanceOfType(tx, typeof(DataTransaction));

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
            var node = new Node(Node.TestNetHost);
            var tx = node.GetTransactionById("7XcsgCeHV1jKYGdpYHTgAMijz5TdLfKZMv2u2oe1EuXT");

            Assert.IsInstanceOfType(tx, typeof(SetScriptTransaction));

            var setScriptTx = (SetScriptTransaction)tx;
            Assert.AreEqual(setScriptTx.Sender, "3MwbQnJ65JZjY3tNdmj2ifoweR1HAoqUSVx");
            Assert.AreEqual(setScriptTx.SenderPublicKey.ToBase58(), "AALTKFwrtTD8yze6QCCKgCyMGTY4pWZFM16AW4oSPpNB");
            Assert.AreEqual(setScriptTx.Fee, Assets.WAVES.LongToAmount(500000));
            Assert.AreEqual(setScriptTx.Timestamp.ToLong(), 1534514481584);
            Assert.AreEqual(setScriptTx.Proofs[0].ToBase58(), "2ex1A8j5P9hTjB25HipFC8yPoDcGhwnagS5RzgAKjpyB3qgRR7Xc5MHFHEiBbeG32LijaRucAZHYzbE2uWs878Pi");
            Assert.AreEqual(setScriptTx.Script.ToBase64(), "base64:AQQAAAALYWxpY2VTaWduZWQJAAH0AAAAAwgFAAAAAnR4AAAACWJvZHlCeXRlcwkAAZEAAAACCAUAAAACdHgAAAAGcHJvb2ZzAAAAAAAAAAAAAQAAACAyuczjMUkCXNyulQ5XMJoscp6PpQdKiwOVTUaNdinzewQAAAAJYm9iU2lnbmVkCQAB9AAAAAMIBQAAAAJ0eAAAAAlib2R5Qnl0ZXMJAAGRAAAAAggFAAAAAnR4AAAABnByb29mcwAAAAAAAAAAAQEAAAAg8vnnzoTlg42VE3HxgTtzt0sm8go1mP98KmEv2vvB31QDBQAAAAthbGljZVNpZ25lZAUAAAAJYm9iU2lnbmVkB7Ewcq0=");
        }

        [TestMethod]
        public void TestSponsoredFeeTransactionDeserialize()
        {
            var node = new Node(Node.MainNetHost);
            var tx = node.GetTransactionById("7EL2XEGP1By427BeLcHPYeVnBzGsXen4egMAwQpWGBVR");

            Assert.IsInstanceOfType(tx, typeof(SponsoredFeeTransaction));

            var sponsoredFeeTx = (SponsoredFeeTransaction)tx;
            Assert.AreEqual(sponsoredFeeTx.Sender, "3PHrS6VNPRtUD8MHkfkmELavL8JnGtSq5sx");
            Assert.AreEqual(sponsoredFeeTx.SenderPublicKey.ToBase58(), "5v5D5pqzKGBejtvtEeyDJXG28iQwMViu1uuetEcyQp9v");
            Assert.AreEqual(sponsoredFeeTx.Fee, Assets.WAVES.LongToAmount(100000000));
            Assert.AreEqual(sponsoredFeeTx.Timestamp.ToLong(), 1534448057070);
            Assert.AreEqual(sponsoredFeeTx.Proofs[0].ToBase58(), "3Q4JS4ujrGxAqp8LMXR9zZJC4tJ7YHiTo4SvMgrPhufo2UtR5x9JAaCGDjEr7qWXFDPJk7vWL8eapQkS45Dx1kcb");

            Asset asset = Assets.GetById("FN76goSi7hQn6gQ8aezKVwyDvhkWx5ekXbP3sNLWqavN");
            Assert.AreEqual(sponsoredFeeTx.Asset, asset);
            Assert.AreEqual(sponsoredFeeTx.MinimalFeeInAssets, asset.LongToAmount(10));
        }

        [TestMethod]
        public void TestExchangeTransactionDeserialize()
        {
            var node = new Node(Node.MainNetHost);
            var tx = node.GetTransactionById("G4wGLw9XtnScqk5eWoLb7r3GXEf1FFg4CMmSX7du1wmg");

            Assert.IsInstanceOfType(tx, typeof(ExchangeTransaction));

            var exchangeTx = (ExchangeTransaction)tx;
            Assert.AreEqual(exchangeTx.Sender, "3PJaDyprvekvPXPuAtxrapacuDJopgJRaU3");
            Assert.AreEqual(exchangeTx.SenderPublicKey.ToBase58(), "7kPFrHDiGw1rCm7LPszuECwWYL3dMf6iMifLRDJQZMzy");
            Assert.AreEqual(exchangeTx.Fee, Assets.WAVES.LongToAmount(300000));
            Assert.AreEqual(exchangeTx.Timestamp.ToLong(), 1534759535561);
            Assert.AreEqual(exchangeTx.Proofs[0].ToBase58(), "5KiaSpEdYePbHkVGVNFUWeJhwyimiQ4iXenV4hpz1curVwk3ag91mWzhb6rzCFH64LiQLi4tbmKfJAjGrX5ico5b");

            Assert.AreEqual(exchangeTx.Order1.Id, "9JoHuni4tyU4qV7xrK1DcRn9U6RVsdiJh94nqvCBzvT5");
            // ??? Assert.AreEqual(exchangeTx.Order1.Sender, "3PBmsJXAcgnH9cu81oyW8abNh9jsaNzFQKJ");
            // ??? Assert.AreEqual(exchangeTx.Order1.SenderPublicKey, "6gs6QPtujkQ6SbagvHMzXyGMjtS2UrseATxCnn84TDFC");
            // ??? Assert.AreEqual(exchangeTx.Order1.MatcherPublicKey, "7kPFrHDiGw1rCm7LPszuECwWYL3dMf6iMifLRDJQZMzy");
            Assert.AreEqual(exchangeTx.Order1.AmountAsset.Id, "4uK8i4ThRGbehENwa6MxyLtxAjAo1Rj9fduborGExarC");
            Assert.AreEqual(exchangeTx.Order1.PriceAsset, Assets.WAVES);
            Assert.AreEqual(exchangeTx.Order1.Side, OrderSide.Buy);
            Assert.AreEqual(exchangeTx.Order1.Price, Asset.LongToPrice(exchangeTx.Order1.AmountAsset, exchangeTx.Order1.PriceAsset, 3920710000000));
            Assert.AreEqual(exchangeTx.Order1.Amount, exchangeTx.Order1.AmountAsset.LongToAmount(38028));
            Assert.AreEqual(exchangeTx.Order1.Timestamp.ToLong(), 1534759534978);
            // ??? Assert.AreEqual(exchangeTx.Order1.Expiration.ToLong(), 1534845934978);
            // ??? Assert.AreEqual(exchangeTx.Order1.MatcherFee, Assets.WAVES.LongToAmount(300000));
            // ??? Assert.AreEqual(exchangeTx.Order1.Signature, "BpPNM5jveAV1p7GLeEuQ9RmCmst8MGyiJkQ7JNHaw2Hu1trpyB7f3C9vUFiMjWqccZfdwaL91yGpCRdLsv9GKGa");

            Assert.AreEqual(exchangeTx.Order2.Id, "985YULuysPMwJXSH8JzcvTJ1VBezRuSDXPcwG566nmQz");
            // ??? Assert.AreEqual(exchangeTx.Order2.Sender, "3P2bd2EQ1vojNuCmFYa3d7qrn7JMSz1EXBz");
            // ??? Assert.AreEqual(exchangeTx.Order2.SenderPublicKey, "FYdPsmUnT8DoMEDCrUYfT1WGExfs48nn7EGuX5HozV3T");
            // ??? Assert.AreEqual(exchangeTx.Order2.MatcherPublicKey, "7kPFrHDiGw1rCm7LPszuECwWYL3dMf6iMifLRDJQZMzy");
            Assert.AreEqual(exchangeTx.Order2.AmountAsset.Id, "4uK8i4ThRGbehENwa6MxyLtxAjAo1Rj9fduborGExarC");
            Assert.AreEqual(exchangeTx.Order2.PriceAsset, Assets.WAVES);
            Assert.AreEqual(exchangeTx.Order2.Side, OrderSide.Sell);
            Assert.AreEqual(exchangeTx.Order2.Price, Asset.LongToPrice(exchangeTx.Order2.AmountAsset, exchangeTx.Order2.PriceAsset, 3920710000000));
            Assert.AreEqual(exchangeTx.Order2.Amount, exchangeTx.Order2.AmountAsset.LongToAmount(500000));
            Assert.AreEqual(exchangeTx.Order2.Timestamp.ToLong(), 1534681075225);
            // ??? Assert.AreEqual(exchangeTx.Order2.Expiration.ToLong(), 1537272775177);
            // ??? Assert.AreEqual(exchangeTx.Order2.MatcherFee, 300000);
            // ??? Assert.AreEqual(exchangeTx.Order2.Signature, "rpVQaKB6vvLbzyf3RECHSNBoTM6V8JiR8f7s3x96jF5TMcnH4wZWEqgn4Sqkf4TUGDBYutEsvfU2gYrSPHqdTHU");

            Assert.AreEqual(exchangeTx.Price, exchangeTx.Order1.PriceAsset.LongToAmount(3920710000000));
            Assert.AreEqual(exchangeTx.Amount, exchangeTx.Order1.AmountAsset.LongToAmount(32986));
            Assert.AreEqual(exchangeTx.BuyMatcherFee, Assets.WAVES.LongToAmount(260224));
            Assert.AreEqual(exchangeTx.SellMatcherFee, Assets.WAVES.LongToAmount(19791));

        }
    }
}
