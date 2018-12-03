using System;
using System.IO;
using System.Text;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public class TransferTransaction : Transaction
    {
        public string Recipient { get; }
        public decimal Amount { get; }
        public Asset Asset { get; }
        public Asset FeeAsset { get; }
        public byte[] Attachment { get; }

        public override byte Version { get; set; } = 1;

        public TransferTransaction(byte[] senderPublicKey, string recipient,
           Asset asset, decimal amount, string attachment) :
        this(senderPublicKey, recipient, asset, amount, 0.001m,
             Encoding.UTF8.GetBytes(attachment))
        {
        }
        
        public TransferTransaction(byte[] senderPublicKey, string recipient,
            Asset asset, decimal amount, decimal fee = 0.001m, byte[] attachment = null) : 
            this(senderPublicKey, recipient, asset, amount, fee, Assets.WAVES, attachment)
        {                  
        }
        
        public TransferTransaction(byte[] senderPublicKey, string recipient,
            Asset asset, decimal amount, decimal fee, Asset feeAsset, byte[] attachment = null) : base(senderPublicKey)
        {
            Recipient = recipient;
            Amount = amount;
            Asset = asset ?? Assets.WAVES;
            Fee = fee;
            FeeAsset = feeAsset ?? Assets.WAVES;
            Attachment = attachment ?? new byte[0];
        }

        public TransferTransaction(DictionaryObject tx): base(tx)
        {
            Asset = Assets.WAVES;
            if (tx.ContainsKey("assetId") && tx.GetString("assetId") != null)
                Asset = Assets.GetById(tx.GetString("assetId"));

            FeeAsset = Assets.WAVES;
            if (tx.ContainsKey("feeAssetId")
                && tx.GetString("feeAssetId") != null
                && tx.GetString("feeAssetId") != "")
            {
                FeeAsset = Assets.GetById(tx.GetString("feeAssetId"));
            }

            Amount = Asset.LongToAmount(tx.GetLong("amount"));
            Fee = FeeAsset.LongToAmount(tx.GetLong("fee"));

            Recipient = tx.GetString("recipient");

            Attachment = tx.ContainsKey("attachment")
                           ? tx.GetString("attachment").FromBase58()
                           : new byte[0];
        }

        public void WriteBytes(BinaryWriter writer)
        {
            writer.Write(SenderPublicKey);
            writer.WriteAsset(Asset.Id);
            writer.WriteAsset(FeeAsset.Id);
            writer.WriteLong(Timestamp.ToLong());
            writer.WriteLong(Asset.AmountToLong(Amount));
            writer.WriteLong(FeeAsset.AmountToLong(Fee));

            if (Recipient.StartsWith("alias", StringComparison.Ordinal))
            {
                var chainId = Recipient[6];
                var name = Recipient.Substring(8);

                writer.Write((byte)2);
                writer.Write(chainId);

                writer.WriteShort(name.Length);
                writer.Write(Encoding.UTF8.GetBytes(name));
            }
            else
                writer.Write(Recipient.FromBase58());
            writer.WriteShort(Attachment.Length);
            writer.Write(Attachment);
        }

        public override byte[] GetBody()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write(TransactionType.Transfer);

            if (Version > 1)
                writer.Write(Version);

            WriteBytes(writer);
            return stream.ToArray();
        }

        public override byte[] GetIdBytes()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write(TransactionType.Transfer);
            WriteBytes(writer);
            return stream.ToArray();
        }


        public override DictionaryObject GetJson()
        {
            var result = new DictionaryObject
            {
                {"type", (byte) TransactionType.Transfer},
                {"senderPublicKey", SenderPublicKey.ToBase58()},
                {"recipient", Recipient},
                {"amount", Asset.AmountToLong(Amount)},
                {"assetId", Asset.IdOrNull},
                {"fee", FeeAsset.AmountToLong(Fee)},
                {"feeAsset", FeeAsset.IdOrNull},  // legacy v0.11.1 compat
                {"feeAssetId", FeeAsset.IdOrNull},
                {"timestamp", Timestamp.ToLong()},
                {"attachment", Attachment.ToBase58()}
            };

            if (Version > 1)
                result.Add("version", Version);

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