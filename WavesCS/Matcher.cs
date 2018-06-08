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
            return Api.GetString(_host + "/matcher");            
        }

        public Matcher(string host = "https://matcher.wavesnodes.com", string matcherKey = null)
        {
            _host = host;
            MatcherKey = matcherKey ?? GetMatcherKey();
        }

        public string PlaceOrder(PrivateKeyAccount sender, OrderSide side,
            Asset amountAsset, Asset priceAsset, decimal price, decimal amount, DateTime expiration)
        {
            var order = MakeOrder(sender, MatcherKey, side, amountAsset, priceAsset, price, amount, expiration, 0.003m);
            
            return Api.Post($"{_host}/matcher/orderbook", order);            
        }               
        
        public Dictionary<Asset, decimal> GetTradableBalance(string address, Asset amountAsset, Asset priceAsset)
        {
            var url = $"{_host}/matcher/orderbook/{amountAsset.Id}/{priceAsset.Id}/tradableBalance/{address}";

            var response = Api.GetObject(url);

            return new Dictionary<Asset, decimal>
            {
                {amountAsset, amountAsset.LongToAmount(response.GetLong(amountAsset.Id))},
                {priceAsset, priceAsset.LongToAmount(response.GetLong(priceAsset.Id))},
            };
        }
        
        public OrderBook GetOrderBook(Asset amountAsset, Asset priceAsset)
        {
            string path = $"{_host}/matcher/orderbook/{amountAsset.Id}/{priceAsset.Id}";
            var json = Api.GetObject(path);
            return OrderBook.CreateFromJson(json, amountAsset, priceAsset);
        }

        public Order[] GetOrders(PrivateKeyAccount account, Asset amountAsset, Asset priceAsset)
        {
                    
            string path = $"{_host}/matcher/orderbook/{amountAsset.Id}/{priceAsset.Id}/publicKey/{account.PublicKey.ToBase58()}";

            var headers = GetProtectionHeaders(account);
            var response = Api.GetObjectsWithHeaders(path, headers);
            
            return response.Select(j => Order.CreateFromJson(j, amountAsset, priceAsset)).ToArray();
        }
        
        public string CancelOrder(PrivateKeyAccount account,
            Asset amountAsset, Asset priceAsset, string orderId)
        {
            var request = MakeOrderCancelRequest(account, orderId);            
            var url = $"{_host}/matcher/orderbook/{amountAsset.Id}/{priceAsset.Id}/cancel";
            return Api.Post(url, request);
        }
        
        public string DeleteOrder(PrivateKeyAccount account,
            Asset amountAsset, Asset priceAsset, string orderId)
        {
            var request = MakeOrderCancelRequest(account, orderId);            
            var url = $"{_host}/matcher/orderbook/{amountAsset.Id}/{priceAsset.Id}/delete";
            return Api.Post(url, request);
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
        
        public static DictionaryObject MakeOrder(PrivateKeyAccount sender, string matcherKey, OrderSide side,
            Asset amountAsset, Asset priceAsset, decimal price, decimal amount, DateTime expiration, decimal matcherFee)
        {
            long timestamp = Utils.CurrentTimestamp();

            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(sender.PublicKey);
            writer.Write(Base58.Decode(matcherKey));
            writer.WriteAsset(amountAsset.Id);
            writer.WriteAsset(priceAsset.Id);
            writer.Write((byte)(side == OrderSide.Buy ? 0x0 : 0x1)); 
            writer.WriteLong(Asset.PriceToLong(amountAsset, priceAsset, price));
            writer.WriteLong(amountAsset.AmountToLong(amount));
            writer.WriteLong(timestamp);
            writer.WriteLong(expiration.ToLong() );
            writer.WriteLong(Assets.WAVES.AmountToLong(matcherFee));
            var signature = sender.Sign(stream);

            return new DictionaryObject {
                { "senderPublicKey", Base58.Encode(sender.PublicKey) },
                { "matcherPublicKey", matcherKey },
                { "assetPair", new DictionaryObject {
                    {"amountAsset", amountAsset.IdOrNull },
                    {"priceAsset", priceAsset.IdOrNull}}
                },
                { "orderType", side.ToString().ToLower() },
                { "price", Asset.PriceToLong(amountAsset, priceAsset, price) },
                { "amount", amountAsset.AmountToLong(amount) },
                { "timestamp", timestamp },
                { "expiration", expiration.ToLong() },
                { "matcherFee", Assets.WAVES.AmountToLong(matcherFee) },
                { "signature", signature.ToBase58() }                
            };
        }
    }
}