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

            var amountAsset = Assets.GetById("5hozySTi6nZtE6SvgmS28MhpPph4hzsfUAP2LjM9Qxod", node);
            var priceAsset = Assets.WAVES;

            decimal amount = 2m;
            var price = Asset.LongToPrice(amountAsset, priceAsset, 100000000L);

            Order sellOrder = new Order(OrderSide.Sell, amount, price,
                                        DateTime.UtcNow,
                                        amountAsset, priceAsset,
                                        Accounts.Alice.PublicKey, Accounts.Carol.PublicKey,
                                        DateTime.UtcNow.AddHours(1),
                                        0.005m,
                                        Accounts.Alice.Address);

            Order buyOrder = new Order(OrderSide.Buy, amount, price,
                                       DateTime.UtcNow,
                                       amountAsset, priceAsset,
                                       Accounts.Bob.PublicKey, Accounts.Carol.PublicKey,
                                       DateTime.UtcNow.AddHours(1),
                                       0.005m,
                                       Accounts.Bob.Address);

            sellOrder.Sign(Accounts.Alice);
            buyOrder.Sign(Accounts.Bob);

            var exchangeTx = new ExchangeTransaction(Accounts.Carol.PublicKey,
                                                     0.007m,
                                                     0.004m, 0.004m,
                                                     amountAsset,priceAsset,
                                                     buyOrder, sellOrder,
                                                     amount, price,
                                                     DateTime.UtcNow.AddSeconds(10));

            Http.Tracing = true;
            var matcher = new Matcher("https://testnode1.wavesnodes.com");
            var aliceBalanceBefore = matcher.GetTradableBalance(Accounts.Alice.Address, amountAsset, priceAsset)[amountAsset];
            var bobBalanceBefore = matcher.GetTradableBalance(Accounts.Bob.Address, amountAsset, priceAsset)[amountAsset];

            exchangeTx.Sign(Accounts.Carol);
            node.Broadcast(exchangeTx.GetJson());

            Thread.Sleep(7000);

            var aliceBalanceAfter = matcher.GetTradableBalance(Accounts.Alice.Address, amountAsset, priceAsset)[amountAsset];
            var bobBalanceAfter = matcher.GetTradableBalance(Accounts.Bob.Address, amountAsset, priceAsset)[amountAsset];

            Assert.IsTrue(aliceBalanceBefore > aliceBalanceAfter);
            Assert.IsTrue(bobBalanceBefore < bobBalanceAfter);
            Assert.AreEqual(aliceBalanceBefore + bobBalanceBefore, aliceBalanceAfter + bobBalanceAfter);
        }
    }
}
