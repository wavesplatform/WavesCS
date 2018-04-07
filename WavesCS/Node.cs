using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

namespace WavesCS
{
    public class Node
    {
        public static readonly String defaultNode = "https://testnode1.wavesnodes.com";

        private readonly Uri Host;        
        private static JavaScriptSerializer serializer = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };

        private const string MatcherPath = "/matcher";
        private const string OrderStatusString = "status";

        public Node()
        {
            try
            {
                _host = new Uri(DefaultNode);
            }
            catch (UriFormatException e)
            {
                throw new SystemException(e.Message);
            }
        }

        public Node(string uri)
        {
            _host = new Uri(uri);
        }

        public T Get<T>(string url, string key)
        {
            Console.WriteLine(Host + url);            
            var json = new WebClient().DownloadString(Host + url);
            dynamic result = serializer.DeserializeObject(json);
            return result[key];
        }
        
        public T Get<T>(string url)
        {
            Console.WriteLine(Host + url);            
            var json = new WebClient().DownloadString(Host + url);
            dynamic result = serializer.DeserializeObject(json);
            return result;
        }

        public int GetHeight()
        {
            return Get<int>("blocks/height", "height");
        }

        public long GetBalance(string address)
        {
            return Get<long>($"addresses/balance/{address}", "balance");
        }

        public long GetBalance(string address, int confirmations)
        {
            return Get<long>($"addresses/balance/{address}/{confirmations}", "balance");
        }

        public long GetBalance(string address, string assetId)
        {
            return Get<long>($"assets/balance/{address}/{assetId}", "balance"); 
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

        // Matcher transactions


        public string GetMatcherKey()
        {
            var json = Request<string>(MatcherPath);           
            return json;
        }

        public string CreateOrder(PrivateKeyAccount account, string matcherKey, string amountAssetId, string priceAssetId,
                                  Order.OrderType orderType, long price, long amount, long expiration, long matcherFee)
        {
            var transaction = Transaction.MakeOrderTransaction(account, matcherKey, orderType,
                    amountAssetId, priceAssetId, price, amount, expiration, matcherFee);            
            return Request(transaction, false); 
        }

        public void CancelOrder(PrivateKeyAccount account,
                string amountAssetId, string priceAssetId, string orderId, long fee)
        {
            var transaction = Transaction.MakeOrderCancelTransaction(account, amountAssetId, priceAssetId, orderId, fee);
            Request(transaction, true);
        }

        public OrderBook GetOrderBook(string asset1, string asset2)
        {
            asset1 = Transaction.NormalizeAsset(asset1);
            asset2 = Transaction.NormalizeAsset(asset2);
            string path = $"matcher/orderbook/{asset1}/{asset2}";
            var orderBookJson = Request<OrderBook.JsonOrderBook>(path);
            return new OrderBook(orderBookJson);
        }

        public string GetOrderStatus(string orderId, string asset1, string asset2)
        {
            asset1 = Transaction.NormalizeAsset(asset1);
            asset2 = Transaction.NormalizeAsset(asset2);
            string path = $"matcher/orderbook/{asset1}/{asset2}/{orderId}";
            dynamic result = Get<string>(path, OrderStatusString);
            return result;
        }

        public string GetOrders(PrivateKeyAccount account)
        {
            long timestamp = Utils.CurrentTimestamp();
            var stream = new MemoryStream(40);
            var writer = new BinaryWriter(stream);
            writer.Write(account.PublicKey);

            writer.WriteLong(timestamp);
            string signature = Transaction.Sign(account, stream);
            string path = OrderBook.BasePath + Base58.Encode(account.PublicKey);
            string json = Request<String>(path, "Timestamp", Convert.ToString(timestamp), "Signature", signature);

            return json;
        }

        public string Broadcast(Transaction transaction)
        {
            var result = "";
            try
            {                
                var uri = new Uri(Host + transaction.Endpoint);
                var currentClient = GetClientWithHeaders();
                var json = currentClient.UploadString(uri, serializer.Serialize(transaction.Data));
                var jsonTransaction = serializer.Deserialize<Transaction.JsonTransaction>(json);

                return jsonTransaction.Id;
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    var resp = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    throw new IOException(resp); 
                }
            }
            return result;
        }

        private string  Request(Transaction transaction, bool isCancel)
        {            
            try
            {
                string jsonTransaction = Serializer.Serialize(transaction.Data);
                Uri currentUri = new Uri(_host + transaction.Endpoint);
                var clint = GetClientWithHeaders();
                var json = clint.UploadString(currentUri, jsonTransaction);
                var result = Serializer.Deserialize<Transaction.JsonTransactionWithStatus>(json);
                if (!isCancel)
                {
                    var message = result.Message;
                    return message.Id;
                }
                return "";
            }
            catch (WebException e)
            {
                var resp = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                Transaction.JsonTransactionError error = Serializer.Deserialize<Transaction.JsonTransactionError>(resp);
                throw new IOException(error.Message);
            }
        }

        private T Request<T>(string path, params string[] headers)
        {
            var uri = new Uri(_host + path);
            var client = GetClientWithHeaders();
            for (int i = 0; i < headers.Length; i += 2)
            {
                client.Headers.Add(headers[i], headers[i + 1]);
            }
            var json = client.DownloadString(uri);
            var result = Serializer.Deserialize<T>(json);
            return result;
        }

        private static WebClient GetClientWithHeaders()
        {
            var client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            client.Headers.Add("Accept", "application/json");
            return client;
        }
    }
}
