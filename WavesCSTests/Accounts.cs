using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS;

namespace WavesCSTests
{
    [TestClass]
    public class Accounts
    {
        public static readonly PrivateKeyAccount Alice = PrivateKeyAccount.CreateFromPrivateKey("G8awYgVXwSTHeKxmNCfMCeH7hubMKmhJSWyZP6qBNx7w", AddressEncoding.TestNet);
        public static readonly PrivateKeyAccount Bob = PrivateKeyAccount.CreateFromPrivateKey("BpU9bhdySk7sbmkTxzUXVYHKiZSZv9miAMmZAbnxarHg", AddressEncoding.TestNet);       
        public static readonly PrivateKeyAccount Carol = PrivateKeyAccount.CreateFromSeed("belts intact sand spot demise rely extra suggest river pretty disease large exchange ceiling inquiry", AddressEncoding.TestNet);
            
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
            
            var scriptInfo = node.GetObject("addresses/scriptInfo/{0}", Alice.Address);
            Assert.IsFalse(scriptInfo.ContainsKey("scriptText"));
            
            scriptInfo = node.GetObject("addresses/scriptInfo/{0}", Bob.Address);
            Assert.IsFalse(scriptInfo.ContainsKey("scriptText"));
            
            scriptInfo = node.GetObject("addresses/scriptInfo/{0}", Carol.Address);
            Assert.IsFalse(scriptInfo.ContainsKey("scriptText"));
        }
    }
}