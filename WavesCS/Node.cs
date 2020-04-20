﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Threading;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public class Node
    {
        public const string StageNetHost = "https://nodes-stagenet.wavesnodes.com";
        public const string TestNetHost = "https://nodes-testnet.wavesnodes.com";
        public const string MainNetHost = "https://nodes.wavesnodes.com";

        public const char StageNetChainId = 'S';
        public const char TestNetChainId = 'T';
        public const char MainNetChainId = 'W';

        private readonly string _host;
        public char ChainId;
        
        private Dictionary<string, Asset> AssetsCache;

        public Node(string nodeHost, char nodeChainId)
        {           
            if (nodeHost.EndsWith("/", StringComparison.InvariantCulture))
                nodeHost = nodeHost.Substring(0, nodeHost.Length - 1);

            _host = nodeHost;

            ChainId = nodeChainId;

            AssetsCache = new Dictionary<string, Asset>();
        }

        public static string NodeHostByChainID(char chainId)
        {
            switch (chainId)
            {
                case StageNetChainId : return StageNetHost;
                case TestNetChainId  : return TestNetHost;
                case MainNetChainId  : return MainNetHost;
                default: throw new ArgumentException("Unknown chainId: " + chainId);
            }
        }

        public Node(char nodeChainId = TestNetChainId) : this(NodeHostByChainID(nodeChainId), nodeChainId) { }

        public DictionaryObject GetObject(string url, params object[] args)
        {
            return Http.GetObject($"{_host}/{url}", args);
        }

        public IEnumerable<DictionaryObject> GetObjects(string url, params object[] args)
        {
            return Http.GetObjects($"{_host}/{url}", args);
        }

        public int GetHeight()
        {
            return GetObject("blocks/height").GetInt("height");
        }

        public int GetTransactionHeight(string transactionId)
        {
            var tx = Http.GetJson($"{_host}/transactions/info/{transactionId}")
                         .ParseJsonObject();
            return (int)(long)tx["height"];
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
            if (asset == Assets.WAVES)
                return GetBalance(address);
            else
                return GetObject($"assets/balance/{address}/{asset.Id}").GetDecimal("balance", asset);
        }
        
        public Dictionary<Asset, decimal> GetAssetBalances(string address)
        {
            return GetObject($"assets/balance/{address}")
                .GetObjects("balances")
                .Select(o =>
                {
                    var asset = GetAsset(o.GetString("assetId"));
                    return new
                    {
                        Asset = asset,
                        Balance = o.GetDecimal("balance", asset)
                    };
                })
                .ToDictionary(o => o.Asset, o => o.Balance);
        }

        public int GetUnconfirmedPoolSize()
        {
            return GetObject("transactions/unconfirmed/size").GetInt("size");
        }

        static public object DataValue(DictionaryObject o)
        {
            switch (o.GetString("type"))
            {
                case "string": return o.GetString("value");
                case "binary": return o.GetString("value").FromBase64();
                case "integer": return o.GetLong("value");
                case "boolean": return o.GetBool("value");
                default: throw new Exception("Unknown value type");
            }
        }

        public DictionaryObject GetAddressData(string address)
        {
            return GetObjects("addresses/data/{0}", address)
                .ToLookup(o => o.GetString("key"), DataValue)
                .ToDictionary(d => d.Key, d => d.First());
        }

        public Asset GetAsset(string assetId)
        {
            if (assetId == Assets.WAVES.Id || assetId == null)
                return Assets.WAVES;

            Asset asset = null;

            if (AssetsCache.ContainsKey(assetId))
                asset = AssetsCache[assetId];
            else
            {
                var tx = GetObject($"transactions/info/{assetId}");
                if ((TransactionType)tx.GetByte("type") != TransactionType.Issue)
                    throw new ArgumentException("Wrong asset id (transaction type)");

                var assetDetails = GetObject($"assets/details/{assetId}?full=true");
                var scripted = assetDetails.ContainsKey("scripted") && assetDetails.GetBool("scripted");

                var script = scripted ? assetDetails.GetString("scriptDetails.script").FromBase64() : null;

                var issueHeight = assetDetails.GetLong("issueHeight");
                var issueTimestamp = assetDetails.GetLong("issueTimestamp");
                var issuer = assetDetails.GetString("issuer");
                var description = assetDetails["description"] == null ? "" : assetDetails.GetString("description");
                var reissuable = assetDetails.GetBool("reissuable");
                var quantity = assetDetails.GetLong("quantity");
                var minSponsoredAssetFee = assetDetails["minSponsoredAssetFee"] == null ? 0 : assetDetails.GetLong("minSponsoredAssetFee");

                asset = new Asset(assetId, tx.GetString("name"), tx.GetByte("decimals"), script,
                    issueHeight, issueTimestamp, issuer, description, reissuable, quantity, minSponsoredAssetFee);
                AssetsCache[assetId] = asset;
            }

            return asset;
        }

        public Transaction[] GetTransactions(string address, int limit = 100)
        {
            return GetTransactionsByAddress(address, limit)
                .Select(tx => Transaction.FromJson(ChainId, tx, this))
                .ToArray();
        }

        public Transaction[] GetTransactionsByAddressAfterId(string address, string afterId, int limit = 100)
        {
            string path = $"{_host}/transactions/address/{address}/limit/{limit}";

            var header = new NameValueCollection{ {"after", afterId } };
            return Http.GetFlatObjectsWithHeaders(path, header)
                .Select(tx => Transaction.FromJson(ChainId, tx, this))
                .ToArray();
        }

        public Transaction[] GetTransactionsByAddressAfterTimestamp(string address, long timestamp, int limit)
        {
            var height = GetHeight();
            List<Transaction> txs = new List<Transaction>();
            var initialTxs = GetBlockTransactionsAtHeight(height);
            while (initialTxs.Length == 0)
            {
                height--;
                initialTxs = GetBlockTransactionsAtHeight(height);
            }
            string afterId = initialTxs.Last().GenerateId();
            txs.AddRange(GetTransactionsByAddressAfterId(address, afterId, limit));
            while (height > 0)
            {
                height -= limit;
                Transaction[] blockTxs = GetBlockTransactionsAtHeight(height);
                while(blockTxs.Length == 0)
                {
                    height--;
                    blockTxs = GetBlockTransactionsAtHeight(height);
                }
                afterId = blockTxs.First().GenerateId();
                foreach (var tx in blockTxs)
                {
                    if (tx.Timestamp.ToLong() <= timestamp)
                    {
                        afterId = tx.GenerateId();
                        height = 0;
                        break;
                    }
                }
                txs.AddRange(GetTransactionsByAddressAfterId(address, afterId, limit));
            }
            return txs.ToArray();
          }

        public long TransactionsCount(long height)
        {
            return Http.GetObject($"{_host}/blocks/headers/at/{height}").GetInt("transactionCount");
        }

        public long GetBlockTimestamp(long height)
        {
            return Http.GetObject($"{_host}/blocks/headers/at/{height}").GetLong("timestamp");
        }

        public long GetBlockTotalFee(long height)
        {
            var t = Http.GetObject($"{_host}/blocks/headers/at/{height}");
            return t.GetLong("totalFee");
        }

        public TransactionType TransactionTypeId(Type transactionType)
        {
            switch (transactionType.Name)
            {
                case nameof(IssueTransaction): return TransactionType.Issue;
                case nameof(TransferTransaction): return TransactionType.Transfer;
                case nameof(ReissueTransaction): return TransactionType.Reissue;
                case nameof(BurnTransaction): return TransactionType.Burn;
                case nameof(ExchangeTransaction): return TransactionType.Exchange;
                case nameof(LeaseTransaction): return TransactionType.Lease;
                case nameof(CancelLeasingTransaction): return TransactionType.LeaseCancel;
                case nameof(AliasTransaction): return TransactionType.Alias;
                case nameof(MassTransferTransaction): return TransactionType.MassTransfer;
                case nameof(DataTransaction): return TransactionType.DataTx;
                case nameof(SetScriptTransaction): return TransactionType.SetScript;
                case nameof(SponsoredFeeTransaction): return TransactionType.SponsoredFee;
                default: throw new Exception("Unknown transaction type");
            }
        }

        public T[] GetTransactions<T>(string address, int limit = 100) where T : Transaction
        {
            var typeId = TransactionTypeId(typeof(T));

            return GetTransactionsByAddress(address, limit)
                .Where(tx => (TransactionType)tx.GetByte("type") == typeId)
                .Select(tx => Transaction.FromJson(ChainId, tx, this))
                .Cast<T>()
                .ToArray();
        }


        public Transaction GetTransactionById(string transactionId)
        {
            var tx = Http.GetJson($"{_host}/transactions/info/{transactionId}")
                         .ParseJsonObject();

            return Transaction.FromJson(ChainId, tx, this);
        }

        public Transaction GetTransactionByIdOrNull(string transactionId)
        {
            try
            {
                return GetTransactionById(transactionId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Transaction[] GetBlockTransactionsAtHeight(long height)
        {
            var block = GetObject($"blocks/at/{height}");
            var transactions = block.ContainsKey("transactions") ? block
                .GetObjects("transactions").Select(tx => {
                    return Transaction.FromJson(ChainId, tx, this);
                    }).ToArray() : null;
            return transactions;
        }

        public string Transfer(PrivateKeyAccount sender, string recipient, Asset asset, decimal amount,
            string message = "")
        {
            var tx = new TransferTransaction(ChainId, sender.PublicKey, recipient, asset, amount, message);
            tx.Fee = CalculateFee(tx);
            tx.Sign(sender);
            return Broadcast(tx);
        }

        public string Transfer(PrivateKeyAccount sender, string recipient, Asset asset, decimal amount,
                               decimal fee, Asset feeAsset = null, byte[] message = null)
        {
            var tx = new TransferTransaction(ChainId, sender.PublicKey, recipient, asset, amount, fee, feeAsset, message);           
            tx.Sign(sender);
            return Broadcast(tx);
        }

        public string MassTransfer(PrivateKeyAccount sender, Asset asset, IEnumerable<MassTransferItem> transfers,
            string message = "", decimal? fee = null)
        {
            var tx = new MassTransferTransaction(ChainId, sender.PublicKey, asset, transfers, message, fee);
            tx.Sign(sender);
            return Broadcast(tx);
        }

        public string MassTransfer(PrivateKeyAccount sender, Asset asset, string recipientsListFile,
           string message = "", decimal? fee = null)
        {
            string line;
            List<MassTransferItem> transfers = new List<MassTransferItem>();
 
            System.IO.StreamReader file =
                new System.IO.StreamReader(recipientsListFile);
            while ((line = file.ReadLine()) != null)
            {
                var item = line.Split(new char[] { ',' });
                var amount = decimal.Parse(item[1], CultureInfo.GetCultureInfo("en-US"));
                transfers.Add(new MassTransferItem(item[0], amount));
            }
            file.Close();
            
            var tx = new MassTransferTransaction(ChainId, sender.PublicKey, asset, transfers, message, fee);
            tx.Sign(sender);
            return Broadcast(tx);
        }

        public string Lease(PrivateKeyAccount sender, string recipient, decimal amount, decimal fee = 0.001m)
        {
            var tx = new LeaseTransaction(ChainId, sender.PublicKey, recipient, amount, fee);
            tx.Sign(sender);
            return Broadcast(tx);
        }
       
        public string CancelLease(PrivateKeyAccount account, string transactionId, decimal fee = 0.001m)
        {
            var tx = new CancelLeasingTransaction(ChainId, account.PublicKey, transactionId, fee);
            tx.Sign(account);
            return Broadcast(tx);
        }

        public Asset IssueAsset(PrivateKeyAccount account,
            string name, string description, decimal quantity, byte decimals, bool reissuable, byte[] script = null, decimal fee = 1m)
        {
            var tx = new IssueTransaction(account.PublicKey, name, description, quantity, decimals, reissuable, ChainId, fee, script);
            tx.Sign(account);
            var response = Broadcast(tx);
            var assetId = response.ParseJsonObject().GetString("id");
            return new Asset(assetId, name, decimals, script);
        }

        public string ReissueAsset(PrivateKeyAccount account, Asset asset, decimal quantity, bool reissuable, decimal fee = 1m)
        {
            var tx = new ReissueTransaction(ChainId, account.PublicKey, asset, quantity, reissuable, fee);
            tx.Sign(account);
            return Broadcast(tx);
        }

        public string BurnAsset(PrivateKeyAccount account, Asset asset, decimal amount, decimal fee = 0.001m)
        {
            var tx = new BurnTransaction(ChainId, account.PublicKey, asset, amount, fee).Sign(account);
            tx.Sign(account);
            return Broadcast(tx);
        }

        public string CreateAlias(PrivateKeyAccount account, string alias, decimal fee = 0.001m)
        {
            var tx = new AliasTransaction(account.PublicKey, alias, ChainId, fee);
            tx.Sign(account);
            return Broadcast(tx);
        }

        public string SponsoredFeeForAsset(PrivateKeyAccount account, Asset asset, decimal minimalFeeInAssets, decimal fee = 1m)
        {
            var tx = new SponsoredFeeTransaction(ChainId, account.PublicKey, asset, minimalFeeInAssets, fee);
            tx.Sign(account);
            return Broadcast(tx);
        }

        public string SetAssetScript(PrivateKeyAccount account, Asset asset, byte[] script, decimal fee = 1m)
        {
            var tx = new SetAssetScriptTransaction(ChainId, account.PublicKey, asset, script, fee = 1m);
            tx.Sign(account);
            return Broadcast(tx);
        }

        public string SetScript(PrivateKeyAccount account, byte[] script, decimal fee = 1m)
        {
            var tx = new SetScriptTransaction(account.PublicKey, script, ChainId, fee = 0.014m);
            tx.Sign(account);
            return Broadcast(tx);
        }

        public string InvokeScript(PrivateKeyAccount caller, string dappAddress,
                string functionHeader, List<object> functionCallArguments,
                Dictionary<Asset, decimal> payment = null, decimal fee = 0.005m, Asset feeAsset = null)
        {
            var tx = new InvokeScriptTransaction(ChainId, caller.PublicKey, dappAddress,
            functionHeader, functionCallArguments,
                payment, fee, feeAsset);
            tx.Sign(caller);
            return Broadcast(tx);
        }

        public string InvokeScript(PrivateKeyAccount caller, string dappAddress,                
                Dictionary<Asset, decimal> payment, decimal fee = 0.005m, Asset feeAsset = null)
        {
            var tx = new InvokeScriptTransaction(ChainId, caller.PublicKey, dappAddress,
                payment, fee, feeAsset);
            tx.Sign(caller);
            return Broadcast(tx);
        }

        public string PutData(PrivateKeyAccount account, DictionaryObject entries, decimal? fee = null)
        {
            var tx = new DataTransaction(ChainId, account.PublicKey, entries, fee);
            tx.Sign(account);
            return Broadcast(tx);
        }

        public byte[] CompileScript(string script)
        {
            return Post("/utils/script/compile", script).ParseJsonObject().Get<string>("script").FromBase64();
        }

        public byte[] CompileCode(string script)
        {
            return Post("/utils/script/compileCode", script).ParseJsonObject().Get<string>("script").FromBase64();
        }

        public string DecompileScript(string script)
        {
            return Post("/utils/script/decompile", script).ParseJsonObject().Get<string>("script");
        }

        public byte[] SecureHash(string message)
        {
            return Post("/utils/hash/secure", message).ParseJsonObject().Get<string>("hash").FromBase58();
        }

        public byte[] FastHash(string message)
        {
            return Post("/utils/hash/fast", message).ParseJsonObject().Get<string>("hash").FromBase58();
        }

        public Transaction[] GetUnconfirmedTransactions()
        {
            return Http.GetObjects($"{_host}/transactions/unconfirmed").Select(tx => {
                return Transaction.FromJson(ChainId, tx, this);
            }).ToArray();
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

        public string BroadcastAndWait(Transaction transaction)
        {
            var response = Broadcast(transaction);
            WaitTransactionConfirmationByResponse(response);
            return response;
        }

        public string BroadcastAndWait(DictionaryObject transaction)
        {
            var response = Broadcast(transaction);
            WaitTransactionConfirmationByResponse(response);
            return response;
        }

        public void WaitTransactionConfirmation(string transactionId)
        {
            while (GetTransactionByIdOrNull(transactionId) == null)
            {
                Thread.Sleep(5000);
            }
            Thread.Sleep(10000);
        }

        public void WaitTransactionConfirmationByResponse(string broadcastResponse)
        {
            var transactionId = broadcastResponse.ParseJsonObject().GetString("id");
            WaitTransactionConfirmation(transactionId);
        }

        public DictionaryObject[] GetTransactionsByAddress(string address, int limit)
        {
            return Http.GetFlatObjects($"{_host}/transactions/address/{address}/limit/{limit}");
        }

        public decimal CalculateFee(Transaction transaction)
        {
            var response =  Http.Post($"{_host}/transactions/calculateFee", transaction.GetJsonWithSignature()).ParseJsonObject().GetInt("feeAmount");
            return Assets.WAVES.LongToAmount(response);
        }

        public string CalculateFee(DictionaryObject transaction)
        {
            return Http.Post($"{_host}/transactions/calculateFee", transaction);
        }

        public string GetScriptMeta(string address)
        {
            return GetObject("addresses/scriptInfo/{0}/meta", address).GetObject("meta").ToJson();
        }

        public string GetScript(string address)
        {
            return GetObject("addresses/scriptInfo/{0}", address).GetString("scriptText");
        }

        public Dictionary<string, object> GetBlockchainRewards(int height)
        {
            return GetObject("blockchain/rewards/{0}", height);
        }
    }
}
