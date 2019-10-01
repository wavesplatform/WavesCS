using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS;

namespace WavesCSTests
{
    [TestClass]
    public class Accounts
    {
        public static readonly PrivateKeyAccount Alice = PrivateKeyAccount.CreateFromSeed("seed4Alice", Node.TestNetChainId);
        public static readonly PrivateKeyAccount Bob = PrivateKeyAccount.CreateFromSeed("seed4Bob", Node.TestNetChainId);
        public static readonly PrivateKeyAccount Carol = PrivateKeyAccount.CreateFromSeed("seed4Carol4", Node.TestNetChainId);

        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }

        [TestMethod]
        public void TestBalance()
        {
            // Use faucet to fill accounts https://testnet.wavesexplorer.com/faucet
            var node = new Node(Node.TestNetChainId);

            var aliceBalanceWaves = node.GetBalance(Alice.Address);
            var bobBalanceWaves = node.GetBalance(Bob.Address);
            var carolBalanceWaves = node.GetBalance(Carol.Address);

            Console.WriteLine("Alice address: {0}, balance: {1}", Alice.Address, aliceBalanceWaves);
            Console.WriteLine("Bob address: {0}, balance: {1}", Bob.Address, bobBalanceWaves);
            Console.WriteLine("Carol address: {0}, balance: {1}", Carol.Address, carolBalanceWaves);
            
            Assert.IsTrue(aliceBalanceWaves > 1);
            Assert.IsTrue(bobBalanceWaves > 1);
            Assert.IsTrue(carolBalanceWaves > 1);
        }

        [TestMethod]
        public void TestScript()
        {
            Http.Tracing = false;
            var node = new Node(Node.TestNetChainId);

            var scriptInfo = node.GetObject("addresses/scriptInfo/{0}", Alice.Address);
            Assert.IsFalse(scriptInfo.ContainsKey("scriptText"));

            scriptInfo = node.GetObject("addresses/scriptInfo/{0}", Bob.Address);
            Assert.IsFalse(scriptInfo.ContainsKey("scriptText"));

            scriptInfo = node.GetObject("addresses/scriptInfo/{0}", Carol.Address);
            Assert.IsFalse(scriptInfo.ContainsKey("scriptText"));
        }
    }
}