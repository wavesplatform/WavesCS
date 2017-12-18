using System.Collections.Generic;
using System.Linq;

namespace WavesCS
{
    public class OrderBook
    {
        public List<Order> bids, asks;

        public const string BasePath = "matcher/orderbook/";

        public List<Order> Bids
        {
            get { return bids; }
        }

        public List<Order> Asks
        {
            get { return asks; }
        }

        public OrderBook(List<Order> bids, List<Order> asks)
        {
            this.bids = bids;
            this.asks = asks;
        }

        public OrderBook(JsonOrderBook jsonOrderBook)
        {
            this.bids = jsonOrderBook.Bids.Select(x => new Order(x.Price, x.Amount)).ToList();
            this.asks = jsonOrderBook.Asks.Select(x => new Order(x.Price, x.Amount)).ToList();
        }

        public class Assets
        {
            public string AmountAsset { get; set; }
            public string PriceAsset { get; set; }
        }

        public class Ask
        {
            public int Price { get; set; }
            public int Amount { get; set; }
        }

        public class Bid
        {
            public int Price { get; set; }
            public int Amount { get; set; }
        }

        public class JsonOrderBook
        {
            public long Timestamp { get; set; }
            public Assets Pair { get; set; }
            public List<Bid> Bids { get; set; }
            public List<Ask> Asks { get; set; }
        }        
    }
}
