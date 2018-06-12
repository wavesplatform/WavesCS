using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WavesCS
{
    public class MassTransferItem
    {
        public string Recipient { get; }
        public decimal Amount { get;  }
        
        public MassTransferItem(string recipient, decimal amount)
        {
            Recipient = recipient;
            Amount = amount;
        }        
    }
    
    public class MassTransferTransaction : Transaction
    {
        public Asset Asset { get; }
        public byte[] Attachment { get; }
        public decimal Fee { get; }
        public MassTransferItem[] Transfers { get; }
        
        private const byte Version = 1;

        public MassTransferTransaction(byte[] senderPublicKey, Asset asset, IEnumerable<MassTransferItem> transfers,
            string attachment, decimal? fee = null) : 
            this(senderPublicKey, asset, transfers, Encoding.UTF8.GetBytes(attachment), fee) 
        {            
        }
        
        public MassTransferTransaction(byte[] senderPublicKey, Asset asset, IEnumerable<MassTransferItem> transfers,
            byte[] attachment = null, decimal? fee = null) : base(senderPublicKey)
        {
            Asset = asset;
            Attachment = attachment ?? new byte[0];
            Transfers = transfers.ToArray();
            Fee = fee ?? Math.Round(0.001m + Transfers.Length * 0.0005m, 3, MidpointRounding.AwayFromZero);
        }

        public override byte[] GetBody()
        {                                    
            using(var stream = new MemoryStream())
            using(var writer = new BinaryWriter(stream))
            {
                writer.Write(TransactionType.MassTransfer);
                writer.Write(Version);
                writer.Write(SenderPublicKey);
                writer.WriteAsset(Asset.Id);
                writer.WriteShort(Transfers.Length);
                foreach (var transfer in Transfers)
                {
                    writer.Write(transfer.Recipient.FromBase58());
                    writer.WriteLong(Asset.AmountToLong(transfer.Amount));
                }
                writer.WriteLong(Timestamp.ToLong());
                writer.WriteLong(Assets.WAVES.AmountToLong(Fee));
                writer.WriteShort((short) Attachment.Length);
                writer.Write(Attachment);
                return stream.ToArray();
            }
        }

        public override Dictionary<string, object> GetJson()
        {
            return new Dictionary<string, object>
            {
                { "type", TransactionType.MassTransfer},
                { "version", Version},
                { "senderPublicKey", Base58.Encode(SenderPublicKey)},                
                { "transfers", Transfers.Select(t => new Dictionary<string, object>()
                {
                    {"recipient", t.Recipient },
                    {"amount", Asset.AmountToLong(t.Amount)}
                }).ToArray() },
                { "assetId", Asset.IdOrNull},
                { "fee", Assets.WAVES.AmountToLong(Fee)},
                { "timestamp", Timestamp.ToLong() },
                { "attachment", Attachment.ToBase58() }
            };
        }

        protected override bool SupportsProofs()
        {
            return true;
        }  
    }
}