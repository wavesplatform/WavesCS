using System;
using System.Threading;
using WavesCS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WavesCSTests
{
    [TestClass]
    public class AliasTest
    {
        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }

        [TestMethod]
        public void TestTransferToAlias()
        {
            var node = new Node(Node.TestNetHost);

            var seed = PrivateKeyAccount.GenerateSeed();
            var account = PrivateKeyAccount.CreateFromSeed(seed, 'T');

            var balanceBefore = node.GetBalance(account.Address);

            var alias = "alias:T:123alias456";
            node.CreateAlias(account, alias, 'T');

            var amount = 0.0001m;
            node.Transfer(Accounts.Alice, alias, Assets.WAVES, amount);

            var balanceAfter = node.GetBalance(account.Address);
            Assert.AreEqual(balanceBefore + amount, balanceAfter);
        }
    }
}
