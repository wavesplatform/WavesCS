using System;
using WavesCS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace WavesCSTests
{
    [TestClass]
    public class AccountTest
    {
 
        [TestMethod]
        public void TestAccountProperties()
        {
            var publicKey = "8LbAU5BSrGkpk5wbjLMNjrbc9VzN9KBBYv9X8wGpmAJT";
            var privateKey = "CMLwxbMZJMztyTJ6Zkos66cgU7DybfFJfyJtTVpme54t";
            var address = "3MzZCGFyuxgC4ZmtKRS7vpJTs75ZXdkbp1K";

            var account = PrivateKeyAccount.CreateFromPrivateKey(privateKey, AddressEncoding.TestNet);
            CollectionAssert.AreEqual(Base58.Decode(privateKey), account.PrivateKey);
            CollectionAssert.AreEqual(Base58.Decode(publicKey), account.PublicKey);
            Assert.AreEqual(address, account.Address);
        }

        [TestMethod]
        public void TestAccountEncodeDecode()
        {
            var publicKey = "8LbAU5BSrGkpk5wbjLMNjrbc9VzN9KBBYv9X8wGpmAJT";
            var privateKey = "CMLwxbMZJMztyTJ6Zkos66cgU7DybfFJfyJtTVpme54t";
             
            var account = PrivateKeyAccount.CreateFromPrivateKey(privateKey, AddressEncoding.TestNet);
            Assert.AreEqual(privateKey, Base58.Encode(Base58.Decode(privateKey)));
            Assert.AreEqual(publicKey, Base58.Encode(Base58.Decode(publicKey)));
        }        

        [TestMethod]
        public void TestAccountCreation()
        {
            var seed = "health lazy lens fix dwarf salad breeze myself silly december endless rent faculty report beyond";
            var account = PrivateKeyAccount.CreateFromSeed(seed, AddressEncoding.TestNet);

            byte[] seed2 = Encoding.UTF8.GetBytes(seed);
            var account2 = PrivateKeyAccount.CreateFromSeed(seed2, AddressEncoding.TestNet);

            CollectionAssert.AreEqual(Base58.Decode("CMLwxbMZJMztyTJ6Zkos66cgU7DybfFJfyJtTVpme54t"), account.PrivateKey);
            CollectionAssert.AreEqual(Base58.Decode("8LbAU5BSrGkpk5wbjLMNjrbc9VzN9KBBYv9X8wGpmAJT"), account.PublicKey);
            Assert.AreEqual("3MzZCGFyuxgC4ZmtKRS7vpJTs75ZXdkbp1K", account.Address);

            CollectionAssert.AreEqual(Base58.Decode("CMLwxbMZJMztyTJ6Zkos66cgU7DybfFJfyJtTVpme54t"), account2.PrivateKey);
            CollectionAssert.AreEqual(Base58.Decode("8LbAU5BSrGkpk5wbjLMNjrbc9VzN9KBBYv9X8wGpmAJT"), account2.PublicKey);
            Assert.AreEqual("3MzZCGFyuxgC4ZmtKRS7vpJTs75ZXdkbp1K", account2.Address);
        }


        [TestMethod]
        public void TestSeedGeneration()
        {
            String seed = PrivateKeyAccount.GenerateSeed();
            Assert.AreEqual(15, seed.Split(' ').Length);

            seed = PrivateKeyAccount.GenerateSeed();
            Assert.AreEqual(15, seed.Split(' ').Length);
        }
    }
}
