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
            var node = new Node(Node.TestNetChainId);

            Asset amountAsset = null;
            try
            {
                amountAsset = node.GetAsset("CVRciuSiK8xiNJSRitAG9dGqcmfFPHvn9bcXtntnpuvp");

                if (node.GetBalance(Accounts.Alice.Address, amountAsset) < 0.1m)
                    throw new Exception();
            }
            catch (Exception)
            {
                amountAsset = node.IssueAsset(Accounts.Alice, "asset", "asset", 1e12m, 6, true);
                Assert.IsNotNull(amountAsset);
                node.WaitTransactionConfirmation(amountAsset.Id);
            }

            var priceAsset = Assets.WAVES;

            decimal amount = amountAsset.LongToAmount(10000);
            decimal price = Asset.LongToPrice(amountAsset, priceAsset, 10000);

            Order sellOrder = new Order(OrderSide.Sell, amount, price,
                                        DateTime.UtcNow,
                                        amountAsset, priceAsset,
                                        Accounts.Alice.PublicKey, Accounts.Carol.PublicKey,
                                        DateTime.UtcNow.AddHours(1),
                                        0.003m,
                                        Accounts.Alice.Address);

            Order buyOrder = new Order(OrderSide.Buy, amount, price,
                                       DateTime.UtcNow,
                                       amountAsset, priceAsset,
                                       Accounts.Bob.PublicKey, Accounts.Carol.PublicKey,
                                       DateTime.UtcNow.AddHours(1),
                                       0.003m,
                                       Accounts.Bob.Address);

            sellOrder.Sign(Accounts.Alice);
            buyOrder.Sign(Accounts.Bob);

            var exchangeTx = new ExchangeTransaction(node.ChainId, Accounts.Carol.PublicKey,
                                                     0.003m,
                                                     0.003m, 0.003m,
                                                     amountAsset,priceAsset,
                                                     buyOrder, sellOrder,
                                                     amount, price,
                                                     DateTime.UtcNow.AddSeconds(10));

            Http.Tracing = true;
            var matcher = new Matcher("https://matcher-testnet.wavesnodes.com");
            var aliceBalanceBefore = matcher.GetTradableBalance(Accounts.Alice.Address, amountAsset, priceAsset)[amountAsset];
            var bobBalanceBefore = matcher.GetTradableBalance(Accounts.Bob.Address, amountAsset, priceAsset)[amountAsset];

            exchangeTx.Sign(Accounts.Carol);
            node.BroadcastAndWait(exchangeTx);


            var aliceBalanceAfter = matcher.GetTradableBalance(Accounts.Alice.Address, amountAsset, priceAsset)[amountAsset];
            var bobBalanceAfter = matcher.GetTradableBalance(Accounts.Bob.Address, amountAsset, priceAsset)[amountAsset];

            Assert.IsTrue(aliceBalanceBefore > aliceBalanceAfter);
            Assert.IsTrue(bobBalanceBefore < bobBalanceAfter);
            Assert.AreEqual(aliceBalanceBefore + bobBalanceBefore, aliceBalanceAfter + bobBalanceAfter);
        }
    }
}
