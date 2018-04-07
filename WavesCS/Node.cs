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
                this.Host = new Uri(defaultNode);
            }
            catch (UriFormatException e)
            {
                throw new SystemException(e.Message);
            }
        }

        public Node(String uri)
        {
            Host = new Uri(uri);
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

        public String TransferAsset(PrivateKeyAccount from, String toAddress,
                long amount, String assetId, long fee, String feeAssetId, String message)
        {
            var transaction = Transaction.MakeTransferTransaction(from, toAddress, amount, assetId, fee, feeAssetId, message);
            return Broadcast(transaction);
        }

        public String Lease(PrivateKeyAccount from, String toAddress, long amount, long fee)
        {
            var transaction = Transaction.MakeLeaseTransaction(from, toAddress, amount, fee);
            return Broadcast(transaction);
        }

        public String CancelLease(PrivateKeyAccount account, String transactionId, long fee)
        {
            var transaction = Transaction.MakeLeaseCancelTransaction(account, transactionId, fee);
            return Broadcast(transaction);
        }

        public String IssueAsset(PrivateKeyAccount account,
                String name, String description, long quantity, int decimals, bool reissuable, long fee)
        {
            var transaction = Transaction.MakeIssueTransaction(account, name, description, quantity, decimals, reissuable, fee);
            return Broadcast(transaction);
        }

        public String ReissueAsset(PrivateKeyAccount account, String assetId, long quantity, bool reissuable, long fee)
        {
            var transaction = Transaction.MakeReissueTransaction(account, assetId, quantity, reissuable, fee);
            return Broadcast(transaction);
        }

        public String BurnAsset(PrivateKeyAccount account, String assetId, long amount, long fee)
        {
            var transaction = Transaction.MakeBurnTransaction(account, assetId, amount, fee);
            return Broadcast(transaction);
        }

        public String Alias(PrivateKeyAccount account, String alias, char scheme, long fee)
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


        public String GetMatcherKey()
        {
            String json = Request<String>(MatcherPath);           
            return json;
        }

        public String CreateOrder(PrivateKeyAccount account, String matcherKey, String amountAssetId, String priceAssetId,
                                  Order.Type orderType, long price, long amount, long expiration, long matcherFee)
        {
            Transaction transaction = Transaction.MakeOrderTransaction(account, matcherKey, orderType,
                    amountAssetId, priceAssetId, price, amount, expiration, matcherFee);            
            return Request(transaction, false); 
        }

        public void CancelOrder(PrivateKeyAccount account,
                String amountAssetId, String priceAssetId, String orderId, long fee)
        {
            Transaction transaction = Transaction.MakeOrderCancelTransaction(account, amountAssetId, priceAssetId, orderId, fee);
            Request(transaction, true);
        }

        public OrderBook GetOrderBook(String asset1, String asset2)
        {
            asset1 = Transaction.NormalizeAsset(asset1);
            asset2 = Transaction.NormalizeAsset(asset2);
            String path = String.Format("{0}{1}/{2}", OrderBook.BasePath, asset1, asset2);
            OrderBook.JsonOrderBook orderBookJson = Request<OrderBook.JsonOrderBook>(path);
            return new OrderBook(orderBookJson);
        }

        public String GetOrderStatus(String orderId, String asset1, String asset2)
        {
            asset1 = Transaction.NormalizeAsset(asset1);
            asset2 = Transaction.NormalizeAsset(asset2);
            String path = String.Format("{0}{1}/{2}/{3}", OrderBook.BasePath, asset1, asset2, orderId);
            dynamic result = Get<String>(path, OrderStatusString);
            return result;
        }

        public String GetOrders(PrivateKeyAccount account)
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
                string jsonTransaction = serializer.Serialize(transaction.Data);
                Uri currentUri = new Uri(Host + transaction.Endpoint);
                WebClient currentClient = GetClientWithHeaders();
                var json = currentClient.UploadString(currentUri, jsonTransaction);
                Transaction.JsonTransactionWithStatus result = serializer.Deserialize<Transaction.JsonTransactionWithStatus>(json);
                if (!isCancel)
                {
                    Transaction.JsonTransaction message = result.Message;
                    return message.Id;
                }
                return "";
            }
            catch (WebException e)
            {
                var resp = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                Transaction.JsonTransactionError error = serializer.Deserialize<Transaction.JsonTransactionError>(resp);
                throw new IOException(error.Message.ToString());
            }
        }

        private T Request<T>(String path, params String[] headers)
        {
            Uri currentUri = new Uri(Host + path);
            WebClient currentClient = GetClientWithHeaders();
            for (int i = 0; i < headers.Length; i += 2)
            {
                currentClient.Headers.Add(headers[i], headers[i + 1]);
            }
            var json = currentClient.DownloadString(currentUri);
            T result = serializer.Deserialize<T>(json);
            return result;
        }

        private WebClient GetClientWithHeaders()
        {
            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            client.Headers.Add("Accept", "application/json");
            return client;
        }
    }
}
