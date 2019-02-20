using System;
using System.Threading;
using WavesCS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;

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

        public string GenerateRandomAlias(int length = 15)
        {
            Assert.IsTrue(4 <= length && length <= 30);

            Random random = new Random();
            string aliasAlphabet = "-.0123456789@_abcdefghijklmnopqrstuvwxyz";

            return new string(Enumerable.Repeat(aliasAlphabet, length)
                                   .Select(s => s[random.Next(s.Length)]).ToArray());

        }

        [TestMethod]
        public void TestTransferToAlias()
        {
            var node = new Node(Node.TestNetHost);

            var seed = PrivateKeyAccount.GenerateSeed();
            var account = PrivateKeyAccount.CreateFromSeed(seed, 'T');

            node.Transfer(Accounts.Alice, account.Address, Assets.WAVES, 0.001m);
            Thread.Sleep(10000);

            var alias = GenerateRandomAlias();
            node.CreateAlias(account, alias, 'T');
            Thread.Sleep(10000);

            var amount = 0.0001m;
            var balanceBefore = node.GetBalance(account.Address);

            node.Transfer(Accounts.Alice, "alias:T:" + alias, Assets.WAVES, amount);
            Thread.Sleep(15000);

            var balanceAfter = node.GetBalance(account.Address);
            Assert.AreEqual(balanceBefore + amount, balanceAfter);
        }

        [TestMethod]
        public void TestMassTransferToAlias()
        {
            var node = new Node(Node.TestNetHost);

            var seed = PrivateKeyAccount.GenerateSeed();
            var account = PrivateKeyAccount.CreateFromSeed(seed, 'T');

            node.Transfer(Accounts.Alice, account.Address, Assets.WAVES, 0.001m);
            Thread.Sleep(10000);

            var alias = GenerateRandomAlias();
            node.CreateAlias(account, alias, 'T');
            Thread.Sleep(10000);

            var amount = 0.0001m;
            var balanceBefore = node.GetBalance(account.Address);

            var recipients = new List<MassTransferItem>
            {
                new MassTransferItem(account.Address, amount),
                new MassTransferItem("alias:T:" + alias, amount),
                new MassTransferItem(account.Address, amount),
                new MassTransferItem(account.Address, amount),
                new MassTransferItem("alias:T:" + alias, amount)
            };

            var tx = new MassTransferTransaction(node.ChainId, Accounts.Alice.PublicKey, Assets.WAVES, recipients);
            tx.Sign(Accounts.Alice);
            node.BroadcastAndWait(tx.GetJsonWithSignature());

            var balanceAfter = node.GetBalance(account.Address);
            Assert.AreEqual(balanceBefore + amount * recipients.Count, balanceAfter);
        }
    }
}
