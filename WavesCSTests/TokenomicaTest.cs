using System;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;


namespace WavesCSTests
{
    [TestClass]
    public class TokenomicaTest
    {
        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }       

        [TestMethod]
        public void TestCreateUserAccountScript()
        {
            var node = new Node();
            var script = @"let tokenomicaPubKey = base58'7dkSgXFv9EpYi3C3JK76wJTkciBsVPZ1xE5fVAMB6AD9'                            
                                let this = extract(tx.sender)

                                match tx { 
                                    case s: SetScriptTransaction =>
                                        sigVerify(s.bodyBytes, s.proofs[0], tokenomicaPubKey)
                                    case t: TransferTransaction =>
                                        let limit = extract(getInteger(this, ""limit""))
                                        (t.amount <= limit) &&
                                        isDefined(t.assetId) &&                                        
                                        sigVerify(t.bodyBytes, t.proofs[0], tx.senderPublicKey)                                    
                                    case m: MassTransferTransaction =>
                                        let limit = extract(getInteger(this, ""limit""))
                                        (m.totalAmount <= limit) &&
                                        isDefined(m.assetId) &&
                                        sigVerify(m.bodyBytes, m.proofs[0], tx.senderPublicKey)  
                                    case e: ExchangeTransaction =>
                                        let limit = extract(getInteger(this, ""limit""))
                                        (!isDefined(e.sellOrder.assetPair.amountAsset) && e.buyOrder.amount <= limit) ||
                                        (!isDefined(e.sellOrder.assetPair.priceAsset) && e.sellOrder.amount <= limit) &&
                                        sigVerify(e.bodyBytes, e.proofs[0], tx.senderPublicKey)  
                                    case d: DataTransaction =>
                                        sigVerify(d.bodyBytes, d.proofs[0], tokenomicaPubKey)
                                    case _ => false
                                }";
            var compiledScript = node.CompileScript(script);

            Console.WriteLine("Compiled script: {0}", compiledScript);
            var userSeed = PrivateKeyAccount.GenerateSeed();
            var userAccount = PrivateKeyAccount.CreateFromSeed(userSeed, AddressEncoding.TestNet);

            var setScriptTx = new SetScriptTransaction(userAccount.PublicKey, compiledScript, AddressEncoding.TestNet, 0.14m);
            setScriptTx.Sign(userAccount);
            node.Broadcast(setScriptTx.GetJsonWithSignature());

            Thread.Sleep(10000);
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
                                let limit = extract(getInteger(tx.sender, ""limit""))
                                        (!isDefined(t.assetId) &&
                                         t.amount <= limit ||
                                        isDefined(t.assetId)) &&
                                match tx { 
                                    case s: SetScriptTransaction =>
                                        sigVerify(s.bodyBytes, s.proofs[0], tokenomicaPubKey)
                                    case t: TransferTransaction =>
                                        
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

            var scriptInfo = node.GetObject("addresses/scriptInfo/{0}", Accounts.Carol.Address);
                        
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
           
            var wavesTransferTx = new TransferTransaction(userAccount.PublicKey, Accounts.Alice.Address, Assets.WAVES, 0.01m, 0.005m).Sign(userAccount);
            wavesTransferTx.Version = 2;
            node.Broadcast(wavesTransferTx.GetJsonWithSignature());
        }        
    }
}
