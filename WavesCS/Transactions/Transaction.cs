using System;
using System.Linq;
using System.Text;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public abstract class Transaction
    {
        public DateTime Timestamp { get; }        
        
        public byte[] SenderPublicKey { get; }

        public abstract byte[] GetBody();
        public abstract DictionaryObject GetJson();

        public byte[][] Proofs { get; }        

        protected Transaction(byte[] senderPublicKey)
        {
            Timestamp = DateTime.UtcNow;                        
            SenderPublicKey = senderPublicKey;            
            Proofs = new byte[8][];
        }

        protected Transaction(DictionaryObject tx)
        {
            Timestamp = tx.GetDate("timestamp");

            SenderPublicKey = tx.GetString("senderPublicKey").FromBase58();

            if (tx.ContainsKey("proofs"))
            {
                Proofs = tx.Get<string[]>("proofs")
                           .Select(item => item.FromBase58())
                           .ToArray();
            }
            else
            {
                Proofs = new byte[8][];
                if (tx.ContainsKey("signature"))
                    Proofs[0] = tx.GetString("signature").FromBase58();
            }
        }
        
        protected abstract bool SupportsProofs();

        public DictionaryObject GetJsonWithSignature()
        {
            var json = GetJson();
            var proofs = Proofs
                .Take(Array.FindLastIndex(Proofs, p => p != null && p.Length > 0) + 1)
                .Select(p => p == null ? "" : p.ToBase58())
                .ToArray();
            if (SupportsProofs())
            {                
                json.Add("proofs", proofs);
            }
            else
            {
                if (proofs.Length == 0)
                    throw new InvalidOperationException("Transaction is not signed");
                if (proofs.Length > 1)
                    throw new InvalidOperationException("Transaction type and version doesn't support multiple proofs");
                json.Add("signature", proofs.Single());
            }
            return json;
        }
    }

    public static class TransactionExtensons
    {
        public static T Sign<T>(this T transaction, PrivateKeyAccount account, int proofIndex = 0) where T: Transaction
        {
            transaction.Proofs[proofIndex] = account.Sign(transaction.GetBody());
            return transaction;
        }
    }
}