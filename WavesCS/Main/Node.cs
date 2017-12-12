using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace WavesCS.Main
{
    public class Node
    {
        public static readonly String DEFAULT_NODE = "https://testnode1.wavesnodes.com";

        private readonly Uri host;
        private readonly WebClient client = new WebClient();
        private static JavaScriptSerializer serializer = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
        private const string HEIGHT_STRING = "height";
        private const string BALANCE_STRING = "balance";
        private const string BASE_PATH_BLOCKS = "blocks/";
        private const string BASE_PATH_ASSETS = "assets/";
        private const string BASE_PATH_ADDRESS = "addresses/";
        private const string MATCHER_PATH = "/matcher";
        private const string ORDER_STATUS_STRING = "status";

        public Node()
        {
            try
            {
                this.host = new Uri(DEFAULT_NODE);
            }
            catch (UriFormatException e)
            {
                throw new SystemException(e.Message);
            }
        }

        public Node(String uri)
        {
            host = new Uri(uri);
        }

        public T Get<T>(string url, string key)
        {
            Console.WriteLine(host + url);
            Uri resorce = new Uri(host, url);
            var json = (new WebClient()).DownloadString(host + url);
            dynamic result = serializer.DeserializeObject(json);
            return result[key];
        }

        public int GetHeight()
        {
            return Get<int>(String.Format("{0}{1}", BASE_PATH_BLOCKS, HEIGHT_STRING), HEIGHT_STRING);
        }

        public long GetBalance(String address)
        {
            return Get<long>(String.Format("{0}{1}/{2}", BASE_PATH_ADDRESS, BALANCE_STRING, address), BALANCE_STRING);
        }

        public long GetBalance(String address, int confirmations)
        {
            return Get<long>(String.Format("{0}{1}/{2}/{3}", BASE_PATH_ADDRESS, BALANCE_STRING, address, confirmations), BALANCE_STRING);
        }

        public long GetBalance(String address, String assetId)
        {
            return Get<long>(String.Format("{0}{1}/{2}/{3}",BASE_PATH_ASSETS, BALANCE_STRING, address, assetId), BALANCE_STRING); 
        }

        public String Transfer(PrivateKeyAccount from, String toAddress, long amount, long fee, String message)
        {
            Transaction transaction = Transaction.MakeTransferTransaction(from, toAddress, amount, null, fee, null, message);
            return Send(transaction);
        }

        public String TransferAsset(PrivateKeyAccount from, String toAddress,
                long amount, String assetId, long fee, String feeAssetId, String message)
        {
            Transaction transaction = Transaction.MakeTransferTransaction(from, toAddress, amount, assetId, fee, feeAssetId, message);
            return Send(transaction);
        }

        public String Lease(PrivateKeyAccount from, String toAddress, long amount, long fee)
        {
            Transaction transaction = Transaction.MakeLeaseTransaction(from, toAddress, amount, fee);
            return Send(transaction);
        }

        public String CancelLease(PrivateKeyAccount account, String transactionId, long fee)
        {
            Transaction transaction = Transaction.MakeLeaseCancelTransaction(account, transactionId, fee);
            return Send(transaction);
        }

        public String IssueAsset(PrivateKeyAccount account,
                String name, String description, long quantity, int decimals, bool reissuable, long fee)
        {
            Transaction transaction = Transaction.MakeIssueTransaction(account, name, description, quantity, decimals, reissuable, fee);
            return Send(transaction);
        }

        public String ReissueAsset(PrivateKeyAccount account, String assetId, long quantity, bool reissuable, long fee)
        {
            Transaction transaction = Transaction.MakeReissueTransaction(account, assetId, quantity, reissuable, fee);
            return Send(transaction);
        }

        public String BurnAsset(PrivateKeyAccount account, String assetId, long amount, long fee)
        {
            Transaction transaction = Transaction.MakeBurnTransaction(account, assetId, amount, fee);
            return Send(transaction);
        }

        public String Alias(PrivateKeyAccount account, String alias, char scheme, long fee)
        {
            Transaction transaction = Transaction.MakeAliasTransaction(account, alias, scheme, fee);
            return Send(transaction);
        }

        // Matcher transactions


        public String GetMatcherKey()
        {
            String json = Request<String>(MATCHER_PATH);           
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
            String path = String.Format("{0}{1}/{2}", OrderBook.BASE_PATH, asset1, asset2);
            OrderBook.JsonOrderBook orderBookJson = Request<OrderBook.JsonOrderBook>(path);
            return new OrderBook(orderBookJson);
        }

        public String GetOrderStatus(String orderId, String asset1, String asset2)
        {
            asset1 = Transaction.NormalizeAsset(asset1);
            asset2 = Transaction.NormalizeAsset(asset2);
            String path = String.Format("{0}{1}/{2}/{3}", OrderBook.BASE_PATH, asset1, asset2, orderId);
            dynamic result = Get<String>(path, ORDER_STATUS_STRING);
            return result;
        }

        public String GetOrders(PrivateKeyAccount account)
        {
            long timestamp = Utils.CurrentTimestamp();
            MemoryStream stream = new MemoryStream(40);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(account.PublicKey);
            Utils.WriteToNetwork(writer, timestamp);
            String signature = Transaction.Sign(account, stream);
            String path = OrderBook.BASE_PATH + Base58.Encode(account.PublicKey);
            String json = Request<String>(path, "Timestamp", Convert.ToString(timestamp), "Signature", signature);
            return json;
        } 

        private String  Send(Transaction transaction)
        {
            String result = "";
            try
            {                
                Uri currentUri = new Uri(host + transaction.Endpoint);
                WebClient currentClient = GetClientWithHeaders();
                string json = currentClient.UploadString(currentUri, serializer.Serialize(transaction.Data));
                Transaction.JsonTransaction jsonTransaction = serializer.Deserialize<Transaction.JsonTransaction>(json);
                return jsonTransaction.Id;
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    var resp = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    Transaction.JsonTransactionError error = serializer.Deserialize<Transaction.JsonTransactionError>(resp);
                    throw new IOException(error.Message.ToString()); 
                }
            }
            return result;
        }

        private string  Request(Transaction transaction, bool isCancel)
        {            
            try
            {
                string jsonTransaction = serializer.Serialize(transaction.Data);
                Uri currentUri = new Uri(host + transaction.Endpoint);
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
            Uri currentUri = new Uri(host + path);
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
