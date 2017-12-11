using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using System.Linq;

namespace WavesCS.Main
{
    public class Node
    {
        public static readonly String DEFAULT_NODE = "https://testnode1.wavesnodes.com";

        private readonly Uri host;
        private readonly WebClient client = new WebClient();
        private static JavaScriptSerializer serializer = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };

        public Node()
        {
            try
            {
                this.host = new Uri(DEFAULT_NODE);
            }
            catch (UriFormatException e)
            {
                // should not happen
                throw new SystemException(e.Message);
            }
        }

        public Node(String uri)
        {
            host = new Uri(uri);
        }

        public T Get<T>(string url)
        {
            Console.WriteLine(host + url);
            Uri resorce = new Uri(host, url);
            var json = (new WebClient()).DownloadString(host + url);
            dynamic result = serializer.DeserializeObject(json);
            return result;
        }

        public int GetHeight()
        {
            return Get<dynamic>("blocks/height")["height"];
        }

        public long GetBalance(String address) 
        {
            return Get<dynamic>("addresses/balance/" + address)["balance"];
        }

        public long GetBalance(String address, int confirmations)
        {
            return Get<dynamic>("addresses/balance/" + address + "/" + confirmations)["balance"];
        }

        public long GetBalance(String address, String assetId)
        {
            return Get<dynamic>("assets/balance/" + address + "/" + assetId)["balance"]; 
        }

        /**
        * Sends a signed transaction and returns its ID.
        * @param transaction signed transaction (as created by static methods in Transaction class)
        * @return Transaction ID
        * @throws IOException
        */
        public String Send(Transaction transaction)
        {
            return Request(transaction, "id");
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
            String json = Request<String>("/matcher");           
            return json;
        }

        public String CreateOrder(PrivateKeyAccount account, String matcherKey, String amountAssetId, String priceAssetId,
                                  Order.Type orderType, long price, long amount, long expiration, long matcherFee)
        {
            try
            {
                Transaction transaction = Transaction.MakeOrderTransaction(account, matcherKey, orderType,
                        amountAssetId, priceAssetId, price, amount, expiration, matcherFee);
                dynamic message = Request<Dictionary<String, Object>>(transaction);
                return (String)message["message"]["id"];
            }
            catch (IOException)
            {
                throw;
            }
        }

        public void CancelOrder(PrivateKeyAccount account,
                String amountAssetId, String priceAssetId, String orderId, long fee)
        {
            Transaction transaction = Transaction.MakeOrderCancelTransaction(account, amountAssetId, priceAssetId, orderId, fee);
            Request<String>(transaction);
        }

        public OrderBook GetOrderBook(String asset1, String asset2)
        {
            asset1 = Transaction.NormalizeAsset(asset1);
            asset2 = Transaction.NormalizeAsset(asset2);
            String path = "matcher/orderbook/" + asset1 + '/' + asset2;
            dynamic dictionary = Request<Dictionary<String, Object>>(path);
            ArrayList bids = dictionary["bids"];
            ArrayList asks = dictionary["asks"];
            return new OrderBook(
                ProcessOrders(bids.Cast<Dictionary<string, object>>().ToList()), 
                ProcessOrders(asks.Cast<Dictionary<string, object>>().ToList()));

        }

        public String GetOrderStatus(String orderId, String asset1, String asset2)
        {
            asset1 = Transaction.NormalizeAsset(asset1);
            asset2 = Transaction.NormalizeAsset(asset2);
            String path = "matcher/orderbook/" + asset1 + '/' + asset2 + '/' + orderId;
            dynamic result = Get<Dictionary<String, object>>(path);
            return result["status"];
        }

        public String GetOrders(PrivateKeyAccount account)
        {
            long epochTicks = new DateTime(1970, 1, 1).Ticks;
            long timestamp = ((DateTime.UtcNow.Ticks - epochTicks) / TimeSpan.TicksPerSecond) * 1000;
            MemoryStream stream = new MemoryStream(40);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(account.PublicKey);
            writer.Write(timestamp);
            String signature = Transaction.Sign(account, stream);

            WebClient c;
            

            String path = "matcher/orderbook/" + Base58.Encode(account.PublicKey);
            String json = Request<String>(path, "Timestamp", Convert.ToString(timestamp), "Signature", signature);
            return json;///finish this once API is pushed
        }

        private List<Order> ProcessOrders(List<Dictionary<string, object>> orders) //List<Order> ProcessOrders<T>(List<T> orders)
        {
            return orders.Select(x => new Order(Convert.ToInt64(x["price"]), Convert.ToInt64(x["amount"]))).ToList();
        }      

        private String  Request(Transaction transaction, String key)
        {
            String result = "";
            try
            {                
                string jsonTransaction = transaction.GetJson();
                Uri currentUri = new Uri(host + transaction.Endpoint);
                WebClient currentClient = new WebClient();
                client.Headers.Add("Content-Type", "application/json");
                client.Headers.Add("Accept", "application/json");
                string json = client.UploadString(currentUri, jsonTransaction);
                dynamic usr = serializer.DeserializeObject(json);
                result = usr[key];
                return result;
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    Console.WriteLine("Status Code : {0}", ((HttpWebResponse)e.Response).StatusCode);
                    Console.WriteLine("Status Description : {0}", ((HttpWebResponse)e.Response).StatusDescription);
                    var resp = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    dynamic usr = serializer.DeserializeObject(resp);
                    result = usr["tx"][key];
                    return result;
                }
            }
            return result;
        }

        private Dictionary<String, Object>  Request<T>(Transaction transaction)
        {
            Dictionary<String, Object> result = new Dictionary<string, object>();
            try
            {
                string jsonTransaction = transaction.GetJson();
                Uri currentUri = new Uri(host + transaction.Endpoint);
                WebClient currentClient = new WebClient();
                client.Headers.Add("Content-Type", "application/json");
                client.Headers.Add("Accept", "application/json");
                var json = client.UploadString(currentUri, jsonTransaction);
                result = serializer.Deserialize<Dictionary<string, object>>(json);
                return result;
            }
            catch (WebException e)
            {
                var resp = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                var result2 = (Dictionary<string, object>)serializer.DeserializeObject(resp);
                throw new IOException(result2["message"].ToString());                              
            }
        }

        private T Request<T>(String path, params String[] headers)
        {
            Uri currentUri = new Uri(host + path);
            WebClient currentClient = new WebClient();
            currentClient.Headers.Add("Content-Type", "application/json");
            currentClient.Headers.Add("Accept", "application/json");

            for (int i = 0; i < headers.Length; i += 2)
            {
                client.Headers.Add(headers[i], headers[i + 1]);
            }
            var json = currentClient.DownloadString(currentUri);
            T result = serializer.Deserialize<T>(json);
            return result;
        }
    }
}
