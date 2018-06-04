using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{    

    public enum TransactionType : byte
    {
        Issue = 3,
        Transfer = 4,
        Reissue = 5,
        Burn = 6,
        Lease = 8,
        LeaseCancel = 9,
        Alias = 10,
        MassTransfer = 11,
        DataTx = 12,    
    }
    
    
    
    public static class Transactions
    {
        private static readonly int MinBufferSize = 300;

        public static DictionaryObject MakeIssueTransaction(PrivateKeyAccount account,
            string name, string description, long quantity, int decimals, bool reissuable, long fee)
        {
            long timestamp = Utils.CurrentTimestamp();

            var stream = new MemoryStream(MinBufferSize);
            var writer = new BinaryWriter(stream);
            writer.Write(TransactionType.Issue);
            writer.Write(account.PublicKey);
            writer.WriteShort(name.Length);
            writer.Write(Encoding.ASCII.GetBytes(name));

            int descriptionLegth = description?.Length ?? 0;
            writer.WriteShort((short) descriptionLegth);
            if (descriptionLegth > 0)
            {
                writer.Write(Encoding.ASCII.GetBytes(description));
            }

            writer.WriteLong(quantity);
            writer.Write((byte) decimals);
            writer.Write((byte) (reissuable ? 1 : 0));
            writer.WriteLong(fee);
            writer.WriteLong(timestamp);

            string signature = account.Sign(stream);
            return new DictionaryObject
            {
                {"type", TransactionType.Issue},
                {"senderPublicKey", Base58.Encode(account.PublicKey)},
                {"signature", signature},
                {"name", name},
                {"description", description},
                {"quantity", quantity},
                {"decimals", decimals},
                {"reissuable", reissuable},
                {"fee", fee},
                {"timestamp", timestamp}
            };
        }

        public static DictionaryObject MakeReissueTransaction(PrivateKeyAccount account, string assetId, long quantity, bool reissuable, long fee)
        {
            long timestamp = Utils.CurrentTimestamp();
            var stream = new MemoryStream(MinBufferSize);
            var writer = new BinaryWriter(stream);
            writer.Write(TransactionType.Reissue);
            writer.Write(account.PublicKey);
            writer.Write(Base58.Decode(assetId));
            writer.WriteLong(quantity);
            writer.Write((byte)(reissuable ? 1 : 0));
            writer.WriteLong(fee);
            writer.WriteLong(timestamp);

            string signature = account.Sign(stream);
            return new DictionaryObject {
                { "type", TransactionType.Reissue },
                { "senderPublicKey", Base58.Encode(account.PublicKey) },
                { "signature", signature },
                { "assetId", assetId },
                { "quantity", quantity },
                { "reissuable", reissuable },
                { "fee", fee },
                { "timestamp", timestamp }
            };
        }

        public static DictionaryObject MakeTransferTransaction(PrivateKeyAccount account, string toAddress,
            long amount, string assetId, long fee, string feeAssetId, string attachment)
        {

            byte[] attachmentBytes = Encoding.UTF8.GetBytes(attachment ?? "");
            long timestamp = Utils.CurrentTimestamp();

            var stream = new MemoryStream(MinBufferSize);
            var writer = new BinaryWriter(stream);
            writer.Write(TransactionType.Transfer);
            writer.Write(account.PublicKey);
            writer.WriteAsset(assetId);
            writer.WriteAsset(feeAssetId);
            writer.WriteLong(timestamp);
            writer.WriteLong(amount);
            writer.WriteLong(fee);
            writer.Write(Base58.Decode(toAddress));
            //writer.Write((short)attachmentBytes.Length);
            writer.WriteShort((short) attachmentBytes.Length);
            writer.Write(attachmentBytes);
            string signature = account.Sign(stream);
            return new DictionaryObject
            {
                {"type", TransactionType.Transfer},
                {"senderPublicKey", Base58.Encode(account.PublicKey)},
                {"signature", signature},
                {"recipient", toAddress},
                {"amount", amount},
                {"assetId", assetId},
                {"fee", fee},
                {"feeAssetId", feeAssetId},
                {"timestamp", timestamp},
                {"attachment", Base58.Encode(attachmentBytes)}
            };
        }

        public static DictionaryObject MakeMassTransferTransaction(PrivateKeyAccount account, List<MassTransferRecipient> transfers,
           String assetId, long fee, String feeAssetId, String attachment)
        {
            byte[] attachmentBytes = Encoding.UTF8.GetBytes(attachment ?? "");
            long timestamp = Utils.CurrentTimestamp();
            const byte version = 1;

            MemoryStream stream = new MemoryStream(MinBufferSize);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(TransactionType.MassTransfer);
            writer.Write(version);
            writer.Write(account.PublicKey);
            writer.WriteAsset(assetId);
            writer.WriteShort(transfers.Count);
            foreach (var recipient in transfers)
            {
                writer.Write(Base58.Decode(recipient.recipient));
                writer.WriteLong(recipient.amount);
            }
            writer.WriteLong(timestamp);
            writer.WriteLong(fee);
            writer.WriteShort((short)attachmentBytes.Length);
            writer.Write(attachmentBytes);
            string signature = account.Sign(stream);

            return new DictionaryObject
            {
                { "type", TransactionType.MassTransfer},
                { "version", version},
                { "senderPublicKey", Base58.Encode(account.PublicKey)},
                { "proofs", new[] { signature }},
                { "transfers", transfers.ToArray()},
                { "assetId", assetId},
                { "fee", fee},
                { "timestamp", timestamp},
                { "attachment", Base58.Encode(attachmentBytes)}
            };
        }

        public class MassTransferRecipient
        {
            public MassTransferRecipient(string recipient, long amount)
            {
                this.recipient = recipient;
                this.amount = amount;
            }
            public string recipient { get; set; }
            public long amount { get; set; }
        }

        public static DictionaryObject MakeBurnTransaction(PrivateKeyAccount account, String assetId, long amount, long fee)
        {
            
            long timestamp = Utils.CurrentTimestamp();
            var stream = new MemoryStream(MinBufferSize);
            var writer = new BinaryWriter(stream);
            writer.Write(TransactionType.Burn);
            writer.Write(account.PublicKey);
            writer.Write(Base58.Decode(assetId));
            writer.WriteLong(amount);
            writer.WriteLong(fee);
            writer.WriteLong(timestamp);

            string signature = account.Sign(stream);
            return new Dictionary<string, object>
            {
                {"type", TransactionType.Burn},
                {"senderPublicKey", Base58.Encode(account.PublicKey)},
                {"signature", signature},
                {"assetId", assetId},
                {"quantity", amount},
                {"fee", fee},
                {"timestamp", timestamp}
            };
        }

        public static DictionaryObject MakeLeaseTransaction(PrivateKeyAccount account, String toAddress, long amount, long fee)
        {
            long timestamp = Utils.CurrentTimestamp();
            var stream = new MemoryStream(MinBufferSize);
            var writer = new BinaryWriter(stream);
            writer.Write(TransactionType.Lease);
            writer.Write(account.PublicKey);
            writer.Write(Base58.Decode(toAddress));

            writer.WriteLong(amount);
            writer.WriteLong(fee);
            writer.WriteLong(timestamp);

            string signature = account.Sign(stream);
            return new DictionaryObject {
                {"type", TransactionType.Lease },
                {"senderPublicKey", Base58.Encode(account.PublicKey)},
                {"signature", signature},
                {"recipient", toAddress},
                {"amount", amount},
                {"fee", fee},
                {"timestamp", timestamp}                
            };
        }

        public static DictionaryObject MakeLeaseCancelTransaction(PrivateKeyAccount account, String TransactionId, long fee)
        {
            long timestamp = Utils.CurrentTimestamp();
            var stream = new MemoryStream(MinBufferSize);
            var writer = new BinaryWriter(stream);
            writer.Write(TransactionType.LeaseCancel);
            writer.Write(account.PublicKey);
            writer.WriteLong(fee);
            writer.WriteLong(timestamp);
            writer.Write(Base58.Decode(TransactionId));
            string signature = account.Sign(stream);
            return new DictionaryObject
            {
                {"type", TransactionType.LeaseCancel},
                {"senderPublicKey", Base58.Encode(account.PublicKey)},
                {"signature", signature},
                {"TransactionId", TransactionId},
                {"fee", fee},
                {"timestamp", timestamp}
            };
        }

        public static DictionaryObject MakeAliasTransaction(PrivateKeyAccount account, String alias, char scheme, long fee)
        {
            long timestamp = Utils.CurrentTimestamp();
            var stream = new MemoryStream(MinBufferSize);
            var writer = new BinaryWriter(stream);
            writer.Write(TransactionType.Alias);
            writer.Write(account.PublicKey);
            writer.WriteShort((short) (alias.Length + 4));
            writer.Write(0x02);
            writer.Write((byte) scheme);
            writer.WriteShort((short) alias.Length);
            writer.Write(Encoding.ASCII.GetBytes(alias));

            writer.WriteLong(fee);
            writer.WriteLong(timestamp);

            string signature = account.Sign(stream);
            return new DictionaryObject
            {
                {"type", TransactionType.Alias},
                {"senderPublicKey", Base58.Encode(account.PublicKey)},
                {"signature", signature},
                {"alias", alias},
                {"fee", fee},
                {"timestamp", timestamp}
            };
        }

        public static DictionaryObject MakeDataTransaction(PrivateKeyAccount account, Dictionary<string, object> entries,
            long fee)
        {
            long timestamp = Utils.CurrentTimestamp();

            var stream = new MemoryStream(MinBufferSize);
            var writer = new BinaryWriter(stream);

            const byte INTEGER = 0;
            const byte BOOLEAN = 1;
            const byte BINARY = 2;
            const byte STRING = 3;
            const byte version = 1;

            writer.Write(TransactionType.DataTx);
            writer.Write(version);
            writer.Write(account.PublicKey);
            writer.WriteShort((short) entries.Count);
            foreach (var pair in entries)
            {
                var key = Encoding.UTF8.GetBytes(pair.Key);
                writer.WriteShort((short) key.Length);
                writer.Write(key);
                switch (pair.Value)
                {
                    case long value:                        
                        writer.Write(INTEGER);
                        writer.WriteLong(value);
                        break;
                    case bool value:
                        writer.Write(BOOLEAN);
                        writer.Write(value ? (byte) 1 : (byte) 0);
                        break;
                    case byte[] value:
                        writer.Write(BINARY);
                        writer.WriteShort((short) value.Length);
                        writer.Write(value);
                        break;
                    case string value:
                        writer.Write(STRING);
                        var encoded = Encoding.UTF8.GetBytes(value);
                        writer.WriteShort((short) encoded.Length);
                        writer.Write(encoded);                        
                        break;
                    default:
                        throw new ArgumentException("Only long, bool and byte[] entry values supported",
                            nameof(entries));
                }
            }

            writer.WriteLong(timestamp);
            writer.WriteLong(fee);
            string signature = account.Sign(stream);
            return new DictionaryObject
            {
                {"type", TransactionType.DataTx},
                {"version", version},
                {"senderPublicKey", account.PublicKey.ToBase58() },
                {"data", entries.Select(pair => new DictionaryObject
                {
                    {"key", pair.Key},
                    {"type", pair.Value is long ? "integer" : (pair.Value is bool ? "boolean" : (pair.Value is string ? "string"  : "binary"))},
                    {"value", pair.Value is byte[] bytes ? "base64:" + bytes.ToBase64() : pair.Value }                    
                })},
                {"fee", fee},
                {"timestamp", timestamp},
                {"proofs", new []{ signature }}
            };
        }

    }
}
