namespace WavesCS
{

    public class Asset
    {
        public string Id { get; }
        public string Name { get; }
        public byte Decimals { get; }

        public string IdOrNull => Id == "WAVES" ? null : Id;
        
        private readonly decimal _scale;

        public Asset(string id, string name, byte decimals)
        {
            Id = id;
            Name = name;
            Decimals = decimals;                        
            _scale = new decimal(1, 0, 0, false, decimals);
        }

        public long AmountToLong(decimal amount)
        {            
            return decimal.ToInt64(amount / _scale);
        }
        
        public decimal LongToAmount(long value)
        {            
            return value * _scale;
        }

        public static long AmountToLong(byte digits, decimal amount)
        {
            var scale = new decimal(1, 0, 0, false, digits);
            return decimal.ToInt64(amount / scale);
        }

        public static long PriceToLong(Asset amountAsset, Asset priceAsset, decimal price)
        {
            var decimals =  8 - amountAsset.Decimals + priceAsset.Decimals;
            var scale = new decimal(1, 0, 0, false, (byte) decimals);
            return decimal.ToInt64(price / scale);
        }
        
        public static decimal LongToPrice(Asset amountAsset, Asset priceAsset, long price)
        {
            var decimals =  8 - amountAsset.Decimals + priceAsset.Decimals;
            var scale = new decimal(1, 0, 0, false, (byte) decimals);
            return price * scale;
        }
    }

    public static class Assets
    {
        public static readonly Asset WAVES = new Asset("WAVES", "WAVES", 8);
        public static readonly Asset BTC = new Asset("8LQW8f7P5d5PZM7GtZEBgaqRPGSzS3DfPuiXrURJ4AJS", "BTC", 8);
        public static readonly Asset BCH = new Asset("zMFqXuoyrn5w17PFurTqxB7GsS71fp9dfk6XFwxbPCy", "BCH", 8);
        public static readonly Asset MRT = new Asset("4uK8i4ThRGbehENwa6MxyLtxAjAo1Rj9fduborGExarC", "MRT", 2);
        public static readonly Asset ETH = new Asset("474jTeYx2r2Va35794tCScAXWJG9hU2HcgxzMowaZUnu", "ETH", 8);
        public static readonly Asset LTC = new Asset("HZk1mbfuJpmxU1Fs4AX5MWLVYtctsNcg6e2C6VKqK8zk", "LTC", 8);
        public static readonly Asset ZEC = new Asset("BrjUWjndUanm5VsJkbUip8VRYy6LWJePtxya3FNv4TQa", "ZEC", 8);
        public static readonly Asset USD = new Asset("Ft8X1v1LTa1ABafufpaCWyVj8KkaxUWE6xBhW6sNFJck", "USD", 2);
        public static readonly Asset EUR = new Asset("Gtb1WRznfchDnTh37ezoDTJ4wcoKaRsKqKjJjy7nm2zU", "EUR", 2);
        public static readonly Asset DASH = new Asset("B3uGHFRpSUuGEDWjqB9LWWxafQj8VTvpMucEyoxzws5H", "DASH", 8);
        public static readonly Asset MONERO = new Asset("5WvPKSJXzVE2orvbkJ8wsQmmQKqTv9sGBPksV4adViw3", "XMR", 8);

        public static Asset GetById(string assetId, Node node)
        {
            return node.GetAsset(assetId);
        }

        public static Asset GetById(string assetId)
        {
            return GetById(assetId, Node.DefaultNode);
        }
    }
}