using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public class ExchangeTransaction : Transaction
    {

        public decimal Fee { get; }
        public decimal BuyMatcherFee { get; }
        public decimal SellMatcherFee { get; }

        public decimal Amount { get; }
        public decimal Price { get; }

        public Asset AmountAsset;
        public Asset PriceAsset;

        public Order Order1;
        public Order Order2;

        public ExchangeTransaction(byte[] senderPublicKey,
                                   decimal fee, decimal buyMatcherFee,
                                   decimal sellMatcherFee, Asset amountAsset,
                                   Asset priceAsset,
                                   Order order1, Order order2,
                                   decimal amount, decimal price, DateTime timestamp) : base(senderPublicKey)
        {
            Fee = fee;

            BuyMatcherFee = buyMatcherFee;
            SellMatcherFee = sellMatcherFee;

            AmountAsset = amountAsset;
            PriceAsset = priceAsset;

            Order1 = order1;
            Order2 = order2;

            Amount = amount;
            Price = price;
            Timestamp = timestamp;
        }

        public ExchangeTransaction(Dictionary<string, object> tx) : base(tx)
        {
            Fee = Assets.WAVES.LongToAmount(tx.GetLong("fee"));

            BuyMatcherFee = Assets.WAVES.LongToAmount(tx.GetLong("buyMatcherFee"));
            SellMatcherFee = Assets.WAVES.LongToAmount(tx.GetLong("sellMatcherFee"));

            AmountAsset = Assets.GetById((tx.GetValue("order1.assetPair.amountAsset") ?? Assets.WAVES.Id).ToString());
            PriceAsset = Assets.GetById((tx.GetValue("order1.assetPair.priceAsset") ?? Assets.WAVES.Id).ToString());

            Order1 = Order.CreateFromJson(tx.GetObject("order1"), AmountAsset, PriceAsset);
            Order2 = Order.CreateFromJson(tx.GetObject("order2"), AmountAsset, PriceAsset);

            Amount = AmountAsset.LongToAmount(tx.GetLong("amount"));
            Price = Asset.LongToPrice(AmountAsset, PriceAsset, tx.GetLong("price"));
        }

        public override byte[] GetBody()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                var buyOrder = Order1;
                var sellOrder = Order2;

                if (Order1.Side == OrderSide.Sell)
                {
                    buyOrder = Order2;
                    sellOrder = Order1;
                }

                writer.Write(TransactionType.Exchange);

                var buyOrderBytes = buyOrder.GetBytes();
                var sellOrderBytes = sellOrder.GetBytes();

                writer.WriteShort(0);
                writer.WriteShort((short)buyOrderBytes.Length + buyOrder.Signature.Length);
                writer.WriteShort(0);
                writer.WriteShort((short)sellOrderBytes.Length + buyOrder.Signature.Length);
                writer.Write(buyOrder.GetBytes());
                writer.Write(buyOrder.Signature);
                writer.Write(sellOrder.GetBytes());
                writer.Write(sellOrder.Signature);
                writer.WriteLong(Asset.PriceToLong(AmountAsset, PriceAsset, Price));
                writer.WriteLong(AmountAsset.AmountToLong(Amount));
                writer.WriteLong(Assets.WAVES.AmountToLong(BuyMatcherFee));
                writer.WriteLong(Assets.WAVES.AmountToLong(SellMatcherFee));
                writer.WriteLong(Assets.WAVES.AmountToLong(Fee));
                writer.WriteLong(Timestamp.ToLong());
                return stream.ToArray();
            }
        }

        public override Dictionary<string, object> GetJson()
        {
            return new Dictionary<string, object>
            {
                {"type", TransactionType.Exchange},
                {"senderPublicKey", SenderPublicKey.ToBase58() },
                {"fee", Assets.WAVES.AmountToLong(Fee)},
                {"timestamp", Timestamp.ToLong()},
                {"order1", Order1.GetJson()},
                {"order2", Order2.GetJson()},
                {"price", Asset.PriceToLong(AmountAsset, PriceAsset, Price) },
                {"amount", AmountAsset.AmountToLong(Amount) },
                {"buyMatcherFee", Assets.WAVES.AmountToLong(BuyMatcherFee)},
                {"signature", Proofs[0].ToBase58()},
                {"sellMatcherFee", Assets.WAVES.AmountToLong(SellMatcherFee)}
            };
        }

        protected override bool SupportsProofs()
        {
            return true;
        }
    }
}
