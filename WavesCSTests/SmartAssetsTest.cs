using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS;

namespace WavesCSTests
{
    [TestClass]
    public class SmartAssetsTest
    {
        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }

        [TestMethod]
        public void TestIssueSmartAsset()
        {
            var node = new Node();

            var compiledScript = node.CompileScript("true");

            Asset smartAsset = node.IssueAsset(Accounts.Alice, "SmartAsset",
                                          "Smart Asset", 100, 4,
                                          true, compiledScript);
            Assert.IsNotNull(smartAsset);

            Thread.Sleep(15000);

            Assert.AreEqual(node.GetBalance(Accounts.Alice.Address, smartAsset), 100);
            Assert.AreEqual(Assets.GetById(smartAsset.Id).Script.ToBase64(), compiledScript.ToBase64());
        }

        [TestMethod]
        public void TestSetAssetScript()
        {
            var node = new Node();
            var wavesBalanceBefore = node.GetBalance(Accounts.Alice.Address, Assets.WAVES);

            Asset smartAsset = node.IssueAsset(Accounts.Alice, "SmartAsset",
                                               "Smart Asset", 100, 8,
                                               true, node.CompileScript("true"));

            Thread.Sleep(10000);

            var script = $@"                
                match tx {{
                    case t : TransferTransaction => t.amount < 30000000
                    case bs : BurnTransaction | SetAssetScriptTransaction => true
                    case _  => false
                }}";

            var compiledScript = node.CompileScript(script);

            var setAssetScriptTransaction = new SetAssetScriptTransaction(Accounts.Alice.PublicKey, smartAsset,
                                                                          compiledScript, 'T', 1);

            setAssetScriptTransaction.Sign(Accounts.Alice);
            node.Broadcast(setAssetScriptTransaction);
            Thread.Sleep(10000);

            var aliceBalanceBefore = node.GetBalance(Accounts.Alice.Address, smartAsset);
            var bobBalanceBefore = node.GetBalance(Accounts.Bob.Address, smartAsset);

            for (decimal amount = 0.01m; amount < 0.5m; amount += 0.1m)
            {
                try
                {
                    node.Transfer(Accounts.Alice, Accounts.Bob.Address, smartAsset, amount, 0.005m);
                    Thread.Sleep(4000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            Thread.Sleep(10000);

            var aliceBalanceAfter = node.GetBalance(Accounts.Alice.Address, smartAsset);
            var bobBalanceAfter = node.GetBalance(Accounts.Bob.Address, smartAsset);

            Assert.AreEqual(aliceBalanceBefore - aliceBalanceAfter, 0.01m + 0.11m + 0.21m);
            Assert.AreEqual(bobBalanceAfter - bobBalanceBefore, 0.01m + 0.11m + 0.21m);

            setAssetScriptTransaction = new SetAssetScriptTransaction(Accounts.Alice.PublicKey, smartAsset,
                                                                      node.CompileScript("false"), 'T', 1);

            setAssetScriptTransaction.Sign(Accounts.Alice);
            node.Broadcast(setAssetScriptTransaction);
            Thread.Sleep(12000);

            Assert.AreEqual(Assets.GetById(smartAsset.Id).Script.ToBase64(), node.CompileScript("false").ToBase64());

            var wavesBalanceAfter = node.GetBalance(Accounts.Alice.Address, Assets.WAVES);

            // Check the fee
            Assert.AreEqual(wavesBalanceBefore, wavesBalanceAfter + 1m + 1m + 0.005m * 3 + 1m);
        }
    }
}
