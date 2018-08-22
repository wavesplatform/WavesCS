using System;
using System.Threading;
using WavesCS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            var id1 = "123";
            var id2 = "123";

            Decimal amount = 0.001m;
            Decimal price = 0.001m;
            Decimal filled = 0.001m;
            Decimal fee = 0.001m;
            Decimal matcherFee = 0.001m;
            Decimal buyMatcherFee = 0.001m;
            Decimal sellMatcherFee = 0.001m;

            var matcherPublicKey = "7kPFrHDiGw1rCm7LPszuECwWYL3dMf6iMifLRDJQZMzy";

            var expiration = DateTime.Now.Add(new TimeSpan(1, 0, 0));

            Order order1 = new Order(id1, OrderSide.Buy, amount, price, DateTime.Now, filled,
                                     OrderStatus.NotFound, amountAsset, priceAsset,
                                     Accounts.Alice.PublicKey, matcherPublicKey.FromBase58(),
                                     expiration, matcherFee);
            
            Order order2 = new Order(id2, OrderSide.Sell, amount, price, DateTime.Now, filled,
                                     OrderStatus.NotFound, amountAsset, priceAsset,
                                     Accounts.Alice.PublicKey, matcherPublicKey.FromBase58(),
                                     expiration, matcherFee);




            ExchangeTransaction exchangeTx = new ExchangeTransaction(Accounts.Alice.PublicKey,
                                                                     fee, buyMatcherFee, sellMatcherFee,
                                                                     amountAsset, priceAsset,
                                                                     order1, order2,
                                                                     amount, price);
            exchangeTx.Sign(Accounts.Alice);

            var response = node.Broadcast(exchangeTx);
            Console.WriteLine(response);

        }
    }
}
