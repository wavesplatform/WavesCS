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

        public static byte Version = 1;
        
        public TransferTransaction(byte[] senderPublicKey, string recipient,
            Asset asset, decimal amount, string attachment) : 
            this(senderPublicKey, recipient, asset, amount, 0.001m, Encoding.UTF8.GetBytes(attachment))
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

        public override byte[] GetBody()
        {                        
            using(var stream = new MemoryStream())
            using(var writer = new BinaryWriter(stream))
            {
                writer.Write(TransactionType.Transfer);
                if (Version == 2)
                    writer.Write(Version);
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
        }

        public override Dictionary<string, object> GetJson()
        {
            var result = new Dictionary<string, object>
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
            if (Version > 1)
                result.Add("version", Version);
            return result;
        }

        protected override bool SupportsProofs()
        {
            return Version == 2;
        }
    }
}