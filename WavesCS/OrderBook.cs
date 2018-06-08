using System;
using System.Collections.Generic;
using System.Linq;

namespace WavesCS
{
    public class OrderBook
    {
        public DateTime Timestamp { get; }
        public Asset AmountAsset { get; }
        public Asset PriceAsset { get; }
        public Item[] Bids { get; }
        public Item[] Asks { get; }

        public class Item
        {
            public decimal Price { get; }
            public decimal Amount { get; }

            public Item(decimal price, decimal amount)
            {
                Price = price;
                Amount = amount;
            }
        }
         
        public OrderBook(DateTime timestamp, Asset amountAsset, Asset priceAsset, Item[] bids, Item[] asks)
        {
            Timestamp = timestamp;
            AmountAsset = amountAsset;
            PriceAsset = priceAsset;
            Bids = bids;
            Asks = asks;
        }

        public static OrderBook CreateFromJson(Dictionary<string, object> json, Asset amountAsset, Asset priceAsset)
        {
            return new OrderBook(
                json.GetDate("timestamp"),
                amountAsset,
                priceAsset,
                json.GetObjects("bids").Select(o => ParseItem(amountAsset, priceAsset, o)).ToArray(),
                json.GetObjects("asks").Select(o => ParseItem(amountAsset, priceAsset, o)).ToArray());
        }

        private static Item ParseItem(Asset amountAsset, Asset priceAsset, Dictionary<string, object> o)
        {
            return new Item(
                Asset.LongToPrice(amountAsset, priceAsset, o.GetLong("price")), 
                amountAsset.LongToAmount(o.GetLong("amount")));
        }
    }
}
