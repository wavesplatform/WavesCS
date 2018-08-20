using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        public ExchangeTransaction(Dictionary<string, object> tx) : base(tx)
        {
            Fee = Assets.WAVES.LongToAmount(tx.GetLong("fee"));

            BuyMatcherFee = Assets.WAVES.LongToAmount(tx.GetLong("buyMatcherFee"));
            SellMatcherFee = Assets.WAVES.LongToAmount(tx.GetLong("sellMatcherFee"));

            if (tx.GetObject("order1").GetObject("assetPair").GetString("amountAsset") != null)
                AmountAsset = Assets.GetById(tx.GetObject("order1")
                                               .GetObject("assetPair")
                                             .GetString("amountAsset"));
            else
                AmountAsset = Assets.WAVES;

            if (tx.GetObject("order1").GetObject("assetPair").GetString("priceAsset") != null)
                PriceAsset = Assets.GetById(tx.GetObject("order1")
                                              .GetObject("assetPair")
                                              .GetString("priceAsset"));
            else
                PriceAsset = Assets.WAVES;

            Order1 = Order.CreateFromJson(tx.GetObject("order1"), AmountAsset, PriceAsset);
            Order2 = Order.CreateFromJson(tx.GetObject("order2"), AmountAsset, PriceAsset);

            Amount = AmountAsset.LongToAmount(tx.GetLong("amount"));
            Price = PriceAsset.LongToAmount(tx.GetLong("price"));
        }

        public override byte[] GetBody()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(TransactionType.Exchange);
                writer.Write(SenderPublicKey);

                writer.Write(Order1.Id);
                writer.Write(Order2.Id);

                writer.WriteLong(PriceAsset.AmountToLong(Price));
                writer.WriteLong(AmountAsset.AmountToLong(Amount));
                writer.WriteLong(Assets.WAVES.AmountToLong(BuyMatcherFee));
                writer.WriteLong(Assets.WAVES.AmountToLong(SellMatcherFee));

                writer.WriteLong(Timestamp.ToLong());
                writer.WriteLong(Assets.WAVES.AmountToLong(Fee));
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
                {"order1", Order1},
                {"order2", Order2},
                {"price", Asset.PriceToLong(AmountAsset, PriceAsset, Price) },
                {"amount", AmountAsset.AmountToLong(Amount) },
                {"buyMatcherFee", Assets.WAVES.AmountToLong(BuyMatcherFee)},
                {"sellMatcherFee", Assets.WAVES.AmountToLong(SellMatcherFee)},
            };
        }

        protected override bool SupportsProofs()
        {
            return true;
        }
    }
}
