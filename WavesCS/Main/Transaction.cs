using System;
using System.Collections.Generic;
using System.Linq;
using org.whispersystems.curve25519;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace WavesCS.Main
{
    public class Transaction
    {
        private static readonly byte ISSUE = 3;
        private static readonly byte TRANSFER = 4;
        private static readonly byte REISSUE = 5;
        private static readonly byte BURN = 6;
        private static readonly byte LEASE = 8;
        private static readonly byte LEASE_CANCEL = 9;
        private static readonly byte ALIAS = 10;

        private static readonly int MIN_BUFFER_SIZE = 120;
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

        public String GetJson()
        {
            try
            {
                var serializer = new JavaScriptSerializer();
                serializer.RegisterConverters(new JavaScriptConverter[] { new KeyValuePairJsonConverter() });
                var serializedResult = serializer.Serialize(data);
                return serializedResult;
            }
            catch (SystemException)
            {
                // not expected to ever happen
                return null;
            }
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
            long epochTicks = new DateTime(1970, 1, 1).Ticks;
            long timestamp = ((DateTime.UtcNow.Ticks - epochTicks) / TimeSpan.TicksPerSecond) * 1000;
            int desclen = description == null ? 0 : description.Length;
            MemoryStream stream = new MemoryStream(MIN_BUFFER_SIZE + name.Length + desclen);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(ISSUE);
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
            long epochTicks = new DateTime(1970, 1, 1).Ticks;
            long timestamp = ((DateTime.UtcNow.Ticks - epochTicks) / TimeSpan.TicksPerSecond) * 1000;
            MemoryStream stream = new MemoryStream(MIN_BUFFER_SIZE);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(REISSUE);
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
            byte[] attachmentBytes = Encoding.Default.GetBytes(attachment == null ? "" : attachment);
            int datalen = (assetId == null ? 0 : 32) +
                    (feeAssetId == null ? 0 : 32) +
                    attachmentBytes.Length + MIN_BUFFER_SIZE;
            long epochTicks = new DateTime(1970, 1, 1).Ticks;
            long timestamp = ((DateTime.UtcNow.Ticks - epochTicks) / TimeSpan.TicksPerSecond) * 1000;

            MemoryStream stream = new MemoryStream(datalen);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(TRANSFER);
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
            long epochTicks = new DateTime(1970, 1, 1).Ticks;
            long timestamp = ((DateTime.UtcNow.Ticks - epochTicks) / TimeSpan.TicksPerSecond) * 1000;
            MemoryStream stream = new MemoryStream(MIN_BUFFER_SIZE);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(BURN);
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
            long epochTicks = new DateTime(1970, 1, 1).Ticks;
            long timestamp = ((DateTime.UtcNow.Ticks - epochTicks) / TimeSpan.TicksPerSecond) * 1000;
            MemoryStream stream = new MemoryStream(MIN_BUFFER_SIZE);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(LEASE);
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
            long epochTicks = new DateTime(1970, 1, 1).Ticks;
            long timestamp = ((DateTime.UtcNow.Ticks - epochTicks) / TimeSpan.TicksPerSecond) * 1000;
            MemoryStream stream = new MemoryStream(MIN_BUFFER_SIZE);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(LEASE_CANCEL);
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
            long epochTicks = new DateTime(1970, 1, 1).Ticks;
            long timestamp = ((DateTime.UtcNow.Ticks - epochTicks) / TimeSpan.TicksPerSecond) * 1000;
            int aliaslen = alias.Length;
            MemoryStream stream = new MemoryStream(MIN_BUFFER_SIZE + aliaslen);
            BinaryWriter writer = new BinaryWriter(stream);            
            writer.Write(ALIAS);
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
            long epochTicks = new DateTime(1970, 1, 1).Ticks;
            long timestamp = ((DateTime.UtcNow.Ticks - epochTicks) / TimeSpan.TicksPerSecond) * 1000;
            int datalen = MIN_BUFFER_SIZE +
                    (amountAssetId == null ? 0 : 32) +
                    (priceAssetId == null ? 0 : 32);
            if (datalen == MIN_BUFFER_SIZE)
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
                    "assetPair", AssetPair(amountAssetId, priceAssetId),
                    "orderType", orderType.json,
                    "price", price,
                    "amount", amount,
                    "timestamp", timestamp,
                    "expiration", expiration,
                    "matcherFee", matcherFee,
                    "signature", signature);
        }

        private static Dictionary<String, String> AssetPair(String amountAssetId, String priceAssetId)
        {
            Dictionary<String, String> assetPair = new Dictionary<String, String>
            {
                ["amountAsset"] = amountAssetId,
                ["priceAsset"] = priceAssetId
            };
            return assetPair;
        }

        public static Transaction MakeOrderCancelTransaction(PrivateKeyAccount sender,
                String amountAssetId, String priceAssetId, String orderId, long fee)
        {
            MemoryStream stream = new MemoryStream(MIN_BUFFER_SIZE);
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
    }
}
