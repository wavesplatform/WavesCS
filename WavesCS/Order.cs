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
        public decimal Amount { get; }
        public decimal Price { get; }
        public DateTime Timestamp { get; }
        public decimal Filled { get; }
        public OrderStatus Status { get; }
        public Asset AmountAsset { get; }
        public Asset PriceAsset { get; }

        public Order(
            string id, OrderSide side, decimal amount, decimal price, DateTime timestamp, decimal filled, OrderStatus status,
            Asset amountAsset, Asset priceAsset)
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

        public static Order CreateFromJson(Dictionary<string, object> json, Asset amountAsset, Asset priceAsset)
        {
            return new Order(
                json.GetString("id"),
                (OrderSide) Enum.Parse(typeof(OrderSide), json.GetString("type"), true),
                amountAsset.LongToAmount(json.GetLong("amount")),
                Asset.LongToPrice(amountAsset, priceAsset, json.GetLong("price")),
                json.GetDate("timestamp"),
                amountAsset.LongToAmount(json.GetLong("filled")),
                (OrderStatus) Enum.Parse(typeof(OrderStatus), json.GetString("status")),
                amountAsset,
                priceAsset);
        }
    }
}
