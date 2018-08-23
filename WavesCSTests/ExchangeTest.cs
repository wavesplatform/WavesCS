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

            var account1 = Accounts.Alice;
            var matcherAccount = Accounts.Bob;

            byte[] senderPublicKey = account1.PublicKey;
            byte[] matcherPublicKey = matcherAccount.PublicKey;

            var asset1 = Assets.WAVES;
            var asset2 = Assets.GetById("FUMBLu8GVgegKf8WYzahTUaiBqxfVsXoxyMrv99vKSeC", node);

            var ts = 1535043887572.ToDate(); //DateTime.Now;
            var price = Asset.LongToPrice(asset1, asset2, 176L);

            decimal amount1 = asset1.LongToAmount(100000000L);
            DateTime expiration1 = (ts.ToLong() + 3000).ToDate();
            decimal matcherFee1 = Assets.WAVES.LongToAmount(300001L);

            Order order1 = new Order(OrderSide.Sell, amount1, price,
                                     ts, 0.1m, OrderStatus.Accepted,
                                     asset1, asset2,
                                     senderPublicKey, matcherPublicKey,
                                     expiration1, matcherFee1,
                                     account1);
            
            decimal amount2 = asset1.LongToAmount(100000000L);
            DateTime expiration2 = (ts.ToLong() + 3001).ToDate();
            decimal matcherFee2 = Assets.WAVES.LongToAmount(300002L);

            Order order2 = new Order(OrderSide.Buy, amount2, price,
                                     ts, 0.1m, OrderStatus.Accepted,
                                     asset1, asset2,
                                     senderPublicKey, matcherPublicKey,
                                     expiration2, matcherFee2,
                                     account1);

            var amount = asset1.LongToAmount(100000000L);
            var buyMatcherFee = Assets.WAVES.LongToAmount(300002L);
            var sellMatcherFee = Assets.WAVES.LongToAmount(300001L);
            var fee = Assets.WAVES.LongToAmount(600003L);
            var timestamp = (ts.ToLong() + 1).ToDate();

            var exchangeTx = new ExchangeTransaction(matcherPublicKey, fee,
                                                     buyMatcherFee, sellMatcherFee,
                                                     asset1, asset2,
                                                     order2, order1,
                                                     amount, price, timestamp);
            exchangeTx.Sign(matcherAccount);

            var response = node.Broadcast(exchangeTx.GetJson());
            Console.WriteLine(response);
        }
    }
}
