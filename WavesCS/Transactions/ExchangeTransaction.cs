using System;
using System.Collections.Generic;
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

            AmountAsset = Assets.GetById((tx.GetValue("order1.assetPair.amountAsset") ?? Assets.WAVES.Id).ToString());
            PriceAsset = Assets.GetById((tx.GetValue("order1.assetPair.priceAsset") ?? Assets.WAVES.Id).ToString());

            Order1 = Order.CreateFromJson(tx.GetObject("order1"), AmountAsset, PriceAsset);
            Order2 = Order.CreateFromJson(tx.GetObject("order2"), AmountAsset, PriceAsset);

            Amount = AmountAsset.LongToAmount(tx.GetLong("amount"));
            Price = PriceAsset.LongToAmount(tx.GetLong("price"));
        }

        public override byte[] GetBody()
        {
            
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
            return false;
        }
    }
}
