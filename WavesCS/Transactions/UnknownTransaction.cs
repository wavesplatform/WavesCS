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
            throw new Exception("Unknown transaction");
        }

        public override Dictionary<string, object> GetJson()
        {
            throw new Exception("Unknown transaction");
        }

        protected override bool SupportsProofs()
        {
            return false;
        }
    }
}