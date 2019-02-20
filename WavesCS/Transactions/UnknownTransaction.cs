using System;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public class UnknownTransaction : Transaction
    {
        int Type;

        public UnknownTransaction(char chainId, byte[] senderPublicKey, int type) : base(chainId, senderPublicKey)
        {
            Type = type;
        }

        public UnknownTransaction(DictionaryObject tx) : base(tx)
        {
            Type = tx.GetByte("type");
        }

        public override byte[] GetBody()
        {
            throw new Exception("Unknown transaction");
        }

        public override byte[] GetIdBytes()
        {
            throw new Exception("Unknown transaction");
        }

        public override DictionaryObject GetJson()
        {
            throw new Exception("Unknown transaction");
        }

        protected override bool SupportsProofs()
        {
            return false;
        }
    }
}