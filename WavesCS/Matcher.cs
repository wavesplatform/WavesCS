using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public class Matcher
    {
        private readonly string _host;
        public string MatcherKey { get; }

        private string GetMatcherKey()
        {
            return Http.GetString(_host + "/matcher");            
        }

        public Matcher(string host = "https://matcher.wavesnodes.com", string matcherKey = null)
        {
            _host = host;
            MatcherKey = matcherKey ?? GetMatcherKey();
        }

        public string PlaceOrder(PrivateKeyAccount sender, Order order)
        {
            order.Sign(sender);

            var json = order.GetJson();
            var jsontring = json.ToJson();
            return Http.Post($"{_host}/matcher/orderbook", json);
        }

        public Dictionary<Asset, decimal> GetTradableBalance(string address, Asset amountAsset, Asset priceAsset)
        {
            var url = $"{_host}/matcher/orderbook/{amountAsset.Id}/{priceAsset.Id}/tradableBalance/{address}";

            var response = Http.GetObject(url);

            return new Dictionary<Asset, decimal>
            {
                {amountAsset, amountAsset.LongToAmount(response.GetLong(amountAsset.Id))},
                {priceAsset, priceAsset.LongToAmount(response.GetLong(priceAsset.Id))},
            };
        }
        
        public OrderBook GetOrderBook(Asset amountAsset, Asset priceAsset)
        {
            string path = $"{_host}/matcher/orderbook/{amountAsset.Id}/{priceAsset.Id}";
            var json = Http.GetObject(path);
            return OrderBook.CreateFromJson(json, amountAsset, priceAsset);
        }

        public Order[] GetOrders(PrivateKeyAccount account, Asset amountAsset, Asset priceAsset)
        {
            string path = $"{_host}/matcher/orderbook/{amountAsset.Id}/{priceAsset.Id}/publicKey/{account.PublicKey.ToBase58()}";

            var headers = GetProtectionHeaders(account);
            var response = Http.GetObjectsWithHeaders(path, headers);
            
            return response.Select(j => Order.CreateFromJson(j, amountAsset, priceAsset)).ToArray();
        }
        
        public string CancelOrder(PrivateKeyAccount account,
            Asset amountAsset, Asset priceAsset, string orderId)
        {
            var request = MakeOrderCancelRequest(account, orderId);            
            var url = $"{_host}/matcher/orderbook/{amountAsset.Id}/{priceAsset.Id}/cancel";
            return Http.Post(url, request);
        }

        public string CancelAll(PrivateKeyAccount account)
        {
            var request = MakeCancelAllRequest(account);
            var url = $"{_host}/matcher/orderbook/cancel";
            return Http.Post(url, request);
        }

        public string DeleteOrder(PrivateKeyAccount account,
            Asset amountAsset, Asset priceAsset, string orderId)
        {
            var request = MakeOrderCancelRequest(account, orderId);            
            var url = $"{_host}/matcher/orderbook/{amountAsset.Id}/{priceAsset.Id}/delete";
            return Http.Post(url, request);
        }

        private static NameValueCollection GetProtectionHeaders(PrivateKeyAccount account)
        {
            long timestamp = Utils.CurrentTimestamp();
            var stream = new MemoryStream(40);
            var writer = new BinaryWriter(stream);
            writer.Write(account.PublicKey);
            writer.WriteLong(timestamp);
            var signature = account.Sign(stream);
            return new NameValueCollection
            {
                {"Timestamp", Convert.ToString(timestamp) },
                {"Signature", signature.ToBase58() }
            };
        }

        public static DictionaryObject MakeOrderCancelRequest(PrivateKeyAccount sender, string orderId)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(sender.PublicKey);
            writer.Write(Base58.Decode(orderId));
            var signature = sender.Sign(stream);
            return new DictionaryObject
            {
                {"sender", sender.PublicKey.ToBase58()},
                {"orderId", orderId},
                {"signature", signature.ToBase58()}
            };
        }

        public static DictionaryObject MakeCancelAllRequest(PrivateKeyAccount sender)
        {
            long timestamp = Utils.CurrentTimestamp();
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(sender.PublicKey);
            writer.WriteLong(timestamp);
            var signature = sender.Sign(stream);
            return new DictionaryObject
            {
                {"sender", sender.PublicKey.ToBase58()},
                {"timestamp", timestamp},
                {"signature", signature.ToBase58()}
            };
        }
    }
}