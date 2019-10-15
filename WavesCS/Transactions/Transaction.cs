using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public abstract class Transaction 
    {
        public DateTime Timestamp { get; set; }

        public byte[] SenderPublicKey { get; }
        public string Sender { get; }
        public decimal Fee { get; set; }
        public char ChainId { get; set; }

        public virtual byte Version { get; set; }

        public abstract byte[] GetBytes();
        public abstract DictionaryObject GetJson();
        public abstract byte[] GetBody();
        protected abstract bool SupportsProofs();

        public byte[][] Proofs { get; set; }

        public static bool checkId = false;

        protected Transaction(char chainId, byte[] senderPublicKey)
        {
            Timestamp = DateTime.UtcNow;
            SenderPublicKey = senderPublicKey;
            Proofs = new byte[8][];
            ChainId = chainId;
        }

        protected Transaction(DictionaryObject tx)
        {
            Timestamp = tx.GetDate("timestamp");
            Sender = tx.ContainsKey("sender") ? tx.GetString("sender") : "";
            SenderPublicKey = tx.GetString("senderPublicKey").FromBase58();
            Version = tx.ContainsKey("version") ? tx.GetByte("version") : (byte)1;
            ChainId = tx.ContainsKey("chainId") ? tx.GetChar("chainId") : '\0';

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

        internal virtual byte[] GetBytesForId()
        {
            return GetBody();
        }

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

        public static Transaction FromJson(char chainId, DictionaryObject tx, Node node)
        {
            tx["chainId"] = chainId;
            return FromJson(tx, node);
        }

        public static Transaction FromJson(DictionaryObject tx, Node node)
        {
            switch ((TransactionType)tx.GetByte("type"))
            {
                case TransactionType.Alias: return new AliasTransaction(tx);
                case TransactionType.Burn: return new BurnTransaction(tx, node);
                case TransactionType.DataTx: return new DataTransaction(tx);
                case TransactionType.Lease: return new LeaseTransaction(tx);
                case TransactionType.Issue: return new IssueTransaction(tx, node);
                case TransactionType.LeaseCancel: return new CancelLeasingTransaction(tx);
                case TransactionType.MassTransfer: return new MassTransferTransaction(tx, node);
                case TransactionType.Reissue: return new ReissueTransaction(tx, node);
                case TransactionType.SetScript: return new SetScriptTransaction(tx);
                case TransactionType.SponsoredFee: return new SponsoredFeeTransaction(tx, node);
                case TransactionType.Transfer: return new TransferTransaction(tx, node);
                case TransactionType.Exchange: return new ExchangeTransaction(tx, node);
                case TransactionType.SetAssetScript: return new SetAssetScriptTransaction(tx, node);
                case TransactionType.InvokeScript: return new InvokeScriptTransaction(tx, node);

                default: return new UnknownTransaction(tx);
            }
        }

        public byte[] GetProofsBytes()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.WriteByte(1);

            var proofs = Proofs
                .Take(Array.FindLastIndex(Proofs, p => p != null && p.Length > 0) + 1)
                .Select(p => p ?? (new byte[0]))
                .ToArray();

            writer.WriteShort(proofs.Count());

            foreach(var proof in proofs)
            {
                writer.WriteShort(proof.Length);
                if (proof.Length > 0)
                    writer.Write(proof);
            }

            return stream.ToArray();
        }
    }


    public static class TransactionExtensons
    {
        public static T Sign<T>(this T transaction, PrivateKeyAccount account, int proofIndex = 0) where T : Transaction
        {
            transaction.Proofs[proofIndex] = account.Sign(transaction.GetBody());
            return transaction;
        }

        public static byte[] GenerateBinaryId<T>(this T transaction) where T : Transaction
        {
            var txBytesForId = transaction.GetBytesForId();
            return AddressEncoding.FastHash(txBytesForId, 0, txBytesForId.Length);
        }

        public static string GenerateId<T>(this T transaction) where T : Transaction
        {
            return transaction.GenerateBinaryId().ToBase58();
        }
    }
}
