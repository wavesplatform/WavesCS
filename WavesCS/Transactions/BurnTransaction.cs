using System;
using System.Collections.Generic;
using System.IO;

namespace WavesCS
{
    public class BurnTransaction : Transaction
    {
        public Asset Asset { get; }
        public decimal Quantity { get; }
        public decimal Fee { get; }

        public BurnTransaction(byte[] senderPublicKey, DateTime timestamp, Asset asset, decimal quantity, decimal fee = 0.001m) : base(senderPublicKey, timestamp)
        {
            Asset = asset;
            Quantity = quantity;
            Fee = fee;
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

        public override Dictionary<string, object> GetJson()
        {
            return new Dictionary<string, object>
            {
                {"type", TransactionType.Burn},
                {"senderPublicKey", SenderPublicKey.ToBase58()},
                {"assetId", Asset.Id},
                {"quantity", Asset.AmountToLong(Quantity)},
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