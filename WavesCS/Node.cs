using System;
using System.Text;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
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
            return Http.GetObject($"{_host}/{url}", args);
        }
        
        public IEnumerable<Dictionary<string, object>> GetObjects(string url, params object[] args)
        {
            return Http.GetObjects($"{_host}/{url}", args);
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

        public string ListRawTransactions(string address, int limit=50)
        {
            return Http.GetRawString($"{_host}/transactions/address/{address}/limit/{limit}");
        }
        
        Asset GetAssetFromCacheOrNode(DictionaryObject tx, Dictionary<string, Asset> cache)
        {
            var assetId = tx.GetString("assetId");
            Asset asset = null;
            if (cache.ContainsKey(assetId))
                asset = cache[assetId];
            else
            {
                asset = GetAsset(assetId);
                cache[assetId] = asset;
            }
            return asset;
        }

        public IEnumerable<Transaction> ListTransactions(string address, int limit=50)
        {
            // get the raw json and then manually remove the extra '[' & ']' characters from the json
            // before parsing the json into a list of objects
            var json = Http.GetRawString($"{_host}/transactions/address/{address}/limit/{limit}");
            string pattern = @"^\s*\[\s*(\[.*\])\s*\]\s*$";       
            var m = Regex.Match(json, pattern, RegexOptions.Singleline);
            if (m.Success)
                json = m.Groups[1].Value;
            var txs_ = json.ParseJsonObjects();

            var assets = new Dictionary<string, Asset>();
            var txs = new List<Transaction>();
            foreach (var tx_ in txs_)
            {
                var senderPublicKey = Base58.Decode(tx_.GetString("senderPublicKey"));
                var type = (TransactionType)tx_.GetInt("type");
                Transaction tx = new UnknownTransaction(senderPublicKey, (int)type);
                switch (type)
                {
                    case TransactionType.Issue:
                        var decimals = (byte)tx_.GetInt("decimals");
                        tx = new IssueTransaction(senderPublicKey, tx_.GetString("name"), tx_.GetString("description"),
                            Asset.AmountToLong(decimals, tx_.GetLong("quantity")), decimals, tx_.GetBool("reissuable"),
                            Asset.AmountToLong(decimals, tx_.GetLong("fee")));
                        break;
                    case TransactionType.Transfer:
                    {
                        var asset = GetAssetFromCacheOrNode(tx_, assets);
                        byte[] attachment = Encoding.UTF8.GetBytes(tx_.GetString("attachment"));
                        tx = new TransferTransaction(senderPublicKey, tx_.GetString("recipient"), asset,
                            asset.LongToAmount(tx_.GetLong("amount")), asset.LongToAmount(tx_.GetLong("fee")), asset, attachment);
                        break;
                    }
                    case TransactionType.Reissue:
                    {
                        var asset = GetAssetFromCacheOrNode(tx_, assets);
                        tx = new ReissueTransaction(senderPublicKey, asset, asset.LongToAmount(tx_.GetLong("quantity")),
                            tx_.GetBool("reissuable"), asset.LongToAmount(tx_.GetLong("fee")));
                        break;
                    }
                    case TransactionType.Burn:
                    {
                        var asset = GetAssetFromCacheOrNode(tx_, assets);
                        tx = new BurnTransaction(senderPublicKey, asset, asset.LongToAmount(tx_.GetLong("quantity")), asset.LongToAmount(tx_.GetLong("fee")));
                        break;
                    }
                    case TransactionType.Lease:
                        tx = new LeaseTransaction(senderPublicKey, tx_.GetString("recipient"), Assets.WAVES.LongToAmount(tx_.GetLong("amount")),
                            Assets.WAVES.LongToAmount(tx_.GetLong("fee")));
                        break;
                    case TransactionType.LeaseCancel:
                        tx = new CancelLeasingTransaction(senderPublicKey, tx_.GetString("leaseid"), Assets.WAVES.LongToAmount(tx_.GetLong("fee")));
                        break;
                    case TransactionType.Alias:
                        tx = new AliasTransaction(senderPublicKey, tx_.GetString("alias"), '0'/*???*/, Assets.WAVES.LongToAmount(tx_.GetLong("fee")));
                        break;
                    case TransactionType.MassTransfer:
                    {
                        var asset = GetAssetFromCacheOrNode(tx_, assets);
                        byte[] attachment = Encoding.UTF8.GetBytes(tx_.GetString("attachment"));
                        var transfers = new List<MassTransferItem>();
                        foreach (var transfer_ in tx_.GetObjects("transfers"))
                            transfers.Add(new MassTransferItem(transfer_.GetString("recipient"), asset.LongToAmount(tx_.GetLong("amount"))));
                        tx = new MassTransferTransaction(senderPublicKey, asset, transfers, attachment, asset.LongToAmount(tx_.GetLong("fee")));
                        break;
                    }
                    case TransactionType.DataTx:
                        var entries = new Dictionary<string, object>();
                        foreach (var entry_ in tx_.GetObjects("data"))
                        {
                            var key = entry_.GetString("key");
                            var entryType = entry_.GetString("type");
                            var value = entry_.GetString("value");
                            object entry = value;
                            switch (entryType)
                            {
                                case "binary":
                                    if (value.StartsWith("base64:") && value.Length > 7)                                  
                                        entry = Convert.FromBase64String(value.Substring(7));
                                    break;
                                case "boolean":
                                    entry = Convert.ToBoolean(value);
                                    break;
                                case "integer":
                                    entry = Convert.ToInt64(value);
                                    break;
                            }
                            entries.Add(key, value);
                        }
                        tx = new DataTransaction(senderPublicKey, entries, Assets.WAVES.LongToAmount(tx_.GetLong("fee")));
                        break;
                }
                txs.Add(tx);
            }
            return txs;
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
            var assetId = response.ParseJsonObject().GetString("id");
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
            return Http.Post($"{_host}/transactions/broadcast", transaction.GetJsonWithSignature());
        }
        
        public string Broadcast(DictionaryObject transaction)
        {
            return Http.Post($"{_host}/transactions/broadcast", transaction);
        }

    }
}
