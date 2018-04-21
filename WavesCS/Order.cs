namespace WavesCS
{
    public class Order
    {
        public class OrderType
        {
            public readonly string Json;

            public OrderType(string json)
            {
                Json = json;
            }

            public int Ordinal => Json == "buy" ? 0 : 1;
        }
        public readonly long Price;
        public readonly long Amount;
        public readonly OrderType Type;

        public Order(long price, long amount)
        {
            Price = price;
            Amount = amount;
        }

        public Order(string type)
        {
            if(type == "sell" || type == "buy")
            {
                this.Type = new OrderType(type);
            }
        }

        public override string ToString()
        {
            return $"Order[price={Price}, amount={Amount}]";
        }
    }
}
