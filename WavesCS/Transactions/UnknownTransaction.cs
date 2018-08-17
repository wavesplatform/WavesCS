using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public class UnknownTransaction : Transaction
    {
        int Type;

        public UnknownTransaction(byte[] senderPublicKey, int type) : base(senderPublicKey)
        {
            Type = type;
        }

        public UnknownTransaction(Dictionary<string, object> tx) : base(tx)
        {
            Type = tx.GetInt("type");
        }

        public override byte[] GetBody()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(TransactionType.Unknown);
                writer.Write(SenderPublicKey);
                writer.WriteLong(Timestamp.ToLong());
                return stream.ToArray();
            }
        }

        public override Dictionary<string, object> GetJson()
        {
            return new Dictionary<string, object>
            {
                {"type", TransactionType.Unknown},
                {"senderPublicKey", SenderPublicKey.ToBase58() },
                {"timestamp", Timestamp.ToLong()},
            };
        }

        protected override bool SupportsProofs()
        {
            return false;
        }
    }
}