using System.Collections.Generic;
using System.IO;

namespace WavesCS
{
    public class CancelLeasingTransaction : Transaction
    {
        public string TransactionId { get; }
        public decimal Fee { get; }

        public CancelLeasingTransaction(byte[] senderPublicKey, string transactionId, decimal fee = 0.001m) : 
            base(senderPublicKey)
        {
            TransactionId = transactionId;
            Fee = fee;
        }

        public CancelLeasingTransaction(Dictionary<string, object> tx) : base (tx)
        {
            TransactionId = tx.GetString("leaseId");
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
                writer.Write(TransactionId.FromBase58());
                return stream.ToArray();
            }            
        }

        public override Dictionary<string, object> GetJson()
        {
            return new Dictionary<string, object>
            {
                {"type", TransactionType.LeaseCancel},
                {"senderPublicKey", SenderPublicKey.ToBase58()},
                {"leaseId", TransactionId},
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