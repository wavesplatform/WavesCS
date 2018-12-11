using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;


namespace WavesCSIntegrationTests
{
    [TestClass]
    public class TokenomicaTest
    {

        public static readonly PrivateKeyAccount Alice = PrivateKeyAccount.CreateFromSeed("seed4Alice", AddressEncoding.TestNet);

        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }

        [TestMethod]
        public void TestCreateUserAccount()
        {
            var node = new Node();
            var script = @"let tokenomicaPubKey = base58'7dkSgXFv9EpYi3C3JK76wJTkciBsVPZ1xE5fVAMB6AD9'                            
                            let this = extract(tx.sender)

                            match tx { 
                                case s: SetScriptTransaction =>
                                    sigVerify(s.bodyBytes, s.proofs[0], tokenomicaPubKey)
                                case t: TransferTransaction =>
                                    let limit = extract(getInteger(tx.sender, ""limit""))
                                    (!isDefined(t.assetId) &&
                                     t.amount <= limit ||
                                    isDefined(t.assetId)) &&
                                    sigVerify(t.bodyBytes, t.proofs[0], t.senderPublicKey)
                                case m: MassTransferTransaction =>
                                    let limit = extract(getInteger(this, ""limit""))
                                    (!isDefined(m.assetId) &&
                                    m.totalAmount <= limit ||
                                    isDefined(m.assetId)) &&
                                    sigVerify(m.bodyBytes, m.proofs[0], tx.senderPublicKey)
                                case e: ExchangeTransaction =>
                                    let limit = extract(getInteger(this, ""limit""))
                                    ((!isDefined(e.sellOrder.assetPair.amountAsset) && e.buyOrder.amount <= limit)
                                        || isDefined(e.sellOrder.assetPair.amountAsset)) &&
                                    ((!isDefined(e.sellOrder.assetPair.priceAsset) && e.sellOrder.amount <= limit)
                                        || isDefined(e.buyOrder.assetPair.amountAsset)) &&
                                    sigVerify(e.bodyBytes, e.proofs[0], tx.senderPublicKey)
                                case d: DataTransaction =>
                                    sigVerify(d.bodyBytes, d.proofs[0], tokenomicaPubKey)
                                case _ => false
                            }";
            var compiledScript = node.CompileScript(script);
            var userSeed = PrivateKeyAccount.GenerateSeed();
            var userAccount = PrivateKeyAccount.CreateFromSeed(userSeed, AddressEncoding.TestNet);
            var transferTransaction = new TransferTransaction(Alice.PublicKey, userAccount.Address, Assets.WAVES, 1m, 0.001m).Sign(Alice);
            node.Broadcast(transferTransaction.GetJsonWithSignature());
            Thread.Sleep(5000);

            var setScriptTx = new SetScriptTransaction(userAccount.PublicKey, compiledScript, AddressEncoding.TestNet, 0.14m);
            setScriptTx.Sign(userAccount);
            node.Broadcast(setScriptTx.GetJsonWithSignature());
        }

        [TestMethod]
        public void TestChangeUserAccountScript()
        {
            var node = new Node();

            var tokenomicaSeed = "aim property attract warfare stamp sample holiday input invest rather potato novel produce car arctic"; //3N6GrCERRyWw9k9siP9iPbqNV9q86jnbrYY
            var tokenomicaAccount = PrivateKeyAccount.CreateFromSeed(tokenomicaSeed, AddressEncoding.TestNet);

            var userSeed = "castle vocal place join absent various dignity sunset thrive hurry island joy you gossip public";
            var userAccount = PrivateKeyAccount.CreateFromSeed(userSeed, AddressEncoding.TestNet);

            var newScript = @"let tokenomicaPubKey = base58'7dkSgXFv9EpYi3C3JK76wJTkciBsVPZ1xE5fVAMB6AD9'                            
                                let this = extract(tx.sender)

                                match tx { 
                                    case s: SetScriptTransaction =>
                                        sigVerify(s.bodyBytes, s.proofs[0], tokenomicaPubKey)
                                    case t: TransferTransaction =>
                                        let limit = extract(getInteger(tx.sender, ""limit""))
                                        (!isDefined(t.assetId) &&
                                         t.amount <= limit ||
                                        isDefined(t.assetId)) &&
                                        sigVerify(t.bodyBytes, t.proofs[0], tx.senderPublicKey)
                                    case m: MassTransferTransaction =>
                                        let limit = extract(getInteger(this, ""limit""))
                                        (m.totalAmount <= limit &&
                                        !isDefined(m.assetId) ||
                                        isDefined(m.assetId)) &&
                                        sigVerify(m.bodyBytes, m.proofs[0], tx.senderPublicKey)
                                    case e: ExchangeTransaction =>
                                        let limit = extract(getInteger(this, ""limit""))
                                        ((!isDefined(e.sellOrder.assetPair.amountAsset) && e.buyOrder.amount <= limit)
                                            || isDefined(e.sellOrder.assetPair.amountAsset)) &&
                                        ((!isDefined(e.sellOrder.assetPair.priceAsset) && e.sellOrder.amount <= limit)
                                            || isDefined(e.buyOrder.assetPair.amountAsset)) &&
                                        sigVerify(e.bodyBytes, e.proofs[0], tx.senderPublicKey)
                                    case d: DataTransaction =>
                                        sigVerify(d.bodyBytes, d.proofs[0], tokenomicaPubKey)
                                    case _ => false
                                }";
            var newCompiledScript = node.CompileScript(newScript);


            var tokenomicaSetScriptTx = new SetScriptTransaction(userAccount.PublicKey, newCompiledScript, AddressEncoding.TestNet, 0.14m);
            var s = newCompiledScript.ToBase58();
            tokenomicaSetScriptTx.Sign(tokenomicaAccount);
            node.Broadcast(tokenomicaSetScriptTx.GetJsonWithSignature());

        }

        [TestMethod]
        public void TestWavesSpendingUserAccountScript()
        {
            var node = new Node();

            var userSeed = "castle vocal place join absent various dignity sunset thrive hurry island joy you gossip public";
            var userAccount = PrivateKeyAccount.CreateFromSeed(userSeed, AddressEncoding.TestNet);

            var tokenomicaSeed = "aim property attract warfare stamp sample holiday input invest rather potato novel produce car arctic"; //3N6GrCERRyWw9k9siP9iPbqNV9q86jnbrYY
            var tokenomicaAccount = PrivateKeyAccount.CreateFromSeed(tokenomicaSeed, AddressEncoding.TestNet);

            var data = new DictionaryObject
            {
                { "limit", 200000001L }
            };

            var dataTx = new DataTransaction(userAccount.PublicKey, data, 0.005m).Sign(tokenomicaAccount);
            node.Broadcast(dataTx.GetJsonWithSignature());

            var wavesTransferTx = new TransferTransaction(userAccount.PublicKey, Alice.Address, Assets.WAVES, 0.01m, 0.005m).Sign(userAccount);
            wavesTransferTx.Version = 2;
            node.Broadcast(wavesTransferTx.GetJsonWithSignature());
        }

        [TestMethod]
        public void TestCreateAssetScript()
        {
            var node = new Node();
            var account = PrivateKeyAccount.CreateFromSeed("aim property attract warfare stamp sample holiday input invest rather potato novel produce car arctic", 'T');
            var script = $@"
            let account = base58'{account.Address}'
            let matcherAccount = account
             match tx {{
                case tx: BurnTransaction => tx.sender.bytes == account
                case tx: ExchangeTransaction =>
                    tx.sender.bytes == matcherAccount
                    && extract(getBoolean(Address(account), toBase58String(tx.buyOrder.sender.bytes))) # whitelist
                    && extract(getBoolean(Address(account), toBase58String(tx.sellOrder.sender.bytes)))
                case tx: TransferTransaction => tx.sender.bytes == account || addressFromRecipient(tx.recipient).bytes == account
                case tx: MassTransferTransaction => false
                case tx: ReissueTransaction => true
                case _ => true # SetAssetScriptTransaction
            }}";
            var compiledScript = node.CompileScript(script);
            var asset = node.IssueAsset(account, "ttoken", "ttoken", 1000000m, 8, true, compiledScript);
        }
    }
}
