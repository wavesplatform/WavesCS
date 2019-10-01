using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

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

        public byte[][] Proofs { get; set; }
        public byte[] Signature => Proofs[0];

        public string Sender { get; }
        public byte Version { get; set; } = 2;

        public Order(OrderSide side, decimal amount, decimal price, DateTime timestamp,
            Asset amountAsset, Asset priceAsset, byte[] senderPublicKey, byte[] matcherPublicKey, DateTime expiration,
            decimal matcherFee, string sender, byte version = 2)
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
            Version = version;
            Proofs = new byte[8][];
        }

        protected bool SupportsProofs()
        {
            return Version > 1;
        }

        public static Order CreateFromJson(DictionaryObject json, Asset amountAsset, Asset priceAsset)
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

            var proofs = new byte[8][];
            if (json.ContainsKey("proofs"))
            {
                proofs = json.Get<string[]>("proofs")
                           .Select(item => item.FromBase58())
                           .ToArray();
            }
            else
            {
                if (json.ContainsKey("signature"))
                    proofs[0] = json.GetString("signature").FromBase58();
            }

            var status = json.ContainsKey("status") ? (OrderStatus)Enum.Parse(typeof(OrderStatus), json.GetString("status")) : OrderStatus.Accepted;
            var id = json.ContainsKey("id") ? json.GetString("id") : null;
            var filled = json.ContainsKey("filled") ? amountAsset.LongToAmount(json.GetLong("filled")) : 1m;
            var version = json.ContainsKey("version") ? json.GetByte("version") : (byte)2;

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
                sender, version)
            {
                Proofs = proofs,
                Status = status,
                Id = id,
                Filled = filled
            };
    }
        public byte[] GetBody()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                if (Version > 1)
                    writer.Write(Version);
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

        public byte[] GetProofsBytes()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            var proofs = Proofs
                .Take(Array.FindLastIndex(Proofs, p => p != null && p.Length > 0) + 1)
                .Select(p => p ?? (new byte[0]))
                .ToArray();

            writer.WriteByte(1);
            writer.WriteShort(proofs.Count());

            foreach (var proof in proofs)
            {
                writer.WriteShort(proof.Length);
                writer.Write(proof);
            }

            return stream.ToArray();
        }

        public byte[] GetBytes()
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


        public DictionaryObject GetJson()
        {
            var json = new DictionaryObject
            {
                {"amount", AmountAsset.AmountToLong(Amount)},
                {"price", Asset.PriceToLong(AmountAsset, PriceAsset, Price)},
                {"timestamp", Timestamp.ToLong()},
                {"expiration", Expiration.ToLong()},
                {"senderPublicKey", SenderPublicKey.ToBase58()},
                {"matcherPublicKey", MatcherPublicKey.ToBase58()},
                {"matcherFee", Assets.WAVES.AmountToLong(MatcherFee)},
                {"assetPair", new DictionaryObject
                    {
                        {"amountAsset", AmountAsset.IdOrNull},
                        {"priceAsset", PriceAsset.IdOrNull}
                    }
                },
                {"orderType", Side.ToString().ToLower()},
                {"version", Version}
            };

            var proofs = Proofs
                .Take(Array.FindLastIndex(Proofs, p => p != null && p.Length > 0) + 1)
                .Select(p => p == null ? "" : p.ToBase58())
                .ToArray();

            if (SupportsProofs())
            {
                json.Add("proofs", proofs);
            }
            else
            {
                if (proofs.Length == 0)
                    throw new InvalidOperationException("Order is not signed");
                if (proofs.Length > 1)
                    throw new InvalidOperationException("Order version doesn't support multiple proofs");
                json.Add("signature", proofs.Single());
            }

            return json;
        }
    }

    public static class OrderExtensons
    {
        public static Order Sign(this Order order, PrivateKeyAccount account, int proofIndex = 0)
        {
            order.Proofs[proofIndex] = account.Sign(order.GetBody());
            return order;
        }

        public static string GenerateId(this Order order)
        {
            var bytes = order.GetBody();
            return AddressEncoding.FastHash(bytes, 0, bytes.Length).ToBase58();
        }
    }
}
