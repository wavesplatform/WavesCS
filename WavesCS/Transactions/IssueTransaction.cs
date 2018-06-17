using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WavesCS
{
    public class IssueTransaction : Transaction
    {
        public string Name { get; }
        public string Description { get; }
        public decimal Quantity { get; }
        public byte Decimals { get; }
        public bool Reissuable { get; }
        public decimal Fee { get; }
       
        public IssueTransaction(byte[] senderPublicKey, DateTime timestamp, 
            string name, string description, decimal quantity, byte decimals, bool reissuable, decimal fee = 1m) : base(senderPublicKey, timestamp)
        {
            Name = name ?? "";
            Description = description ?? "";
            Quantity = quantity;
            Decimals = decimals;
            Reissuable = reissuable;
            Fee = fee;
        }

        public override byte[] GetBody()
        {
            var asset = new Asset("", "", Decimals);             
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(TransactionType.Issue);
            writer.Write(SenderPublicKey);
            writer.WriteShort(Name.Length);
            writer.Write(Encoding.ASCII.GetBytes(Name));
            writer.WriteShort(Description.Length);
            writer.Write(Encoding.ASCII.GetBytes(Description));            
            writer.WriteLong(asset.AmountToLong(Quantity));
            writer.Write(Decimals);
            writer.Write((byte) (Reissuable ? 1 : 0));
            writer.WriteLong(Assets.WAVES.AmountToLong(Fee));
            writer.WriteLong(Timestamp.ToLong());
            return stream.ToArray();
        }

        public override Dictionary<string, object> GetJson()
        {
            return new Dictionary<string, object>
            {
                {"type", TransactionType.Issue},
                {"senderPublicKey", Base58.Encode(SenderPublicKey)},                
                {"name", Name},
                {"description", Description},
                {"quantity", Quantity},
                {"decimals", Decimals},
                {"reissuable", Reissuable},
                {"fee", Assets.WAVES.AmountToLong(Fee)},
                {"timestamp", Timestamp.ToLong()}
            };
        }

        protected override bool SupportsProofs()
        {
            return false;
        }        
    }
}