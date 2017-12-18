using System;
using System.Collections.Generic;
using System.Linq;
using org.whispersystems.curve25519;
using System.IO;
using System.Text;

namespace WavesCS
{
    public class Transaction
    {
        private static readonly byte Issue = 3;
        private static readonly byte Transfer = 4;
        private static readonly byte Reissue = 5;
        private static readonly byte Burn = 6;
        private static readonly byte Lease = 8;
        private static readonly byte LeaseCancel = 9;
        private static readonly byte Alias = 10;

        private static readonly int MinBufferSize = 120;
        private static readonly Curve25519 cipher = Curve25519.getInstance(Curve25519.BEST);

        private readonly string endpoint;
        private readonly Dictionary<string, Object> data;

        Transaction(String endpoint, params Object[] items)
        {
            this.endpoint = endpoint;
            Dictionary<String, Object> map = new Dictionary<string, object>();
            for (int i = 0; i < items.Length; i += 2)
            {
                Object value = items[i + 1];
                if (value != null)
                {
                    map[(String)items[i]] = value;
                }
            }
            //this.data = map.ToLookup(kv => kv.Key, kv => kv.Value);
            this.data = map.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public String Endpoint
        {
            get{ return endpoint; }
        }

        public Dictionary<String, Object> Data
        {
            get { return data; }
        }        

        public static String Sign(PrivateKeyAccount account, MemoryStream stream)
        {
            byte[] bytesToSign = new byte[stream.Position];
            bytesToSign = stream.ToArray();
            byte[] signature = cipher.calculateSignature(account.PrivateKey, bytesToSign);
            return Base58.Encode(signature);
        }

        private static void PutAsset(MemoryStream stream, String assetId)
        {
            if (assetId == null || assetId == string.Empty)
            {
                stream.WriteByte((byte)0);
            }
            else
            {
                stream.WriteByte((byte)1);
                var decoded = Base58.Decode(assetId);
                stream.Write(decoded, 0, decoded.Length);
            }
        }

        public static String NormalizeAsset(String assetId)
        {
            return assetId == null || assetId == string.Empty ? "WAVES" : assetId;
        }

        public static Transaction MakeIssueTransaction(PrivateKeyAccount account,
                String name, String description, long quantity, int decimals, bool reissuable, long fee)
        {
            long timestamp = Utils.CurrentTimestamp();
            int desclen = description == null ? 0 : description.Length;
            MemoryStream stream = new MemoryStream(MinBufferSize + name.Length + desclen);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(Issue);
            writer.Write(account.PublicKey);
            Utils.WriteToNetwork(writer, (short)name.Length);
            writer.Write(Encoding.ASCII.GetBytes(name));
            Utils.WriteToNetwork(writer, (short)desclen);
            if (desclen > 0)
            {
                writer.Write(Encoding.ASCII.GetBytes(description));
            }
            Utils.WriteToNetwork(writer, quantity);
            writer.Write((byte)decimals);
            writer.Write((byte)(reissuable ? 1 : 0));
            Utils.WriteToNetwork(writer, fee);
            Utils.WriteToNetwork(writer, timestamp);        
           

            String signature = Sign(account, stream);
            return new Transaction("assets/broadcast/issue",
                    "senderPublicKey", Base58.Encode(account.PublicKey),
                    "signature", signature,
                    "name", name,
                    "description", description,
                    "quantity", quantity,
                    "decimals", decimals,
                    "reissuable", reissuable,
                    "fee", fee,
                    "timestamp", timestamp);
        }

        public static Transaction MakeReissueTransaction(PrivateKeyAccount account, String assetId, long quantity, bool reissuable, long fee)
        {
            long timestamp = Utils.CurrentTimestamp();
            MemoryStream stream = new MemoryStream(MinBufferSize);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(Reissue);
            writer.Write(account.PublicKey);
            writer.Write(Base58.Decode(assetId));
            Utils.WriteToNetwork(writer, quantity);
            writer.Write((byte)(reissuable ? 1 : 0));
            Utils.WriteToNetwork(writer, fee);
            Utils.WriteToNetwork(writer, timestamp);
            String signature = Sign(account, stream);
            return new Transaction("assets/broadcast/reissue",
                    "senderPublicKey", Base58.Encode(account.PublicKey),
                    "signature", signature,
                    "assetId", assetId,
                    "quantity", quantity,
                    "reissuable", reissuable,
                    "fee", fee,
                    "timestamp", timestamp);
        }

        public static Transaction MakeTransferTransaction(PrivateKeyAccount account, String toAddress,
           long amount, String assetId, long fee, String feeAssetId, String attachment)
        {
            byte[] attachmentBytes = Encoding.UTF8.GetBytes(attachment == null ? "" : attachment);
            int datalen = (assetId == null ? 0 : 32) +
                    (feeAssetId == null ? 0 : 32) +
                    attachmentBytes.Length + MinBufferSize;           
            long timestamp = Utils.CurrentTimestamp();

            MemoryStream stream = new MemoryStream(datalen);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(Transfer);
            writer.Write(account.PublicKey);
            PutAsset(stream, assetId);
            PutAsset(stream, feeAssetId);
            Utils.WriteToNetwork(writer, timestamp);
            Utils.WriteToNetwork(writer, amount);
            Utils.WriteToNetwork(writer, fee);
            writer.Write(Base58.Decode(toAddress));
            //writer.Write((short)attachmentBytes.Length);
            Utils.WriteToNetwork(writer, (short)attachmentBytes.Length);
            writer.Write(attachmentBytes);
            String signature = Sign(account, stream);
            return new Transaction("assets/broadcast/transfer",
                    "senderPublicKey", Base58.Encode(account.PublicKey),
                    "signature", signature,
                    "recipient", toAddress,
                    "amount", amount,
                    "assetId", assetId,
                    "fee", fee,
                    "feeAssetId", feeAssetId,
                    "timestamp", timestamp,
                    "attachment", Base58.Encode(attachmentBytes));
        }

        public static Transaction MakeBurnTransaction(PrivateKeyAccount account, String assetId, long amount, long fee)
        {
            long timestamp = Utils.CurrentTimestamp();
            MemoryStream stream = new MemoryStream(MinBufferSize);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(Burn);
            writer.Write(account.PublicKey);
            writer.Write(Base58.Decode(assetId));
            Utils.WriteToNetwork(writer, amount);
            Utils.WriteToNetwork(writer, fee);
            Utils.WriteToNetwork(writer, timestamp);
            String signature = Sign(account, stream);
            return new Transaction("assets/broadcast/burn",
                    "senderPublicKey", Base58.Encode(account.PublicKey),
                    "signature", signature,
                    "assetId", assetId,
                    "quantity", amount,
                    "fee", fee,
                    "timestamp", timestamp);
        }

        public static Transaction MakeLeaseTransaction(PrivateKeyAccount account, String toAddress, long amount, long fee)
        {
            long timestamp = Utils.CurrentTimestamp();
            MemoryStream stream = new MemoryStream(MinBufferSize);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(Lease);
            writer.Write(account.PublicKey);
            writer.Write(Base58.Decode(toAddress));
            Utils.WriteToNetwork(writer, amount);
            Utils.WriteToNetwork(writer, fee);
            Utils.WriteToNetwork(writer, timestamp);
            String signature = Sign(account, stream);
            return new Transaction("leasing/broadcast/lease",
                    "senderPublicKey", Base58.Encode(account.PublicKey),
                    "signature", signature,
                    "recipient", toAddress,
                    "amount", amount,
                    "fee", fee,
                    "timestamp", timestamp);
        }

        public static Transaction MakeLeaseCancelTransaction(PrivateKeyAccount account, String TransactionId, long fee)
        {
            long timestamp = Utils.CurrentTimestamp();
            MemoryStream stream = new MemoryStream(MinBufferSize);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(LeaseCancel);
            writer.Write(account.PublicKey);
            Utils.WriteToNetwork(writer, fee);
            Utils.WriteToNetwork(writer, timestamp);
            writer.Write(Base58.Decode(TransactionId));
            String signature = Sign(account, stream);
            return new Transaction("leasing/broadcast/cancel",
                    "senderPublicKey", Base58.Encode(account.PublicKey),
                    "signature", signature,
                    "TransactionId", TransactionId,
                    "fee", fee,
                    "timestamp", timestamp);
        }

        public static Transaction MakeAliasTransaction(PrivateKeyAccount account, String alias, char scheme, long fee)
        {
            long timestamp = Utils.CurrentTimestamp();
            int aliaslen = alias.Length;
            MemoryStream stream = new MemoryStream(MinBufferSize + aliaslen);
            BinaryWriter writer = new BinaryWriter(stream);            
            writer.Write(Transaction.Alias);
            writer.Write(account.PublicKey);
            Utils.WriteToNetwork(writer, (short)(alias.Length + 4));
            writer.Write(0x02);
            writer.Write((byte)scheme);
            Utils.WriteToNetwork(writer, (short)alias.Length);
            writer.Write(Encoding.ASCII.GetBytes(alias));
            Utils.WriteToNetwork(writer, fee);
            Utils.WriteToNetwork(writer, timestamp);            
            String signature = Sign(account, stream);
            return new Transaction("alias/broadcast/create",
                    "senderPublicKey", Base58.Encode(account.PublicKey),
                    "signature", signature,
                    "alias", alias,
                    "fee", fee,
                    "timestamp", timestamp);
        }

        public static Transaction MakeOrderTransaction(PrivateKeyAccount sender, String matcherKey, Order.Type orderType,
           String amountAssetId, String priceAssetId, long price, long amount, long expiration, long matcherFee)
        {
            long timestamp = Utils.CurrentTimestamp();
            int datalen = MinBufferSize +
                    (amountAssetId == null ? 0 : 32) +
                    (priceAssetId == null ? 0 : 32);
            if (datalen == MinBufferSize)
            {
                throw new ArgumentException("Both spendAsset and receiveAsset are WAVES");
            }
            MemoryStream stream = new MemoryStream(datalen);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(sender.PublicKey);
            writer.Write(Base58.Decode(matcherKey));
            PutAsset(stream, amountAssetId);
            PutAsset(stream, priceAssetId);
            writer.Write((byte)orderType.Ordinal); 
            Utils.WriteToNetwork(writer, price);
            Utils.WriteToNetwork(writer, amount);
            Utils.WriteToNetwork(writer, timestamp);
            Utils.WriteToNetwork(writer, expiration);
            Utils.WriteToNetwork(writer, matcherFee);
            String signature = Sign(sender, stream);

            return new Transaction("matcher/orderbook",
                    "senderPublicKey", Base58.Encode(sender.PublicKey),
                    "matcherPublicKey", matcherKey,
                    "assetPair", new AssetPair(amountAssetId, priceAssetId).GetDictionary(),
                    "orderType", orderType.json,
                    "price", price,
                    "amount", amount,
                    "timestamp", timestamp,
                    "expiration", expiration,
                    "matcherFee", matcherFee,
                    "signature", signature);
        }

        public class AssetPair
        {
            public AssetPair()
            {

            }

            public AssetPair(string amountAsset, string priceAsset)
            {
                AmountAsset = amountAsset;
                PriceAsset = priceAsset;
            }
            public string AmountAsset { get; set; }
            public string PriceAsset { get; set; }
            public Dictionary<string, string> GetDictionary()
            {
                Dictionary<String, String> assetPair = new Dictionary<String, String>
                {
                    ["amountAsset"] = AmountAsset,
                    ["priceAsset"] = PriceAsset
                };
                return assetPair;
            }
        }

        //private static Dictionary<String, String> AssetPair(String amountAssetId, String priceAssetId)
        //{
        //    Dictionary<String, String> assetPair = new Dictionary<String, String>
        //    {
        //        ["amountAsset"] = amountAssetId,
        //        ["priceAsset"] = priceAssetId
        //    };
        //    return assetPair;
        //}

        public static Transaction MakeOrderCancelTransaction(PrivateKeyAccount sender,
                String amountAssetId, String priceAssetId, String orderId, long fee)
        {
            MemoryStream stream = new MemoryStream(MinBufferSize);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(sender.PublicKey);
            writer.Write(Base58.Decode(orderId));
            String signature = Sign(sender, stream);
            amountAssetId = NormalizeAsset(amountAssetId);
            priceAssetId = NormalizeAsset(priceAssetId);
            return new Transaction(String.Format("matcher/orderbook/{0}/{1}/cancel", amountAssetId, priceAssetId),
                    "sender", Base58.Encode(sender.PublicKey), "orderId", orderId,
                    "signature", signature);
        }

        public class JsonTransaction
        {         
            public int Type { get; set; }
            public string Id { get; set; }
            public string Sender { get; set; }
            public string SenderPublicKey { get; set; }
            public long Fee { get; set; }
            public long Timestamp { get; set; }
            public string Signature { get; set; }
            public string Recipient { get; set; }
            public object AssetId { get; set; }
            public long Amount { get; set; }
            public object FeeAsset { get; set; }
            public string Attachment { get; set; }
            public AssetPair AssetPair { get; set; }
        }

        public class JsonTransactionError
        {
            public int Error { get; set; }
            public string Message { get; set; }
            public JsonTransaction Tx { get; set; }
        }

        public class JsonTransactionWithStatus
        {
            public string Status { get; set; }
            public JsonTransaction Message { get; set; }
        }
    }
}
