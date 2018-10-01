using System;
using System.Threading;
using WavesCS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.IO;
using System.Text;

namespace WavesCSTests
{
    [TestClass]
    public class ExchangeTest
    {
        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }

        [TestMethod]
        public void TestExchangeTransaction()
        {
            var node = new Node(Node.TestNetHost);

            var asset1 = Assets.WAVES;
            var asset2 = Assets.GetById("FUMBLu8GVgegKf8WYzahTUaiBqxfVsXoxyMrv99vKSeC", node);

            var price = Asset.LongToPrice(asset1, asset2, 176L);
            decimal amount = 1m;

            Order sellOrder = new Order(OrderSide.Sell, amount, price,
                                        DateTime.UtcNow,
                                        asset1, asset2,
                                        Accounts.Alice.PublicKey, Accounts.Carol.PublicKey,
                                        DateTime.UtcNow.AddHours(1),
                                        0.005m,
                                        Accounts.Alice.Address);

            sellOrder.Sign(Accounts.Alice);

            Order buyOrder = new Order(OrderSide.Buy, amount, price,
                                       DateTime.UtcNow,
                                       asset1, asset2,
                                       Accounts.Bob.PublicKey, Accounts.Carol.PublicKey,
                                       DateTime.UtcNow.AddHours(1),
                                       0.005m,
                                       Accounts.Bob.Address);

            buyOrder.Sign(Accounts.Bob);

            var exchangeTx = new ExchangeTransaction(Accounts.Carol.PublicKey,
                                                     0.004m,
                                                     0.004m, 0.004m,
                                                     asset1, asset2,
                                                     buyOrder, sellOrder,
                                                     amount, price,
                                                     DateTime.UtcNow.AddSeconds(10));

            var matcher = new Matcher("https://testnet2.wavesnodes.com");
            var aliceBalanceBefore = matcher.GetTradableBalance(Accounts.Alice.Address, asset1, asset2)[asset2];
            var bobBalanceBefore = matcher.GetTradableBalance(Accounts.Bob.Address, asset1, asset2)[asset2];

            exchangeTx.Sign(Accounts.Carol);
            node.Broadcast(exchangeTx.GetJson());

            Thread.Sleep(10000);

            var aliceBalanceAfter = matcher.GetTradableBalance(Accounts.Alice.Address, asset1, asset2)[asset2];
            var bobBalanceAfter = matcher.GetTradableBalance(Accounts.Bob.Address, asset1, asset2)[asset2];

            Assert.IsTrue(aliceBalanceBefore < aliceBalanceAfter);
            Assert.IsTrue(bobBalanceBefore > bobBalanceAfter);
            Assert.AreEqual(aliceBalanceBefore + bobBalanceBefore, aliceBalanceAfter + bobBalanceAfter);
        }
    }
}
