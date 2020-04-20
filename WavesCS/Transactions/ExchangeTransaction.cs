﻿using System;
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

        public ExchangeTransaction(DictionaryObject tx, Node node) : base(tx)
        {
            Fee = Assets.WAVES.LongToAmount(tx.GetLong("fee"));

            BuyMatcherFee = Assets.WAVES.LongToAmount(tx.GetLong("buyMatcherFee"));
            SellMatcherFee = Assets.WAVES.LongToAmount(tx.GetLong("sellMatcherFee"));

            AmountAsset = node.GetAsset((tx.GetValue("order1.assetPair.amountAsset") ?? Assets.WAVES.Id).ToString());
            PriceAsset = node.GetAsset((tx.GetValue("order1.assetPair.priceAsset") ?? Assets.WAVES.Id).ToString());

            BuyOrder = Order.CreateFromJson(tx.GetObject("order1"), AmountAsset, PriceAsset);
            SellOrder = Order.CreateFromJson(tx.GetObject("order2"), AmountAsset, PriceAsset);

            Amount = AmountAsset.LongToAmount(tx.GetLong("amount"));
            Price = Asset.LongToPrice(AmountAsset, PriceAsset, tx.GetLong("price"));
            Version = tx.GetByte("version");
        }

        public override byte[] GetBody()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                if (Version > 1)
                {
                    writer.WriteByte(0);
                }

                writer.Write(TransactionType.Exchange);

                if (Version > 1)
                {
                    writer.WriteByte(Version);
                }

                if (Version == 1)
                {
                    var buyOrderBytes = BuyOrder.GetBytes();
                    var sellOrderBytes = SellOrder.GetBytes();

                    writer.WriteShort(0);
                    writer.WriteShort((short)buyOrderBytes.Length);
                    writer.WriteShort(0);
                    writer.WriteShort((short)sellOrderBytes.Length);
                    writer.Write(buyOrderBytes);
                    writer.Write(sellOrderBytes);
                }
                else
                {
                    var buyOrderBytes = BuyOrder.GetBytes();
                    var sellOrderBytes = SellOrder.GetBytes();

                    writer.WriteShort(0);
                    writer.WriteShort((short)buyOrderBytes.Length);

                    if (BuyOrder.Version == 1)
                        writer.WriteByte(1);

                    writer.Write(buyOrderBytes);

                    writer.WriteShort(0);
                    writer.WriteShort((short)sellOrderBytes.Length);

                    if (SellOrder.Version == 1)
                        writer.WriteByte(1);

                    writer.Write(sellOrderBytes);
                }


                writer.WriteLong(Asset.PriceToLong(AmountAsset, PriceAsset, Price));
                writer.WriteLong(AmountAsset.AmountToLong(Amount));
                writer.WriteLong(Assets.WAVES.AmountToLong(BuyMatcherFee));
                writer.WriteLong(Assets.WAVES.AmountToLong(SellMatcherFee));
                writer.WriteLong(Assets.WAVES.AmountToLong(Fee));
                writer.WriteLong(Timestamp.ToLong());
                return stream.ToArray();
            }
        }

        public override byte[] GetBytes()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write(GetBody());

            if (Version == 1)
                writer.Write(Proofs[0]);
            else
                writer.Write(GetProofsBytes());

            return stream.ToArray();
        }

        public override DictionaryObject GetJson()
        {
            var result = new DictionaryObject
            {
                {"version", Version },
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
            return Version > 1;
        }
    }
}
