using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS;

namespace WavesCSTests
{
    [TestClass]
    public class SmartAssetsTest
    {
        Asset smartAsset = new Asset("", "", 8);

        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }

        [TestMethod]
        public void TestIssueSmartAsset()
        {
            var node = new Node();

            var script = "true";
            var compiledScript = node.CompileScript(script);


            smartAsset = node.IssueAsset(Accounts.Alice, "SmartAsset",
                                          "Smart Asset", 100, 4,
                                          true, compiledScript);
            Assert.IsNotNull(smartAsset);

            Thread.Sleep(15000);

            var quantity = node.GetBalance(Accounts.Alice.Address, smartAsset);
            Assert.AreEqual(quantity, 100);
        }

        [TestMethod]
        public void TestSetAssetScript()
        {
            var node = new Node();

            var script = $@"                
                match tx {{
                    case tx : TransferTransaction => tx.amount < 300000
                    case tx : BurnTransaction | SetAssetScriptTransaction => true
                    case _  => false
                }}";

            var asset = Assets.GetById("");

            var compiledScript = node.CompileScript(script);

            var setAssetScriptTransaction = new SetAssetScriptTransaction(Accounts.Alice.PublicKey,
                                                                          asset, compiledScript,
                                                                          'T', 1);

            setAssetScriptTransaction.Sign(Accounts.Alice);
            node.Broadcast(setAssetScriptTransaction);

            Thread.Sleep(4000);

            var balanceBefore = ;

            for (decimal amount = 0.01m; amount < 0.5m; amount += 0.1m)
            {
                try
                {
                    node.Transfer(Accounts.Alice, Accounts.Bob.Address, smartAsset, amount);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }


            Thread.Sleep(5000);

            var balanceAfter = ;

            Assert.AreEqual(balanceAfter, balanceBefore + 0.01m + 0.11m + 0.21m);

            // + check the fee


        }
    }
}