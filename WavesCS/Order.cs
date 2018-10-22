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
        public string Id { get; set;  }
        public OrderSide Side { get; }
        public decimal Amount { get; }
        public decimal Price { get; }
        public DateTime Timestamp { get; }
        public DateTime Expiration { get; set; }
        public decimal Filled { get; set; }
        public OrderStatus Status { get; set; }
        public Asset AmountAsset { get; }
        public Asset PriceAsset { get; }
        public byte[] SenderPublicKey { get; }
        public byte[] MatcherPublicKey { get; }

        public decimal MatcherFee { get; }

        public byte[] Signature { get; set; }

        public string Sender { get; }

        public Order(OrderSide side, decimal amount, decimal price, DateTime timestamp,
            Asset amountAsset, Asset priceAsset, byte[] senderPublicKey, byte[] matcherPublicKey, DateTime expiration,
            decimal matcherFee, string sender)
        {
            SenderPublicKey = senderPublicKey;
            MatcherPublicKey = matcherPublicKey;
            Side = side;
            Amount = amount;
            Price = price;
            Timestamp = timestamp;
            Expiration = expiration;
            AmountAsset = amountAsset;
            PriceAsset = priceAsset;
            MatcherFee = matcherFee;
            Sender = sender;
        }

        public static Order CreateFromJson(Dictionary<string, object> json, Asset amountAsset, Asset priceAsset)
        {
            var side = OrderSide.Buy;

            if (json.ContainsKey("orderType"))
                side = json.GetString("orderType") == "buy" ? OrderSide.Buy : OrderSide.Sell;
            else
                side = (OrderSide)Enum.Parse(typeof(OrderSide), json.GetString("type"), true);

            var senderPublicKey = json.ContainsKey("senderPublicKey") ? json.GetString("senderPublicKey") : "";
            var matcherPublicKey = json.ContainsKey("matcherPublicKey") ? json.GetString("matcherPublicKey") : "";
            var expiration = json.ContainsKey("expiration") ? json.GetDate("expiration") : json.GetDate("timestamp");
            var matcherFee = json.ContainsKey("matcherFee") ? Assets.WAVES.LongToAmount(json.GetLong("matcherFee")) : 1;
            string sender = json.ContainsKey("sender") ? json.GetString("sender") : null;

            var signature = json.ContainsKey("signature") ? json.GetString("signature").FromBase58() : null;
            var status = json.ContainsKey("status") ? (OrderStatus)Enum.Parse(typeof(OrderStatus), json.GetString("status")) : OrderStatus.Accepted;
            var id = json.ContainsKey("id") ? json.GetString("id") : null;
            var filled = json.ContainsKey("filled") ? amountAsset.LongToAmount(json.GetLong("filled")) : 1m;

            return new Order(
                side,
                amountAsset.LongToAmount(json.GetLong("amount")),
                Asset.LongToPrice(amountAsset, priceAsset, json.GetLong("price")),
                json.GetDate("timestamp"),
                amountAsset,
                priceAsset,
                senderPublicKey.FromBase58(),
                matcherPublicKey.FromBase58(),
                expiration,
                matcherFee,
                sender)
            {
                Signature = signature,
                Status = status,
                Id = id,
                Filled = filled
            };
    }
        public byte[] GetBytes()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(SenderPublicKey);
                writer.Write(MatcherPublicKey);
                writer.WriteAsset(AmountAsset.Id);
                writer.WriteAsset(PriceAsset.Id);
                writer.Write(Side == OrderSide.Buy ? (byte)0x0 : (byte)0x1);
                writer.WriteLong(Asset.PriceToLong(AmountAsset, PriceAsset, Price));
                writer.WriteLong(AmountAsset.AmountToLong(Amount));
                writer.WriteLong(Timestamp.ToLong());
                writer.WriteLong(Expiration.ToLong());
                writer.WriteLong(Assets.WAVES.AmountToLong(MatcherFee));

                return stream.ToArray();
            }
        }

        public Dictionary<string, object> GetJson()
        {
            return new Dictionary<string, object>
            {
                {"amount", AmountAsset.AmountToLong(Amount)},
                {"price", Asset.PriceToLong(AmountAsset, PriceAsset, Price)},
                {"timestamp", Timestamp.ToLong()},
                {"expiration", Expiration.ToLong()},
                {"senderPublicKey", SenderPublicKey.ToBase58()},
                {"matcherPublicKey", MatcherPublicKey.ToBase58()},
                {"matcherFee", Assets.WAVES.AmountToLong(MatcherFee)},
                {"assetPair", new Dictionary<string, object>
                    {
                        {"amountAsset", AmountAsset.IdOrNull},
                        {"priceAsset", PriceAsset.IdOrNull}
                    }
                },
                {"orderType", Side.ToString().ToLower()},
                {"signature", Signature.ToBase58()}
            };
        }
    }

    public static class OrderExtensons
    {
        public static Order Sign(this Order order, PrivateKeyAccount account)
        {
            order.Signature = account.Sign(order.GetBytes());
            return order;
        }

        public static string GenerateId(this Order order)
        {
            var bytes = order.GetBytes();
            return AddressEncoding.FastHash(bytes, 0, bytes.Length).ToBase58();
        }
    }
}
