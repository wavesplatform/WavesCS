using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WavesCS
{
    public class UnknownTransaction : Transaction
    {
        int type;

        public UnknownTransaction(byte[] senderPublicKey, DateTime timestamp, int type) : base(senderPublicKey, timestamp)
        {
            this.type = type;
        }

        public override byte[] GetBody()
        {                        
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(TransactionType.Transfer);
            writer.Write(SenderPublicKey);
            return stream.ToArray();
        }

        public override Dictionary<string, object> GetJson()
        {
            return new Dictionary<string, object>
            {
                {"type", type},
                {"senderPublicKey", SenderPublicKey.ToBase58()}
            };
        }

        protected override bool SupportsProofs()
        {
            return false;
        }
    }
}