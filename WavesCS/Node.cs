using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public class Node
    {
        public const string TestNetHost = "https://testnode1.wavesnodes.com";
        public const string MainNetHost = "https://nodes.wavesnodes.com";

        private readonly string _host;                

        public Node(string nodeHost = TestNetHost)
        {
            if (nodeHost.EndsWith("/"))
                nodeHost = nodeHost.Substring(0, nodeHost.Length - 1);
            _host = nodeHost;
        }

        
        public Dictionary<string, object> GetObject(string url, params object[] args)
        {            
            return Api.GetObject($"{_host}/{url}", args);
        }
        
        public IEnumerable<Dictionary<string, object>> GetObjects(string url, params object[] args)
        {
            return Api.GetObjects($"{_host}/{url}", args);
        }          
        
        public int GetHeight()
        {
            return GetObject("blocks/height").GetInt("height");
        }

        public decimal GetBalance(string address)
        {
            return  GetObject($"addresses/balance/{address}").GetDecimal("balance", Assets.WAVES);
        }

        public decimal GetBalance(string address, int confirmations)
        {
            return GetObject($"addresses/balance/{address}/{confirmations}").GetDecimal("balance", Assets.WAVES);
        }

        public decimal GetBalance(string address, Asset asset)
        {
            return GetObject($"assets/balance/{address}/{asset.Id}").GetDecimal("balance", asset); 
        }
        
        public int GetUnconfirmedPoolSize()
        {
            return GetObject("transactions/unconfirmed/size").GetInt("size"); 
        }
        
        public Dictionary<string, object> GetAddressData(string address)
        {            
            return GetObjects("addresses/data/{0}", address)
                .ToDictionary(o => o.GetString("key"), o =>
                {
                    switch (o.GetString("type"))
                    {
                        case "string": return (object) o.GetString("value");
                        case "binary": return (object) o.GetString("value").FromBase64();
                        case "integer": return (object) o.GetLong("value");
                        case "boolean": return (object) o.GetBool("value");
                        default: throw new Exception("Unknown value type");
                    }
                });
        }

        public Asset GetAsset(string assetId)
        {
            var tx = GetObject($"transactions/info/{assetId}");
            if (tx.GetInt("type") != 3)
                throw new ArgumentException("Wrong asset id (transaction type)");
            return new Asset(assetId, tx.GetString("name"), tx.GetByte("decimals"));
        }
        
        public string Transfer(PrivateKeyAccount sender, string recipient, Asset asset, decimal amount, string message = "")
        {
            var tx = new TransferTransaction(sender.PublicKey, recipient, asset, amount, message);
            tx.Sign(sender);                                   
            return Broadcast(tx);
        }
        
        public string MassTransfer(PrivateKeyAccount sender, Asset asset, IEnumerable<MassTransferItem> transfers, string message = "")
        {
            var tx = new MassTransferTransaction(sender.PublicKey, asset, transfers, message);
            tx.Sign(sender);                                   
            return Broadcast(tx);
        }
        
        public string Lease(PrivateKeyAccount sender, string recipient, decimal amount)
        {
            var tx = new LeaseTransaction(sender.PublicKey, recipient, amount);
            tx.Sign(sender);            
            return Broadcast(tx);
        }

        public string CancelLease(PrivateKeyAccount account, string transactionId)
        {
            var tx = new CancelLeasingTransaction(account.PublicKey, transactionId);
            tx.Sign(account);            
            return Broadcast(tx);
        }

        public Asset IssueAsset(PrivateKeyAccount account,
                string name, string description, decimal quantity, byte decimals, bool reissuable)
        {            
            var tx = new IssueTransaction(account.PublicKey, name, description, quantity, decimals, reissuable);
            tx.Sign(account);                
            var response = Broadcast(tx);
            var assetId = response.GetJsonObject().GetString("id");
            return new Asset(assetId, name, decimals);
        }

        public string ReissueAsset(PrivateKeyAccount account, Asset asset, decimal quantity, bool reissuable)
        {
            var tx = new ReissueTransaction(account.PublicKey, asset, quantity, reissuable);
            tx.Sign(account);          
            return Broadcast(tx);
        }

        public string BurnAsset(PrivateKeyAccount account, Asset asset, decimal amount)
        {
            var tx = new BurnTransaction(account.PublicKey, asset, amount);
            tx.Sign(account);
            return Broadcast(tx);
        }

        public string CreateAlias(PrivateKeyAccount account, string alias, char scheme, long fee)
        {
            var tx = new AliasTransaction(account.PublicKey, alias, scheme);
            tx.Sign(account);
            return Broadcast(tx);
        }
        
        public string PutData(PrivateKeyAccount account, DictionaryObject entries)
        {
            var tx = new DataTransaction(account.PublicKey, entries);
            tx.Sign(account);            
            return Broadcast(tx);
        }

        public string Broadcast(Transaction transaction)
        {
            return Api.Post($"{_host}/transactions/broadcast", transaction.GetJsonWithSignature());
        }
        
        public string Broadcast(DictionaryObject transaction)
        {
            return Api.Post($"{_host}/transactions/broadcast", transaction);
        }

    }
}
