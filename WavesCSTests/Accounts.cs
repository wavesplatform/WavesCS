using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS;

namespace WavesCSTests
{
    [TestClass]
    public class Accounts
    {
        public static readonly PrivateKeyAccount Alice = PrivateKeyAccount.CreateFromPrivateKey("DMLwxbMZJMztyTJ6Zkos66cgU7DybfFJfyJtTVpme54o", AddressEncoding.TestNet);
        public static readonly PrivateKeyAccount Bob = PrivateKeyAccount.CreateFromPrivateKey("25Um7fKYkySZnweUEVAn9RLtxN5xHRd7iqpqYSMNQEeT", AddressEncoding.TestNet);       
        public static readonly PrivateKeyAccount Carol = PrivateKeyAccount.CreateFromSeed("general rose scissors hybrid clutch method era habit client caught toward actress pilot infant theme", AddressEncoding.TestNet);
                
        [TestMethod]
        public void TestBalance()
        {
            var node = new Node();

            var bobBalanceWaves = node.GetBalance(Bob.Address);
            var aliceBalanceWaves = node.GetBalance(Alice.Address);
            var carolBalanceBalanceWaves = node.GetBalance(Carol.Address);
                            
            Console.WriteLine("Alice balance: {0} waves, Bob baance: {1}, Carol balance: {2}", 
                aliceBalanceWaves, bobBalanceWaves, carolBalanceBalanceWaves);

            Assert.IsTrue(aliceBalanceWaves > 100000000);
            Assert.IsTrue(bobBalanceWaves > 100000000);
            Assert.IsTrue(carolBalanceBalanceWaves > 100000000);
        }
    }
}