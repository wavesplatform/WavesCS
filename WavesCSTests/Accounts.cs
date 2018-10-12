using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS;

namespace WavesCSTests
{
    [TestClass]
    public class Accounts
    {
        public static readonly PrivateKeyAccount Alice = PrivateKeyAccount.CreateFromSeed("seed4Alice", AddressEncoding.TestNet);
        public static readonly PrivateKeyAccount Bob = PrivateKeyAccount.CreateFromSeed("seed4Bob", AddressEncoding.TestNet);
        public static readonly PrivateKeyAccount Carol = PrivateKeyAccount.CreateFromSeed("seed4Carol2", AddressEncoding.TestNet);

        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }

        [TestMethod]
        public void TestBalance()
        {
            // Use faucet to fill acounts https://testnet.wavesexplorer.com/faucet
            var node = new Node();

            var bobBalanceWaves = node.GetBalance(Bob.Address);
            var aliceBalanceWaves = node.GetBalance(Alice.Address);
            var carolBalanceWaves = node.GetBalance(Carol.Address);

            Console.WriteLine("Alice address: {0}, balance: {1}", Alice.Address, aliceBalanceWaves);
            Console.WriteLine("Bob address: {0}, balance: {1}", Bob.Address, bobBalanceWaves);
            Console.WriteLine("Carol address: {0}, balance: {1}", Carol.Address, carolBalanceWaves);
            
            Assert.IsTrue(aliceBalanceWaves > 1);
            Assert.IsTrue(bobBalanceWaves > 1);
            Assert.IsTrue(carolBalanceWaves > 1);
        }
        
        [TestMethod]
        public void TestScripts()
        {
            Http.Tracing = true;
            
            var node = new Node();

            var account = PrivateKeyAccount.CreateFromSeed(PrivateKeyAccount.GenerateSeed(), 'T');
            var scriptInfo = node.GetObject("addresses/scriptInfo/{0}", account.Address);

            Assert.IsFalse(scriptInfo.ContainsKey("scriptText"));
        }
    }
}