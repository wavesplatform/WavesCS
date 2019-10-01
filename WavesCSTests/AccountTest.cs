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

            var account = PrivateKeyAccount.CreateFromPrivateKey(privateKey, Node.TestNetChainId);            
            Assert.AreEqual(privateKey, account.PrivateKey.ToBase58());
            Assert.AreEqual(publicKey, account.PublicKey.ToBase58());
            Assert.AreEqual(address, account.Address);
        }

        [TestMethod]
        public void TestAccountEncodeDecode()
        {
            var publicKey = "8LbAU5BSrGkpk5wbjLMNjrbc9VzN9KBBYv9X8wGpmAJT";
            var privateKey = "CMLwxbMZJMztyTJ6Zkos66cgU7DybfFJfyJtTVpme54t";
             
            var account = PrivateKeyAccount.CreateFromPrivateKey(privateKey, Node.TestNetChainId);
            Assert.AreEqual(privateKey, account.PrivateKey.ToBase58());
            Assert.AreEqual(publicKey, account.PublicKey.ToBase58());
        }        

        [TestMethod]
        public void TestAccountCreation()
        {
            var seed = "health lazy lens fix dwarf salad breeze myself silly december endless rent faculty report beyond";
            var account = PrivateKeyAccount.CreateFromSeed(seed, Node.TestNetChainId);

            byte[] seed2 = Encoding.UTF8.GetBytes(seed);
            var account2 = PrivateKeyAccount.CreateFromSeed(seed2, Node.TestNetChainId);

            Assert.AreEqual("CMLwxbMZJMztyTJ6Zkos66cgU7DybfFJfyJtTVpme54t", account.PrivateKey.ToBase58());
            Assert.AreEqual("8LbAU5BSrGkpk5wbjLMNjrbc9VzN9KBBYv9X8wGpmAJT", account.PublicKey.ToBase58());
            Assert.AreEqual("3MzZCGFyuxgC4ZmtKRS7vpJTs75ZXdkbp1K", account.Address);

            Assert.AreEqual("CMLwxbMZJMztyTJ6Zkos66cgU7DybfFJfyJtTVpme54t", account2.PrivateKey.ToBase58());
            Assert.AreEqual("8LbAU5BSrGkpk5wbjLMNjrbc9VzN9KBBYv9X8wGpmAJT", account2.PublicKey.ToBase58());
            Assert.AreEqual("3MzZCGFyuxgC4ZmtKRS7vpJTs75ZXdkbp1K", account2.Address);
        }

        [TestMethod]
        public void TestSeedGeneration()
        {
            string seed = PrivateKeyAccount.GenerateSeed();
            Assert.AreEqual(15, seed.Split(' ').Length);

            seed = PrivateKeyAccount.GenerateSeed();
            Assert.AreEqual(15, seed.Split(' ').Length);
        }
    }
}
