using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
            FeeAsset = feeAsset;
            Attachment = attachment ?? new byte[0];
        }

        public TransferTransaction(Dictionary<string, object> tx): base(tx)
        {
            Asset = Assets.WAVES;
            if (tx.ContainsKey("assetId") && tx.GetString("assetId") != null)
                Asset = Assets.GetById(tx.GetString("assetId"));

            FeeAsset = Assets.WAVES;
            if (tx.ContainsKey("feeAsset") && tx.GetString("feeAsset") != null)
                FeeAsset = Assets.GetById(tx.GetString("feeAssetId"));
            

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
                var networkByte = Recipient[6];
                var name = Recipient.Substring(8);

                writer.Write((byte)2);
                writer.Write(networkByte);

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


        public override Dictionary<string, object> GetJson()
        {
            var result = new Dictionary<string, object>
            {
                {"type", (byte) TransactionType.Transfer},
                {"sender", Sender},
                {"senderPublicKey", SenderPublicKey.ToBase58()},
                {"recipient", Recipient},
                {"amount", Asset.AmountToLong(Amount)},
                {"assetId", Asset.IdOrNull},
                {"fee", FeeAsset.AmountToLong(Fee)},
                {"feeAssetId", FeeAsset.IdOrNull},
                {"timestamp", Timestamp.ToLong()},
                {"attachment", Attachment.ToBase58()}
            };
            if (Version > 1)
                result.Add("version", Version);
            return result;
        }

        protected override bool SupportsProofs()
        {
            return Version > 1;
        }
    }
}