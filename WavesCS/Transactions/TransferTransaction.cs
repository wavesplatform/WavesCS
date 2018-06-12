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
        public decimal Fee { get; }
        public Asset FeeAsset { get; }
        public byte[] Attachment { get; }

        public TransferTransaction(byte[] senderPublicKey, string recipient,
            Asset asset, decimal amount, string attachment) : 
            this(senderPublicKey, recipient, asset, amount, Encoding.UTF8.GetBytes(attachment))
        {                  
        }
        
        public TransferTransaction(byte[] senderPublicKey, string recipient,
            Asset asset, decimal amount, byte[] attachment = null) : 
            this(senderPublicKey, recipient, asset, amount, 0.001m, Assets.WAVES, attachment)
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

        public override byte[] GetBody()
        {                        
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(TransactionType.Transfer);
            writer.Write(SenderPublicKey);
            writer.WriteAsset(Asset.Id);
            writer.WriteAsset(FeeAsset.Id);
            writer.WriteLong(Timestamp.ToLong());
            writer.WriteLong(Asset.AmountToLong(Amount));
            writer.WriteLong(FeeAsset.AmountToLong(Fee));
            writer.Write(Recipient.FromBase58());            
            writer.WriteShort(Attachment.Length);
            writer.Write(Attachment);
            return stream.ToArray();
        }

        public override Dictionary<string, object> GetJson()
        {
            return new Dictionary<string, object>
            {
                {"type", TransactionType.Transfer},
                {"senderPublicKey", SenderPublicKey.ToBase58()},
                {"recipient", Recipient},
                {"amount", Asset.AmountToLong(Amount)},
                {"assetId", Asset.IdOrNull},
                {"fee", FeeAsset.AmountToLong(Fee)},
                {"feeAssetId", FeeAsset.IdOrNull},
                {"timestamp", Timestamp.ToLong()},
                {"attachment", Attachment.ToBase58()}
            };
        }

        protected override bool SupportsProofs()
        {
            return false;
        }
    }
}