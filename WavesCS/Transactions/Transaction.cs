using System;
using System.Linq;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public abstract class Transaction
    {
        public DateTime Timestamp { get; }        
        
        public byte[] SenderPublicKey { get; }

        public abstract byte[] GetBody();
        public abstract DictionaryObject GetJson();

        private byte[][] Proofs { get; set; }        

        protected Transaction(byte[] senderPublicKey)
        {
            Timestamp = DateTime.UtcNow;                        
            SenderPublicKey = senderPublicKey;
            Proofs = new byte[0][];
        }       
        
        protected abstract bool SupportsProofs();

        public void Sign(PrivateKeyAccount account)
        {
            Proofs = new[] { account.Sign(GetBody()) };
        }

        public DictionaryObject GetJsonWithSignature()
        {
            var json = GetJson();
            if (SupportsProofs())
            {
                json.Add("proofs", Proofs.Select(p => p.ToBase58()));
            }
            else
            {
                if (Proofs.Length < 1)
                    throw new InvalidOperationException("Transaction is not signed");
                json.Add("signature", Proofs[0].ToBase58());
            }
            return json;
        }
       
    }
}