using System;
using System.IO;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public class CancelLeasingTransaction : Transaction
    {
        public string LeaseId { get; }
        public override byte Version { get; set; } = 2;

        public CancelLeasingTransaction(char chainId, byte[] senderPublicKey, string leaseId, decimal fee = 0.001m) : 
            base(chainId, senderPublicKey)
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

                if (Version > 1)
                {
                    writer.WriteByte(Version);
                    writer.WriteByte((byte)ChainId);
                }

                writer.Write(SenderPublicKey);
                writer.WriteLong(Assets.WAVES.AmountToLong(Fee));
                writer.WriteLong(Timestamp.ToLong());
                writer.Write(LeaseId.FromBase58());
                return stream.ToArray();
            }            
        }

        public override byte[] GetBytes()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            if (Version == 1)
            {
                writer.Write(GetBody());
                writer.Write(Proofs[0]);
            }
            else
            {
                writer.WriteByte(0);
                writer.Write(GetBody());
                writer.Write(GetProofsBytes());
            }

            return stream.ToArray();
        }
        public override DictionaryObject GetJson()
        {
            var result = new DictionaryObject
            {
                {"type", (byte) TransactionType.LeaseCancel},
                {"senderPublicKey", SenderPublicKey.ToBase58()},
                {"leaseId", LeaseId},
                {"fee", Assets.WAVES.AmountToLong(Fee)},
                {"timestamp", Timestamp.ToLong()}
            };

            if (Sender != null)
                result.Add("sender", Sender);

            return result;
        }

        protected override bool SupportsProofs()
        {
            return false;
        }
    }
}