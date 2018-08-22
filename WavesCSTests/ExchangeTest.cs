using System;
using System.Threading;
using WavesCS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

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
            var node = new Node();

            var priceAsset = Assets.WAVES;
            var amountAsset = Assets.WAVES;

            Asset WBTC = new Asset("Fmg13HEHJHuZYbtJq8Da8wifJENq8uBxDuWoP9pVe2Qe", "BTC", 8);

            var matcher = new Matcher("https://testnet2.wavesnodes.com");
            var orderBook = matcher.GetOrderBook(Assets.WAVES, WBTC);
            var myPrice = orderBook.Asks.FirstOrDefault()?.Price ?? 0 + 0.0001m;
            var amount = 0.5m;

            matcher.PlaceOrder(Accounts.Alice, OrderSide.Buy, Assets.WAVES, WBTC, myPrice, amount, DateTime.UtcNow.AddHours(1));
            Thread.Sleep(3000);
            var order1 = matcher.GetOrders(Accounts.Carol, Assets.WAVES, WBTC)
                                .OrderBy(o => o.Timestamp).Last();

            matcher.PlaceOrder(Accounts.Carol, OrderSide.Sell, Assets.WAVES, WBTC, myPrice, amount, DateTime.UtcNow.AddHours(1));
            Thread.Sleep(3000);
            var order2 = matcher.GetOrders(Accounts.Carol, Assets.WAVES, WBTC)
                                .OrderBy(o => o.Timestamp).Last();

            var fee = 0.001m;
            var buyMatcherFee = 0.001m;
            var sellMatcherFee = 0.001m;

            ExchangeTransaction exchangeTx = new ExchangeTransaction(Accounts.Alice.PublicKey,
                                                                     fee, buyMatcherFee, sellMatcherFee,
                                                                     amountAsset, priceAsset,
                                                                     order1, order2,
                                                                     amount, myPrice);
            var json = exchangeTx.GetJson();

            exchangeTx.Sign(Accounts.Alice);

            var response = node.Broadcast(exchangeTx);
            Console.WriteLine(response);

        }
    }
}
