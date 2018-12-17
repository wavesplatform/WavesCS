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
        public static readonly PrivateKeyAccount Alice = PrivateKeyAccount.CreateFromSeed("seed4Alice", AddressEncoding.TestNet);
        public static readonly PrivateKeyAccount Bob = PrivateKeyAccount.CreateFromSeed("seed4Bob", 'T');

        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }

        [TestMethod]
        public void TestPoW()
        {
            var node = new Node();
            Asset PoWAsset = Assets.GetById("1KcaUQAEUBmuJmdiP9gkwJdXdUwAvk9ZCGXviUeDF3v");

            long N = 2;

            var accountScript = @"#Proof-of-Work account
                        let PoWAccount = extract(tx.sender)
                        let PoWAsset = base58'1KcaUQAEUBmuJmdiP9gkwJdXdUwAvk9ZCGXviUeDF3v'
                        match tx {
                            case tx : TransferTransaction =>
                                if tx.assetId == PoWAsset then tx.fee == 900000 else sigVerify(tx.bodyBytes, tx.proofs[0], tx.senderPublicKey)
                            case tx : DataTransaction =>
                                size(tx.data) == 1
                                && tx.data[0].key == ""N""
                                && sigVerify(tx.bodyBytes, tx.proofs[0], tx.senderPublicKey)
                            case _ => sigVerify(tx.bodyBytes, tx.proofs[0], tx.senderPublicKey)
                        }";

            var compiledAccountScript = node.CompileScript(accountScript);

            var assetScript = @"#Proof-of-Work asset
                    let PoWAccount = Address(base58'3N2hBea4tJ4vmGdTnr7iGEfeCwsDkLyCbWK')
                    let PoWAsset = base58'1KcaUQAEUBmuJmdiP9gkwJdXdUwAvk9ZCGXviUeDF3v'
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

            var compiledAssetScript = node.CompileScript(assetScript);

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
                Assert.AreEqual(compiledAccountScript.ToBase64(), node.GetObject("addresses/scriptInfo/{0}", PoWAccount.Address)["script"]);
            }
            catch (Exception)
            {
                var setScriptTx = new SetScriptTransaction(PoWAccount.PublicKey, compiledAccountScript, 'T', 0.14m).Sign(PoWAccount);
                node.Broadcast(setScriptTx.GetJsonWithSignature());

                Thread.Sleep(10000);
            }

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

            try
            {
                Assert.AreEqual(N, node.GetAddressData(PoWAccount.Address)["N"]);
            }
            catch (Exception)
            {
                var data = new DictionaryObject { { "N", N } };

                var dataTx = new DataTransaction(PoWAccount.PublicKey, data, 0.005m).Sign(PoWAccount);
                node.Broadcast(dataTx);

                Thread.Sleep(10000);
                Assert.AreEqual(N, node.GetAddressData(PoWAccount.Address)["N"]);
            }

            var currentBalance = (int) node.GetBalance(PoWAccount.Address, PoWAsset);
            var transferAmount = currentBalance / 100 + 1;
            var tx = new TransferTransaction(PoWAccount.PublicKey, Bob.Address, PoWAsset, PoWAsset.LongToAmount(transferAmount)).Sign(Bob);

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
                                             Bob.Address,
                                             PoWAsset, PoWAsset.LongToAmount(transferAmount), 0.009m);
                var id = tx.GenerateId();
                var idFirstNBytes = id.FromBase58().Take((int)N).ToArray();

                if (idFirstNBytes.Count(b => b == 0) == N)
                {
                    Console.WriteLine($"Generated id: {id}");
                    break;
                }
            }

            var end = DateTime.UtcNow;
            Console.WriteLine($"Elapsed time = {(end - begin).TotalMilliseconds} ms");
            Console.WriteLine(tx.GenerateId());
            node.Broadcast(tx);
        }
    }
}
