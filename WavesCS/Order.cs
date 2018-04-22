using System;
using System.Collections.Generic;

namespace WavesCS
{

    public enum OrderStatus
    {
        Accepted,
        Filled,
        PartiallyFilled,
        Cancelled,
        NotFound
    }
    
    public class Order
    {
        public string Id { get; }
        public OrderSide Side { get; }
        public long Amount { get; }
        public long Price { get; }
        public DateTime Timestamp { get; }
        public long Filled { get; }
        public OrderStatus Status { get; }
        public string AmountAsset { get; }
        public string PriceAsset { get; }

        public Order(
            string id, OrderSide side, long amount, long price, DateTime timestamp, long filled, OrderStatus status,
            string amountAsset, string priceAsset)
        {
            Id = id;
            Side = side;
            Amount = amount;
            Price = price;
            Timestamp = timestamp;
            Filled = filled;
            Status = status;
            AmountAsset = amountAsset;
            PriceAsset = priceAsset;
        }

        public static Order CreateFromJson(Dictionary<string, object> json)
        {
            return new Order(
                json.GetString("id"),
                (OrderSide) Enum.Parse(typeof(OrderSide), json.GetString("type"), true),
                json.GetLong("amount"),
                json.GetLong("price"),
                json.GetDate("timestamp"),
                json.GetLong("filled"),
                (OrderStatus) Enum.Parse(typeof(OrderStatus), json.GetString("status")),
                json.GetString("assetPair.amountAsset"),
                json.GetString("assetPair.priceAsset"));
        }
    }
}
