using System;
using System.Collections.Generic;
using System.IO;

namespace WavesCS
{

    public enum OrderStatus
    {
        Accepted,
        Filled,
        PartiallyFilled,
        Cancelled,
        NotFound
    }
    
    public class Order
    {
        public string Id { get; }
        public OrderSide Side { get; }
        public decimal Amount { get; }
        public decimal Price { get; }
        public DateTime Timestamp { get; }
        public DateTime Expiration { get; }
        public decimal Filled { get; }
        public OrderStatus Status { get; }
        public Asset AmountAsset { get; }
        public Asset PriceAsset { get; }
        public byte[] SenderPublicKey { get; }
        public byte[] MatcherPublicKey { get; }

        public decimal MatcherFee { get; }

        public Order(
            string id, OrderSide side, decimal amount, decimal price, DateTime timestamp, decimal filled, OrderStatus status,
            Asset amountAsset, Asset priceAsset, byte[] senderPublicKey, byte[] matcherPublicKey, DateTime expiration, decimal matcherFee)
        {
            SenderPublicKey = senderPublicKey;
            MatcherPublicKey = matcherPublicKey;
            Id = id;
            Side = side;
            Amount = amount;
            Price = price;
            Timestamp = timestamp;
            Expiration = expiration;
            Filled = filled;
            Status = status;
            AmountAsset = amountAsset;
            PriceAsset = priceAsset;
            MatcherFee = matcherFee;
        }

        public static Order CreateFromJson(Dictionary<string, object> json, Asset amountAsset, Asset priceAsset)
        {
            var status = OrderStatus.NotFound;

            var side = OrderSide.Buy;
            if (json.ContainsKey("orderType"))
                side = json.GetString("orderType") == "buy" ? OrderSide.Buy : OrderSide.Sell;
            else
                side = (OrderSide)Enum.Parse(typeof(OrderSide), json.GetString("type"), true);

            var filled = json.ContainsKey("filled") ? amountAsset.LongToAmount(json.GetLong("filled")) : 1;

            if (json.ContainsKey("status"))
                status = (OrderStatus) Enum.Parse(typeof(OrderStatus), json.GetString("status"));

            var senderPublicKey = json.ContainsKey("senderPublicKey") ? json.GetString("senderPublicKey") : "";
            var matcherPublicKey = json.ContainsKey("matcherPublicKey") ? json.GetString("matcherPublicKey") : "";
            var expiration = json.ContainsKey("expiration") ? json.GetDate("expiration") : json.GetDate("timestamp");
            var matcherFee = json.ContainsKey("matcherFee") ? Assets.WAVES.LongToAmount(json.GetLong("matcherFee")) : 1;

            return new Order(
                json.GetString("id"),
                side,
                amountAsset.LongToAmount(json.GetLong("amount")),
                Asset.LongToPrice(amountAsset, priceAsset, json.GetLong("price")),
                json.GetDate("timestamp"),
                filled,
                status,
                amountAsset,
                priceAsset,
                senderPublicKey.FromBase58(),
                matcherPublicKey.FromBase58(),
                expiration,
                matcherFee);
        }

        public byte[] GetBytes()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(SenderPublicKey);
                writer.Write(MatcherPublicKey);
                writer.Write((byte)0x01);
                writer.Write(AmountAsset.Id.FromBase58());
                writer.Write((byte)0x01);
                writer.Write(PriceAsset.Id.FromBase58());
                writer.Write(Side == OrderSide.Buy ? 0x00 : 0x01);
                writer.WriteLong(PriceAsset.AmountToLong(Price));
                writer.WriteLong(AmountAsset.AmountToLong(Amount));
                writer.WriteLong(Timestamp.ToLong());
                writer.WriteLong(Expiration.ToLong());
                writer.WriteLong(Assets.WAVES.AmountToLong(MatcherFee));
                return stream.ToArray();
            }
        }
    }
}
