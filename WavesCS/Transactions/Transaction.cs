using System;
using System.Linq;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public abstract class Transaction
    {
        public DateTime Timestamp { get; protected set; }

        public byte[] SenderPublicKey { get; }
        public string Sender { get; }
        public decimal Fee { get; set; }

        public virtual byte Version { get; set; }

        public abstract byte[] GetBody();
        public abstract byte[] GetIdBytes();
        public abstract DictionaryObject GetJson();

        public byte[][] Proofs { get; }

        public static bool checkId = false;

        protected Transaction(byte[] senderPublicKey)
        {
            Timestamp = DateTime.UtcNow;
            SenderPublicKey = senderPublicKey;
            Proofs = new byte[8][];
        }

        protected Transaction(DictionaryObject tx)
        {
            Timestamp = tx.GetDate("timestamp");
            Sender = tx.ContainsKey("sender") ? tx.GetString("sender") : "";
            SenderPublicKey = tx.GetString("senderPublicKey").FromBase58();
            Version = tx.ContainsKey("version") ? tx.GetByte("version") : (byte)1;

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

        public static Transaction FromJson(DictionaryObject tx)
        {
            switch ((TransactionType)tx.GetByte("type"))
            {
                case TransactionType.Alias: return new AliasTransaction(tx);
                case TransactionType.Burn: return new BurnTransaction(tx);
                case TransactionType.DataTx: return new DataTransaction(tx);
                case TransactionType.Lease: return new LeaseTransaction(tx);
                case TransactionType.Issue: return new IssueTransaction(tx);
                case TransactionType.LeaseCancel: return new CancelLeasingTransaction(tx);
                case TransactionType.MassTransfer: return new MassTransferTransaction(tx);
                case TransactionType.Reissue: return new ReissueTransaction(tx);
                case TransactionType.SetScript: return new SetScriptTransaction(tx);
                case TransactionType.SponsoredFee: return new SponsoredFeeTransaction(tx);
                case TransactionType.Transfer: return new TransferTransaction(tx);
                case TransactionType.Exchange: return new ExchangeTransaction(tx);
                case TransactionType.SetAssetScript: return new SetAssetScriptTransaction(tx);

                default: return new UnknownTransaction(tx);
            }
        }
    }

    public static class TransactionExtensons
    {
        public static T Sign<T>(this T transaction, PrivateKeyAccount account, int proofIndex = 0) where T: Transaction
        {
            transaction.Proofs[proofIndex] = account.Sign(transaction.GetBody());
            return transaction;
        }

        public static string GenerateId<T>(this T transaction) where T : Transaction
        {
            var bodyBytes = transaction.GetIdBytes();
            return AddressEncoding.FastHash(bodyBytes, 0, bodyBytes.Length).ToBase58();
        }
    }
}
