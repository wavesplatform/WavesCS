using System;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCSIntegrationTests
{

    [TestClass]
    public class PoWTest
    {
        public static readonly PrivateKeyAccount PoWAccount = PrivateKeyAccount.CreateFromSeed("gossip system also kitten coast fossil much board maximum replace hip stumble color stem", 'T');
        public static readonly PrivateKeyAccount Alice = PrivateKeyAccount.CreateFromSeed("seed4Alice", 'T');

        int N = 2; // current PoW complexity

        string accountScript = @"#Proof-of-Work account
                        let PoWAccount = extract(tx.sender)
                        let PoWAsset = base58'CZk46R9XmGhrtCHFRB1qJqZbQAM9yK2ys7c5xsakBjJ4'
                        match tx {
                            case tx : TransferTransaction =>
                                if tx.assetId == PoWAsset then tx.fee == 900000 else sigVerify(tx.bodyBytes, tx.proofs[0], tx.senderPublicKey)
                            case tx : DataTransaction =>
                                size(tx.data) == 1
                                && tx.data[0].key == ""N""
                                && sigVerify(tx.bodyBytes, tx.proofs[0], tx.senderPublicKey)
                            case _ => sigVerify(tx.bodyBytes, tx.proofs[0], tx.senderPublicKey)
                        }";

        string assetScript = @"#Proof-of-Work asset
                    let PoWAccount = Address(base58'3N2hBea4tJ4vmGdTnr7iGEfeCwsDkLyCbWK')
                    let PoWAsset = base58'CZk46R9XmGhrtCHFRB1qJqZbQAM9yK2ys7c5xsakBjJ4'
                    match tx {
                        case tx : TransferTransaction =>
                            let N = extract(getInteger(PoWAccount, ""N""))
                            let firstBytes = toBase58String(take(tx.id, N))
                            let balance = assetBalance(PoWAccount, PoWAsset)
                            let denominator = 100
                            tx.sender != PoWAccount ||
                            isDefined(getBoolean(PoWAccount, firstBytes)) && tx.amount == balance / denominator + 1
                        case _ => true
                    }";


        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }

        [TestMethod]
        public void TestIssueAsset ()
        {
            var node = new Node();

            Asset smartAsset = node.IssueAsset(PoWAccount, "GoldAsset",
                                               "GoldAsset is a smart asset that can be mined. Get some GoldAssets and sell them on DEX. The detailed info can be found on https://sway.office.com/CXrafP6sNPTKHCXJ?ref=Link",
                                               1000000m, 0, false, node.CompileScript("true"), 1.004m);
            Thread.Sleep(3000);
            Assert.IsNotNull(smartAsset);
        }

        [TestMethod]
        public void TestPoWAccountData()
        {
            var node = new Node();

            try
            {
                Assert.IsTrue(node.GetAddressData(PoWAccount.Address).ContainsKey("N"));
            }
            catch (Exception)
            {
                var data = new DictionaryObject { { "N", N } };
                for (int i = 1; i <= 32; i++)
                {
                    data.Add(Enumerable.Repeat((byte)0, i).ToArray().ToBase58(), true);
                }

                var dataTx = new DataTransaction(PoWAccount.PublicKey, data, 0.005m).Sign(PoWAccount);
                node.Broadcast(dataTx);

                Thread.Sleep(10000);
                Assert.AreEqual(N, node.GetAddressData(PoWAccount.Address)["N"]);
            }

            try
            {
                var currentN = (long) node.GetAddressData(PoWAccount.Address)["N"];
                Assert.AreEqual(N, currentN);
            }
            catch (Exception)
            {
                var data = new DictionaryObject { { "N", N } };

                var dataTx = new DataTransaction(PoWAccount.PublicKey, data, 0.005m).Sign(PoWAccount);
                node.Broadcast(dataTx);

                Thread.Sleep(10000);
                Assert.AreEqual(N, node.GetAddressData(PoWAccount.Address)["N"]);
            }
        }

        [TestMethod]
        public void TestPoWAccountScript()
        {
            var node = new Node();
            var compiledAccountScript = node.CompileScript(accountScript);

            try
            {
                Assert.AreEqual(compiledAccountScript.ToBase64(), node.GetObject("addresses/scriptInfo/{0}", PoWAccount.Address)["script"]);
            }
            catch (Exception)
            {
                var setScriptTx = new SetScriptTransaction(PoWAccount.PublicKey, compiledAccountScript, 'T', 0.14m).Sign(PoWAccount);
                node.Broadcast(setScriptTx.GetJsonWithSignature());

                Thread.Sleep(10000);
            }
        }

        [TestMethod]
        public void TestPoWAssetScript()
        {
            var node = new Node();
            Asset PoWAsset = Assets.GetById("CZk46R9XmGhrtCHFRB1qJqZbQAM9yK2ys7c5xsakBjJ4");
            var compiledAssetScript = node.CompileScript(assetScript);

            try
            {
                Assert.AreEqual(PoWAsset.Script.ToBase64(), compiledAssetScript.ToBase64());
            }
            catch (Exception)
            {
                var setAssetScriptTx = new SetAssetScriptTransaction(PoWAccount.PublicKey, PoWAsset, compiledAssetScript, 'T', 1.004m).Sign(PoWAccount);
                node.Broadcast(setAssetScriptTx.GetJsonWithSignature());

                Thread.Sleep(10000);
            }
        }

        [TestMethod]
        public void TestPoWMining()
        {
            var node = new Node();
            Asset PoWAsset = Assets.GetById("CZk46R9XmGhrtCHFRB1qJqZbQAM9yK2ys7c5xsakBjJ4");

            var currentBalance = (int) node.GetBalance(PoWAccount.Address, PoWAsset);
            var transferAmount = currentBalance / 100 + 1;
            var tx = new TransferTransaction(PoWAccount.PublicKey, Alice.Address, PoWAsset, PoWAsset.LongToAmount(transferAmount), 0.009m);

            try
            {
                node.Broadcast(tx);
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.Message, "The request was canceled");
            }

            var begin = DateTime.UtcNow;

            while (true)
            {
                tx = new TransferTransaction(PoWAccount.PublicKey,
                                             Alice.Address,
                                             PoWAsset, PoWAsset.LongToAmount(transferAmount), 0.009m);
                var id = tx.GenerateId();
                var idFirstNBytes = id.FromBase58().Take((int)N).ToArray();

                if (idFirstNBytes.Count(b => b == 0) == N)
                {
                    Console.WriteLine($"Elapsed time = {(DateTime.UtcNow - begin).TotalMilliseconds} ms");
                    Console.WriteLine($"Generated transaction id: {id}");
                    node.Broadcast(tx);
                    break;
                }
            }
        }
    }
}
