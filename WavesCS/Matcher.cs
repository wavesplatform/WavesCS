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
        private const long Fee = 300000L;       

        private string GetMatcherKey()
        {
            return Api.GetString(_host + "/matcher");            
        }

        public Matcher(string host = "https://matcher.wavesnodes.com", string matcherKey = null)
        {
            _host = host;
            MatcherKey = matcherKey ?? GetMatcherKey();
        }

        public string PlaceOrder(PrivateKeyAccount sender, OrderSide side,
            string amountAssetId, string priceAssetId, long price, long amount, DateTime expiration)
        {
            var order = MakeOrder(sender, MatcherKey, side, amountAssetId, priceAssetId, price, amount, expiration, Fee);
            
            return Api.Post($"{_host}/matcher/orderbook", order);            
        }               
        
        public Dictionary<string, long> GetTradableBalance(string address, string amountAsset, string priceAsset)
        {
            var url = $"{_host}/matcher/orderbook/{NormalizeAsset(amountAsset)}/{NormalizeAsset(priceAsset)}/tradableBalance/{address}";

            var response = Api.GetObject(url);
            
            return response.ToDictionary(pair => pair.Key, pair => Convert.ToInt64(pair.Value));                       
        }
        
        public OrderBook GetOrderBook(string amountAsset, string priceAsset)
        {
            string path = $"{_host}/matcher/orderbook/{NormalizeAsset(amountAsset)}/{NormalizeAsset(priceAsset)}";
            var json = Api.GetObject(path);
            return OrderBook.CreateFromJson(json);
        }

        public Order[] GetOrders(PrivateKeyAccount account, string amountAsset, string priceAsset)
        {
                    
            string path = $"{_host}/matcher/orderbook/{NormalizeAsset(amountAsset)}/{NormalizeAsset(priceAsset)}/publicKey/{account.PublicKey.ToBase58()}";

            var headers = GetProtectionHeaders(account);
            var response = Api.GetObjectsWithHeaders(path, headers);
            
            return response.Select(Order.CreateFromJson).ToArray();
        }
        
        public string CancelOrder(PrivateKeyAccount account,
            string amountAssetId, string priceAssetId, string orderId)
        {
            var request = MakeOrderCancelRequest(account, orderId);            
            var url = $"{_host}/matcher/orderbook/{NormalizeAsset(amountAssetId)}/{NormalizeAsset(priceAssetId)}/cancel";
            return Api.Post(url, request);
        }
        
        public string DeleteOrder(PrivateKeyAccount account,
            string amountAssetId, string priceAssetId, string orderId)
        {
            var request = MakeOrderCancelRequest(account, orderId);            
            var url = $"{_host}/matcher/orderbook/{NormalizeAsset(amountAssetId)}/{NormalizeAsset(priceAssetId)}/delete";
            return Api.Post(url, request);
        }
        
//        public Dictionary<string, object> GetOrders(PrivateKeyAccount account)
//        {            
//            var url = "matcher/orderBook/" + Base58.Encode(account.PublicKey);
//            return Api.GetObjectWithHeaders(url, GetProtectionHeaders(account));               
//        }
//
//        public string GetOrderStatus(string orderId, string asset1, string asset2)
//        {
//            asset1 = NormalizeAsset(asset1);
//            asset2 = NormalizeAsset(asset2);
//            string path = $"matcher/orderbook/{asset1}/{asset2}/{orderId}";
//            dynamic result = Get<string>(path, "status");
//            return result;
//        }
        
        public static string NormalizeAsset(string assetId)
        {
            return string.IsNullOrEmpty(assetId) ? "WAVES" : assetId;
        }
        
        private NameValueCollection GetProtectionHeaders(PrivateKeyAccount account)
        {
            long timestamp = Utils.CurrentTimestamp();
            var stream = new MemoryStream(40);
            var writer = new BinaryWriter(stream);
            writer.Write(account.PublicKey);
            writer.WriteLong(timestamp);
            string signature = account.Sign(stream);
            return new NameValueCollection
            {
                {"Timestamp", Convert.ToString(timestamp)},
                {"Signature", signature}
            };
        }
        
        public static DictionaryObject MakeOrderCancelRequest(PrivateKeyAccount sender, string orderId)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(sender.PublicKey);
            writer.Write(Base58.Decode(orderId));
            string signature = sender.Sign(stream);
            return new DictionaryObject
            {
                {"sender", sender.PublicKey.ToBase58()},
                {"orderId", orderId},
                {"signature", signature}
            };
        }
        
        public static DictionaryObject MakeOrder(PrivateKeyAccount sender, string matcherKey, OrderSide side,
            string amountAssetId, string priceAssetId, long price, long amount, DateTime expiration, long matcherFee)
        {
            long timestamp = Utils.CurrentTimestamp();

            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(sender.PublicKey);
            writer.Write(Base58.Decode(matcherKey));
            writer.WriteAsset(amountAssetId);
            writer.WriteAsset(priceAssetId);
            writer.Write((byte)(side == OrderSide.Buy ? 0x0 : 0x1)); 
            writer.WriteLong(price);
            writer.WriteLong(amount);
            writer.WriteLong(timestamp);
            writer.WriteLong(expiration.DateToTimestamp() );
            writer.WriteLong(matcherFee);
            string signature = sender.Sign(stream);

            return new DictionaryObject {
                { "senderPublicKey", Base58.Encode(sender.PublicKey) },
                { "matcherPublicKey", matcherKey },
                { "assetPair", new DictionaryObject {
                    {"amountAsset", amountAssetId},
                    {"priceAsset", priceAssetId }}
                },
                { "orderType", side.ToString().ToLower() },
                { "price", price },
                { "amount", amount },
                { "timestamp", timestamp },
                { "expiration", expiration.DateToTimestamp() },
                { "matcherFee", matcherFee },
                { "signature", signature }                
            };
        }
    }
}