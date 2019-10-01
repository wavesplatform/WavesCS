using System;
using System.IO;
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var node = new Node(Node.TestNetChainId);

            var script = "true";
            var compiledScript = node.CompileScript(script);

            Console.WriteLine("Compiled script: {0}", compiledScript);

            var response = node.SetScript(Accounts.Carol, compiledScript, 0.014m);
            node.WaitTransactionConfirmationByResponse(response);

            var scriptInfo = node.GetObject("addresses/scriptInfo/{0}", Accounts.Carol.Address);
            Assert.AreEqual("true", scriptInfo["scriptText"]);
            Assert.AreEqual(compiledScript.ToBase64(), scriptInfo["script"]);
            Assert.IsTrue(scriptInfo.GetInt("complexity") > 0);
            Assert.IsTrue(scriptInfo.GetInt("extraFee") > 0);

            response = node.SetScript(Accounts.Carol, null, 0.014m);
            node.WaitTransactionConfirmationByResponse(response);

            scriptInfo = node.GetObject("addresses/scriptInfo/{0}", Accounts.Carol.Address);
            Assert.IsFalse(scriptInfo.ContainsKey("scriptText"));
            Assert.IsFalse(scriptInfo.ContainsKey("script"));
            Assert.AreEqual(0, scriptInfo.GetInt("complexity"));
            Assert.AreEqual(0, scriptInfo.GetInt("extraFee"));
        }

        [TestMethod]
        public void TestMultisig()
        {
            // This test works with transfer transactions of version 2 only
            var node = new Node(Node.TestNetChainId);

            var script = $@"                
                let aliceSigned = sigVerify(tx.bodyBytes, tx.proofs[0], base58'{Accounts.Alice.PublicKey.ToBase58()}')
                let bobSigned   = sigVerify(tx.bodyBytes, tx.proofs[1], base58'{Accounts.Bob.PublicKey.ToBase58()}')
                aliceSigned && bobSigned";

            Console.WriteLine($"Script: {script}");

            var compiledScript = node.CompileScript(script);

            var multiAccount = PrivateKeyAccount.CreateFromSeed(PrivateKeyAccount.GenerateSeed(), Node.TestNetChainId);
            Console.WriteLine("Account generated: {0}", multiAccount.Address);

            var response = node.Transfer(Accounts.Alice, multiAccount.Address, Assets.WAVES, 0.1m);
            node.WaitTransactionConfirmationByResponse(response);

            Thread.Sleep(5000);

            Assert.IsTrue(node.GetBalance(multiAccount.Address) == 0.1m);

            response = node.SetScript(multiAccount, compiledScript, node.ChainId);
            node.WaitTransactionConfirmationByResponse(response);

            var tx = new TransferTransaction(node.ChainId, multiAccount.PublicKey, Accounts.Alice.Address, Assets.WAVES, 0.07m, 0.005m) { Version = 2 };
            tx.Sign(Accounts.Alice, 0);
            tx.Sign(Accounts.Bob, 1);

            node.BroadcastAndWait(tx);

            Thread.Sleep(10000);

            Assert.IsTrue(node.GetBalance(multiAccount.Address) < 0.02m);
        }

        [TestMethod]
        public void TestErrorMessage()
        {
            var node = new Node(Node.TestNetChainId);
            var account = PrivateKeyAccount.CreateFromSeed(PrivateKeyAccount.GenerateSeed(), node.ChainId);

            var transferTxResponse = node.Transfer(Accounts.Alice, account.Address, Assets.WAVES, 0.02m);
            node.WaitTransactionConfirmationByResponse(transferTxResponse);

            var script = @"{-# STDLIB_VERSION 3 #-}
{-# CONTENT_TYPE DAPP #-}
{-# SCRIPT_TYPE ACCOUNT #-}

@Verifier(tx)
func verify() = {
    match tx {
        case d: SetScriptTransaction | DataTransaction => true
        case _ => false
    }
}";

            var compiledScript = node.CompileCode(script);

            var setScriptTxResponse = node.SetScript(account,compiledScript);
            node.WaitTransactionConfirmationByResponse(setScriptTxResponse);

            try
            {
                node.Transfer(account, account.Address, Assets.WAVES, 0.00000001m, 0.005m);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                Assert.AreEqual(e.Message, "Transaction is not allowed by account-script");
            }
        }
    }
}