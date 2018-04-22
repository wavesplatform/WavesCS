using System;
using System.Collections.Generic;

namespace WavesCS
{
    public class Node
    {
        private const string DefaultNode = "https://testnode2.wavesnodes.com";

        private readonly string _host;                

        public Node(string nodeHost = DefaultNode)
        {
            _host = nodeHost;
        }

        public int GetHeight()
        {
            return Api.GetObject($"{_host}/blocks/height").GetInt("height");
        }

        public long GetBalance(string address)
        {
            return  Api.GetObject($"{_host}/addresses/balance/{address}").GetLong("balance");
        }

        public long GetBalance(string address, int confirmations)
        {
            return Api.GetObject($"{_host}/addresses/balance/{address}/{confirmations}").GetLong("balance");
        }

        public long GetBalance(string address, string assetId)
        {
            return Api.GetObject($"{_host}/assets/balance/{address}/{assetId}").GetLong("balance"); 
        }
      
        public string Transfer(PrivateKeyAccount from, String toAddress, long amount, long fee, String message)
        { 
            var transaction = Transaction.MakeTransferTransaction(from, toAddress, amount, null, fee, null, message);
            return Broadcast(transaction);
        }

        public string TransferAsset(PrivateKeyAccount from, string toAddress,
                long amount, string assetId, long fee, string feeAssetId, string message)
        {
            var transaction = Transaction.MakeTransferTransaction(from, toAddress, amount, assetId, fee, feeAssetId, message);
            return Broadcast(transaction);
        }

        public string Lease(PrivateKeyAccount from, string toAddress, long amount, long fee)
        {
            var transaction = Transaction.MakeLeaseTransaction(from, toAddress, amount, fee);
            return Broadcast(transaction);
        }

        public string CancelLease(PrivateKeyAccount account, string transactionId, long fee)
        {
            var transaction = Transaction.MakeLeaseCancelTransaction(account, transactionId, fee);
            return Broadcast(transaction);
        }

        public string IssueAsset(PrivateKeyAccount account,
                string name, string description, long quantity, int decimals, bool reissuable, long fee)
        {
            var transaction = Transaction.MakeIssueTransaction(account, name, description, quantity, decimals, reissuable, fee);
            return Broadcast(transaction);
        }

        public string ReissueAsset(PrivateKeyAccount account, string assetId, long quantity, bool reissuable, long fee)
        {
            var transaction = Transaction.MakeReissueTransaction(account, assetId, quantity, reissuable, fee);
            return Broadcast(transaction);
        }

        public string BurnAsset(PrivateKeyAccount account, string assetId, long amount, long fee)
        {
            var transaction = Transaction.MakeBurnTransaction(account, assetId, amount, fee);
            return Broadcast(transaction);
        }

        public string Alias(PrivateKeyAccount account, string alias, char scheme, long fee)
        {
            var transaction = Transaction.MakeAliasTransaction(account, alias, scheme, fee);
            return Broadcast(transaction);
        }
        
        public String PutData(PrivateKeyAccount account, Dictionary<string, object> entries, long fee)
        {
            var transaction = Transaction.MakeDataTransaction(account, entries, fee);
            return Broadcast(transaction);
        }

        public string Broadcast(Transaction transaction)
        {
            return Api.Post($"{_host}/transactions/broadcast", transaction.Data);
        }
    }
}
