using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
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

        public static Node DefaultNode { get; private set; }
        private Dictionary<string, Asset> AssetsCache;

        public Node(string nodeHost = TestNetHost)
        {
            if (nodeHost.EndsWith("/"))
                nodeHost = nodeHost.Substring(0, nodeHost.Length - 1);
            _host = nodeHost;

            if (DefaultNode == null)
                DefaultNode = this;
            AssetsCache = new Dictionary<string, Asset>();
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
            return GetObject($"addresses/balance/{address}").GetDecimal("balance", Assets.WAVES);
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
            if ((TransactionType) tx.GetInt("type") != TransactionType.Issue)
                throw new ArgumentException("Wrong asset id (transaction type)");

            Asset asset = null;

            if (AssetsCache.ContainsKey(assetId))
                asset = AssetsCache[assetId];
            else
            {
                asset = new Asset(assetId, tx.GetString("name"), tx.GetByte("decimals"));
                AssetsCache[assetId] = asset;
            }

            return asset;
        }

        public Transaction CreateTransactionFromJson(DictionaryObject tx)
        {
            switch ((TransactionType)tx.GetInt("type"))
            {
                case TransactionType.Alias: return (Transaction)new AliasTransaction(tx);
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
                case TransactionType.Exchange: return null;
                default: throw new Exception("Unknown transaction type: " + (TransactionType)tx.GetInt("type"));
            }
        }

        public IEnumerable<Transaction> ListTransactions(string address, int limit = 50)
        {
            return Http.GetJson($"{_host}/transactions/address/{address}/limit/{limit}")
                       .ParseFlatObjects()
                       .Select(CreateTransactionFromJson);
        }

        public Transaction GetTransactionById(string transactionId)
        {
            var tx = Http.GetJson($"{_host}/transactions/info/{transactionId}")
                       .ParseJsonObject();

            return CreateTransactionFromJson(tx);
        }

        public string Transfer(PrivateKeyAccount sender, string recipient, Asset asset, decimal amount,
            string message = "")
        {
            var tx = new TransferTransaction(sender.PublicKey, recipient, asset, amount, message);
            tx.Sign(sender);
            return Broadcast(tx);
        }

        public string Transfer(PrivateKeyAccount sender, string recipient, Asset asset, decimal amount,
          decimal fee, Asset feeAsset, byte[] message = null)
        {
            var tx = new TransferTransaction(sender.PublicKey, recipient, asset, amount, fee, feeAsset, message);           
            tx.Sign(sender);
            return Broadcast(tx);
        }

        public string MassTransfer(PrivateKeyAccount sender, Asset asset, IEnumerable<MassTransferItem> transfers,
            string message = "")
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
            var tx = new BurnTransaction(account.PublicKey, asset, amount).Sign(account);
            tx.Sign(account);
            return Broadcast(tx);
        }

        public string CreateAlias(PrivateKeyAccount account, string alias, char scheme, long fee)
        {
            var tx = new AliasTransaction(account.PublicKey, alias, scheme);
            tx.Sign(account);
            return Broadcast(tx);
        }

        public string SponsoredFeeForAsset(PrivateKeyAccount account, Asset asset, decimal minimalFeeInAssets)
        {
            var tx = new SponsoredFeeTransaction(account.PublicKey, asset, minimalFeeInAssets);
            tx.Sign(account);
            return Broadcast(tx);
        }

        public string PutData(PrivateKeyAccount account, DictionaryObject entries)
        {
            var tx = new DataTransaction(account.PublicKey, entries);
            tx.Sign(account);
            return Broadcast(tx);
        }

        public byte[] CompileScript(string script)
        {
            return Post("/utils/script/compile", script).ParseJsonObject().Get<string>("script").FromBase64();
        }

        public string Post(string url, string data)
        {
            return Http.Post(_host + url, data);
        }
        
        public string Broadcast(Transaction transaction)
        {
            return Http.Post($"{_host}/transactions/broadcast", transaction.GetJsonWithSignature());
        }
        
        public string Broadcast(DictionaryObject transaction)
        {
            return Http.Post($"{_host}/transactions/broadcast", transaction);
        }

        public string BatchBroadcast(IEnumerable<Transaction> transactions)
        {
            var data = transactions.Select(t => t.GetJsonWithSignature()).ToArray();
            return Http.Post($"{_host}/assets/broadcast/batch-transfer", data);
        }

        public DictionaryObject[] GetTransationsByAddress(string address, int limit)
        {
            return Http.GetFlatObjects($"{_host}/transactions/address/{address}/limit/{limit}");
        }
    }
}
