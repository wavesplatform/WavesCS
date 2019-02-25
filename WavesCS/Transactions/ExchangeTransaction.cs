using System;
using System.IO;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public class ExchangeTransaction : Transaction
    {
        public decimal BuyMatcherFee { get; }
        public decimal SellMatcherFee { get; }

        public decimal Amount { get; }
        public decimal Price { get; }

        public Asset AmountAsset;
        public Asset PriceAsset;

        public Order BuyOrder;
        public Order SellOrder;
        public override byte Version { get; set; } = 2;

        public ExchangeTransaction(char chainId, byte[] senderPublicKey,
                                   decimal fee, decimal buyMatcherFee,
                                   decimal sellMatcherFee, Asset amountAsset,
                                   Asset priceAsset,
                                   Order buyOrder, Order sellOrder,
                                   decimal amount, decimal price, DateTime timestamp) : base(chainId, senderPublicKey)
        {
            Fee = fee;

            BuyMatcherFee = buyMatcherFee;
            SellMatcherFee = sellMatcherFee;

            AmountAsset = amountAsset;
            PriceAsset = priceAsset;

            BuyOrder = buyOrder;
            SellOrder = sellOrder;

            Amount = amount;
            Price = price;
            Timestamp = timestamp;
        }

        public ExchangeTransaction(DictionaryObject tx) : base(tx)
        {
            var node = new Node(tx.GetChar("chainId"));
            Fee = Assets.WAVES.LongToAmount(tx.GetLong("fee"));

            BuyMatcherFee = Assets.WAVES.LongToAmount(tx.GetLong("buyMatcherFee"));
            SellMatcherFee = Assets.WAVES.LongToAmount(tx.GetLong("sellMatcherFee"));

            AmountAsset = node.GetAsset((tx.GetValue("order1.assetPair.amountAsset") ?? Assets.WAVES.Id).ToString());
            PriceAsset = node.GetAsset((tx.GetValue("order1.assetPair.priceAsset") ?? Assets.WAVES.Id).ToString());

            BuyOrder = Order.CreateFromJson(tx.GetObject("order1"), AmountAsset, PriceAsset);
            SellOrder = Order.CreateFromJson(tx.GetObject("order2"), AmountAsset, PriceAsset);

            Amount = AmountAsset.LongToAmount(tx.GetLong("amount"));
            Price = Asset.LongToPrice(AmountAsset, PriceAsset, tx.GetLong("price"));
        }

        public override byte[] GetBody()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(TransactionType.Exchange);

                var buyOrderBytes = BuyOrder.GetBytes();
                var sellOrderBytes = SellOrder.GetBytes();

                writer.WriteShort(0);
                writer.WriteShort((short)buyOrderBytes.Length + BuyOrder.Signature.Length);
                writer.WriteShort(0);
                writer.WriteShort((short)sellOrderBytes.Length + BuyOrder.Signature.Length);
                writer.Write(buyOrderBytes);
                writer.Write(BuyOrder.Signature);
                writer.Write(sellOrderBytes);
                writer.Write(SellOrder.Signature);
                writer.WriteLong(Asset.PriceToLong(AmountAsset, PriceAsset, Price));
                writer.WriteLong(AmountAsset.AmountToLong(Amount));
                writer.WriteLong(Assets.WAVES.AmountToLong(BuyMatcherFee));
                writer.WriteLong(Assets.WAVES.AmountToLong(SellMatcherFee));
                writer.WriteLong(Assets.WAVES.AmountToLong(Fee));
                writer.WriteLong(Timestamp.ToLong());
                return stream.ToArray();
            }
        }

        public override byte[] GetIdBytes()
        {
            return GetBody();
        }

        public override DictionaryObject GetJson()
        {
            var result = new DictionaryObject
            {
                {"type", (byte) TransactionType.Exchange},
                {"senderPublicKey", SenderPublicKey.ToBase58() },
                {"fee", Assets.WAVES.AmountToLong(Fee)},
                {"timestamp", Timestamp.ToLong()},
                {"order1", BuyOrder.GetJson()},
                {"order2", SellOrder.GetJson()},
                {"price", Asset.PriceToLong(AmountAsset, PriceAsset, Price) },
                {"amount", AmountAsset.AmountToLong(Amount) },
                {"buyMatcherFee", Assets.WAVES.AmountToLong(BuyMatcherFee)},
                {"sellMatcherFee", Assets.WAVES.AmountToLong(SellMatcherFee)}
            };

            if (Sender != null)
                result.Add("sender", Sender);

            return result;
        }

        protected override bool SupportsProofs()
        {
            return false;
        }
    }
}
