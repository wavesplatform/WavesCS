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
        private static readonly string WBTC = "Fmg13HEHJHuZYbtJq8Da8wifJENq8uBxDuWoP9pVe2Qe";
//
//        private static readonly PrivateKeyAccount alice = PrivateKeyAccount.CreateFromPrivateKey("CMLwxbMZJMztyTJ6Zkos66cgU7DybfFJfyJtTVpme54t", AddressEncoding.TestNet);
//        private static readonly PrivateKeyAccount bob = PrivateKeyAccount.CreateFromPrivateKey("25Um7fKYkySZnweUEVAn9RLtxN5xHRd7iqpqYSMNQEeT", AddressEncoding.TestNet);

        private static readonly PrivateKeyAccount account = PrivateKeyAccount.CreateFromSeed(
            "general rose scissors hybrid clutch method era habit client caught toward actress pilot infant theme",
            AddressEncoding.TestNet);
        
        
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestKey()
        {
            var matcher = new Matcher("https://matcher.wavesnodes.com");

            Assert.AreEqual("7kPFrHDiGw1rCm7LPszuECwWYL3dMf6iMifLRDJQZMzy", matcher.MatcherKey);
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
                       
            Assert.IsTrue(10000 < orderBook.Bids[1].Price);
            Assert.IsTrue(orderBook.Bids[1].Price < orderBook.Bids[0].Price);                        
            Assert.IsTrue(orderBook.Bids[0].Price < orderBook.Asks[0].Price);
            Assert.IsTrue(orderBook.Asks[0].Price < orderBook.Asks[1].Price);
            Assert.IsTrue(orderBook.Asks[1].Price < 1000000);
            
            Assert.IsTrue(orderBook.Bids.Any(b => b.Amount > 100*100000000L));
            Assert.IsTrue(orderBook.Bids.Any(b => b.Amount < 10*100000000L));                       
        }

        [TestMethod]
        public void TestTradableBalance()
        {
            var matcher = new Matcher("https://testnet1.wavesnodes.com");

            var balance = matcher.GetTradableBalance(account.Address, null, WBTC);

            Assert.AreEqual(2, balance.Count);
            Assert.IsTrue(balance["WAVES"] > 0);
            Assert.IsTrue(balance[WBTC] >= 0);
            
            TestContext.WriteLine(string.Join(", ", balance.Select(p => $"{p.Key}: {p.Value}")));
        }
            
        
        [TestMethod]
        public void TestOrders()
        {            
            Api.DataProcessed += s => TestContext.WriteLine(s);

            var matcher = new Matcher("https://testnet1.wavesnodes.com");

            var orderBook = matcher.GetOrderBook(null, WBTC);
            var myPrice = orderBook.Asks.First().Price + 100000;
            
            matcher.PlaceOrder(account, OrderSide.Sell, null, WBTC, myPrice, 50000000, DateTime.UtcNow.AddHours(1));

            Thread.Sleep(3000);
            
            var orders = matcher.GetOrders(account, null, WBTC);

            var lastOrder = orders.OrderBy(o => o.Timestamp).Last();

            Assert.AreEqual(OrderStatus.Accepted, lastOrder.Status);
            Assert.AreEqual(myPrice, lastOrder.Price);
            Assert.AreEqual(50000000, lastOrder.Amount);
            Assert.AreEqual(OrderSide.Sell, lastOrder.Side);
            Assert.AreEqual(null, lastOrder.AmountAsset);
            Assert.AreEqual(WBTC, lastOrder.PriceAsset);
            Assert.AreEqual(0.0, (lastOrder.Timestamp - DateTime.UtcNow).TotalSeconds, 10.0);

            foreach (var order in orders.Where(o => o.Status == OrderStatus.Accepted || o.Status == OrderStatus.PartiallyFilled))
            {
                matcher.CancelOrder(account, null, WBTC, order.Id);
            }
            
            Thread.Sleep(3000);
            
            orders = matcher.GetOrders(account, null, WBTC);
            
            Assert.IsTrue(orders.All(o => o.Status == OrderStatus.Cancelled));

            foreach (var order in orders)
            {
                matcher.DeleteOrder(account, null, WBTC, order.Id);
            }
            
            Thread.Sleep(3000);

            orders = matcher.GetOrders(account, null, WBTC);
            
            Assert.IsFalse(orders.Any());
        }

    }
}
