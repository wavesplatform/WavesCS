using System;

namespace WavesCS
{
    public class Order
    {
        public class Type
        {
            public readonly string json;

            public Type(string json)
            {
                this.json = json;
            }

            public int Ordinal
            {
                get { return json == "buy" ? 0 : 1; }
            }
        }
        public readonly long price;
        public readonly long amount;
        public readonly Type type;

        public Order(long price, long amount)
        {
            this.price = price;
            this.amount = amount;
        }

        public Order(string type)
        {
            if(type == "sell" || type == "buy")
            {
                this.type = new Type(type);
            }
        }

        public override string ToString()
        {
            return String.Format("Order[price={0}, amount={1}]", price, amount);
        }
    }
}
