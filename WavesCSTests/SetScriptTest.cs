﻿using System;
using System.Collections;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Serialization;
using WavesCS;

namespace WavesCSTests
{
    [TestClass]
    public class SetScriptTest
    {        
        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }

        [TestMethod]
        public void TestSetScript()
        {
            var node = new Node();            
                
            var script = "true";
            var compiledScript = node.CompileScript(script);
            
            Console.WriteLine("Compiled script: {0}", compiledScript);
       
            var setScriptTx = new SetScriptTransaction(Accounts.Carol.PublicKey, compiledScript, 'T', 0.014m);            
            setScriptTx.Sign(Accounts.Carol);
            node.Broadcast(setScriptTx.GetJsonWithSignature());

            Thread.Sleep(10000);
            
            var scriptInfo = node.GetObject("addresses/scriptInfo/{0}", Accounts.Carol.Address);
            Assert.AreEqual("TRUE", scriptInfo["scriptText"]);
            Assert.AreEqual(compiledScript.ToBase64(), scriptInfo["script"]);
            Assert.IsTrue(scriptInfo.GetInt("complexity") > 0);
            Assert.IsTrue(scriptInfo.GetInt("extraFee") > 0);
            
            var cleanScriptTx = new SetScriptTransaction(Accounts.Carol.PublicKey, null, 'T', 0.014m);                        
            node.Broadcast(cleanScriptTx.GetJsonWithSignature());
            
            Thread.Sleep(10000);
            
            scriptInfo = node.GetObject("addresses/scriptInfo/{0}", Accounts.Carol.Address);
            Assert.IsFalse(scriptInfo.ContainsKey("scriptText"));
            Assert.IsFalse(scriptInfo.ContainsKey("script"));
            Assert.AreEqual(0, scriptInfo.GetInt("complexity"));
            Assert.AreEqual(0, scriptInfo.GetInt("extraFee"));
        }
        
        
        [TestMethod]
        public void MultisigTest()
        {
            // This test works with tranfer transactions of version 2 only
            TransferTransaction.Version = 2;
            var node = new Node();            

            var script = $@"                
                let aliceSigned = sigVerify(tx.bodyBytes, tx.proofs[0], base58'{Accounts.Alice.PublicKey.ToBase58()}')
                let bobSigned   = sigVerify(tx.bodyBytes, tx.proofs[1], base58'{Accounts.Bob.PublicKey.ToBase58()}')
                aliceSigned && bobSigned";
            
            Console.WriteLine($"Scrit: {script}");
            
            var compiledScript = node.CompileScript(script);
            
            var multiAccount = PrivateKeyAccount.CreateFromSeed(PrivateKeyAccount.GenerateSeed(), AddressEncoding.TestNet);           
            Console.WriteLine("Account generated: {0}", multiAccount.Address);
            node.Transfer(Accounts.Alice, multiAccount.Address, Assets.WAVES, 0.1m);
            
            Thread.Sleep(10000);
            
            Assert.IsTrue(node.GetBalance(multiAccount.Address) == 0.1m);
            
            var setScriptTx = new SetScriptTransaction(multiAccount.PublicKey, compiledScript, 'T', 0.01m);            
            setScriptTx.Sign(multiAccount);
            node.Broadcast(setScriptTx.GetJsonWithSignature());

            Thread.Sleep(10000);
            
            var tx = new TransferTransaction(multiAccount.PublicKey, Accounts.Alice.Address, Assets.WAVES, 0.09m, 0.005m);
            tx.Sign(Accounts.Alice, 0);
            tx.Sign(Accounts.Bob, 1);

            node.Broadcast(tx);
            
            Thread.Sleep(10000);
            
            Assert.IsTrue(node.GetBalance(multiAccount.Address) < 0.02m);
        }
       
    }
}