using System;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS;

namespace WavesCSTests
{
    [TestClass]
    public class MatcherTest
    {
        private static readonly Asset WBTC = new Asset("Fmg13HEHJHuZYbtJq8Da8wifJENq8uBxDuWoP9pVe2Qe", "BTC", 8);

        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }
        
        [TestMethod]
        public void TestKey()
        {
            var matcher = new Matcher("https://matcher.wavesnodes.com");

            Assert.AreEqual("7kPFrHDiGw1rCm7LPszuECwWYL3dMf6iMifLRDJQZMzy", matcher.MatcherKey);
            
            Console.WriteLine(new Node().GetBalance(Accounts.Carol.Address));
            Thread.Sleep(3000);
        }

        [TestMethod]
        public void TestOrderBook()
        {
            var matcher = new Matcher("https://matcher.wavesnodes.com");            

            var orderBook = matcher.GetOrderBook(Assets.WAVES, Assets.BTC);
            
            Assert.IsNotNull(orderBook);
            
            Assert.AreEqual((orderBook.Timestamp - DateTime.UtcNow).TotalSeconds, 0, 5);            
            
            Assert.IsTrue(orderBook.Bids.Length > 10);
            Assert.IsTrue(orderBook.Asks.Length > 10);
                       
            Assert.IsTrue(0.0001m < orderBook.Bids[1].Price);
            Assert.IsTrue(orderBook.Bids[1].Price < orderBook.Bids[0].Price);                        
            Assert.IsTrue(orderBook.Bids[0].Price < orderBook.Asks[0].Price);
            Assert.IsTrue(orderBook.Asks[0].Price < orderBook.Asks[1].Price);
            Assert.IsTrue(orderBook.Asks[1].Price < 0.01m);
            
            Assert.IsTrue(orderBook.Bids.Any(b => b.Amount > 100));
            Assert.IsTrue(orderBook.Bids.Any(b => b.Amount < 10));                       
        }

        [TestMethod]
        public void Test2()
        {        

            var t = PrivateKeyAccount.CreateFromSeed("Lvje1NjwcXFoR3SqNn1kwX3igAeNh5MqpPM5YmqhK".FromBase58(), chainId: 'D', nonce: 0).Address;
            Console.WriteLine();
        }

        [TestMethod]
        public void TestTradableBalance()
        {
            var matcher = new Matcher("https://testnode1.wavesnodes.com");

            var balance = matcher.GetTradableBalance(Accounts.Carol.Address, Assets.WAVES, WBTC);

            Assert.AreEqual(2, balance.Count);
            Assert.IsTrue(balance[Assets.WAVES] > 0);
            Assert.IsTrue(balance[WBTC] >= 0);
            
            Console.WriteLine(string.Join(", ", balance.Select(p => $"{p.Key}: {p.Value}")));
        }       

        [TestMethod]
        public void TestOrders()
        {
            var matcher = new Matcher("https://matcher.testnet.wavesnodes.com");

            var priceAsset = Assets.WAVES;
            var amountAsset = WBTC;

            var orderBook = matcher.GetOrderBook(amountAsset, priceAsset);
            var myPrice = orderBook.Asks.FirstOrDefault()?.Price ?? 0 + 0.0001m;

            Order order1 = new Order(OrderSide.Sell, 0.5m, myPrice, DateTime.UtcNow,
                                     amountAsset, priceAsset, Accounts.Carol.PublicKey, matcher.MatcherKey.FromBase58(),
                                     DateTime.UtcNow.AddHours(1), 0.007m , Accounts.Carol.Address, 2);

            matcher.PlaceOrder(Accounts.Carol, order1);
            Thread.Sleep(3000);

            var orders = matcher.GetOrders(Accounts.Carol, amountAsset, priceAsset);

            var lastOrder = orders.OrderBy(o => o.Timestamp).Last();

            Assert.AreEqual(OrderStatus.Accepted, lastOrder.Status);
            Assert.AreEqual(myPrice, lastOrder.Price);
            Assert.AreEqual(0.5m, lastOrder.Amount);
            Assert.AreEqual(OrderSide.Sell, lastOrder.Side);
            Assert.AreEqual(amountAsset, lastOrder.AmountAsset);
            Assert.AreEqual(priceAsset, lastOrder.PriceAsset);
            Assert.AreEqual(0.0, (lastOrder.Timestamp - DateTime.UtcNow).TotalSeconds, 10.0);

            matcher.CancelAll(Accounts.Carol);

            Thread.Sleep(3000);
            
            orders = matcher.GetOrders(Accounts.Carol, amountAsset, priceAsset);
            
            Assert.IsTrue(orders.All(o => o.Status == OrderStatus.Cancelled));
        }

    }
}
