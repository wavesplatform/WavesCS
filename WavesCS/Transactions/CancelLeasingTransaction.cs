using System.IO;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public class CancelLeasingTransaction : Transaction
    {
        public string LeaseId { get; }
      
        public CancelLeasingTransaction(byte[] senderPublicKey, string leaseId, decimal fee = 0.001m) : 
            base(senderPublicKey)
        {
            LeaseId = leaseId;
            Fee = fee;
        }

        public CancelLeasingTransaction(DictionaryObject tx) : base (tx)
        {
            LeaseId = tx.GetString("leaseId");
            Fee = Assets.WAVES.LongToAmount(tx.GetLong("fee"));
        }

        public override byte[] GetBody()
        {
            using(var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(TransactionType.LeaseCancel);
                writer.Write(SenderPublicKey);
                writer.WriteLong(Assets.WAVES.AmountToLong(Fee));
                writer.WriteLong(Timestamp.ToLong());
                writer.Write(LeaseId.FromBase58());
                return stream.ToArray();
            }            
        }

        public override byte[] GetIdBytes()
        {
            return GetBody();
        }

        public override DictionaryObject GetJson()
        {
            return new DictionaryObject
            {
                {"type", (byte) TransactionType.LeaseCancel},
                {"senderPublicKey", SenderPublicKey.ToBase58()},
                {"sender", Sender},
                {"leaseId", LeaseId},
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