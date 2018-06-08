using System.Collections.Generic;
using System.IO;

namespace WavesCS
{
    public class LeaseTransaction : Transaction
    {
        public string Recipient { get; }
        public decimal Amount { get; }
        public decimal Fee { get; }

        public LeaseTransaction(byte[] senderPublicKey, string recipient, decimal amount, decimal fee = 0.001m) : 
            base(senderPublicKey)
        {
            Recipient = recipient;
            Amount = amount;
            Fee = fee;
        }

        public override byte[] GetBody()
        {
            
            using(var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(TransactionType.Lease);
                writer.Write(SenderPublicKey);
                writer.Write(Recipient.FromBase58());
                writer.WriteLong(Assets.WAVES.AmountToLong(Amount));
                writer.WriteLong(Assets.WAVES.AmountToLong(Fee));
                writer.WriteLong(Timestamp.ToLong());
                return stream.ToArray();
            }
        }

        public override Dictionary<string, object> GetJson()
        {
            return new Dictionary<string, object> {
                {"type", TransactionType.Lease },
                {"senderPublicKey", SenderPublicKey.ToBase58()},                
                {"recipient", Recipient},
                {"amount", Assets.WAVES.AmountToLong(Amount)},
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