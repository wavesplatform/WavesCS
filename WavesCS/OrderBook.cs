using System;
using System.Collections.Generic;
using System.Linq;

namespace WavesCS
{
    public class OrderBook
    {
        public DateTime Timestamp { get; }
        public string AmountAsset { get; }
        public string PriceAsset { get; }
        public Item[] Bids { get; }
        public Item[] Asks { get; }

        public class Item
        {
            public long Price { get; }
            public long Amount { get; }

            public Item(long price, long amount)
            {
                Price = price;
                Amount = amount;
            }
        }
         
        public OrderBook(DateTime timestamp, string amountAsset, string priceAsset, Item[] bids, Item[] asks)
        {
            Timestamp = timestamp;
            AmountAsset = amountAsset;
            PriceAsset = priceAsset;
            Bids = bids;
            Asks = asks;
        }

        public static OrderBook CreateFromJson(Dictionary<string, object> json)
        {
            return new OrderBook(
                json.GetDate("timestamp"),
                json.GetString("pair.amountAsset"),
                json.GetString("pair.priceAsset"),
                json.GetObjects("bids").Select(o => new Item(o.GetLong("price"), o.GetLong("amount"))).ToArray(),
                json.GetObjects("asks").Select(o => new Item(o.GetLong("price"), o.GetLong("amount"))).ToArray());
        }
    }
}
