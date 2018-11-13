using System.IO;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public class BurnTransaction : Transaction
    {
        public Asset Asset { get; }
        public decimal Quantity { get; }

        public BurnTransaction(byte[] senderPublicKey, Asset asset, decimal quantity, decimal fee = 0.001m) : base(senderPublicKey)
        {
            Asset = asset;
            Quantity = quantity;
            Fee = fee;
        }

        public BurnTransaction(DictionaryObject tx) : base(tx)
        {
            Asset = Assets.GetById(tx.GetString("assetId"));
            Quantity = Asset.LongToAmount(tx.GetLong("amount"));
            Fee = Assets.WAVES.LongToAmount(tx.GetLong("fee"));
        }

        public override byte[] GetBody()
        {
            using(var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(TransactionType.Burn);
                writer.Write(SenderPublicKey);
                writer.Write(Asset.Id.FromBase58());
                writer.WriteLong(Asset.AmountToLong(Quantity));
                writer.WriteLong(Assets.WAVES.AmountToLong(Fee));
                writer.WriteLong(Timestamp.ToLong());
                return stream.ToArray();
            }
        }

        public override byte[] GetIdBytes()
        {
            return GetBody();
        }

        public override DictionaryObject GetJson()
        {
            return new DictionaryObject
            {
                {"type", (byte) TransactionType.Burn},
                {"senderPublicKey", SenderPublicKey.ToBase58()},
                {"sender", Sender},
                {"assetId", Asset.Id},
                {"amount", Asset.AmountToLong(Quantity)},
                {"fee", Assets.WAVES.AmountToLong(Fee)},
                {"timestamp", Timestamp.ToLong()}
            };
        }

        protected override bool SupportsProofs()
        {
            return false;
        }
    }
}