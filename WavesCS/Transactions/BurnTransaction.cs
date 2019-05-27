using System;
using System.IO;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public class BurnTransaction : Transaction
    {
        public Asset Asset { get; }
        public decimal Quantity { get; }
        public override byte Version { get; set; } = 2;

        public BurnTransaction(char chainId, byte[] senderPublicKey, Asset asset, decimal quantity, decimal fee = 0.001m) : base(chainId, senderPublicKey)
        {
            Asset = asset;
            Quantity = quantity;
            Fee = fee;
        }

        public BurnTransaction(DictionaryObject tx) : base(tx)
        {
            var node = new Node(tx.GetChar("chainId"));
            Asset = node.GetAsset(tx.GetString("assetId"));
            Quantity = Asset.LongToAmount(tx.GetLong("amount"));
            Fee = Assets.WAVES.LongToAmount(tx.GetLong("fee"));
        }

        public override byte[] GetBody()
        {
            using(var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(TransactionType.Burn);

                if (Version > 1)
                {
                    writer.WriteByte(Version);
                    writer.WriteByte((byte)ChainId);
                }

                writer.Write(SenderPublicKey);
                writer.Write(Asset.Id.FromBase58());
                writer.WriteLong(Asset.AmountToLong(Quantity));
                writer.WriteLong(Assets.WAVES.AmountToLong(Fee));
                writer.WriteLong(Timestamp.ToLong());
                return stream.ToArray();
            }
        }

        public override byte[] GetBytes()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            if (Version == 1)
            {
                writer.Write(GetBody());
                writer.Write(GetProofsBytes());
            }
            else
            {
                writer.WriteByte(0);
                writer.Write(GetBody());
                writer.Write(GetProofsBytes());
            }

            return stream.ToArray();
        }

        public override DictionaryObject GetJson()
        {
            var result = new DictionaryObject
            {
                {"type", (byte) TransactionType.Burn},
                {"senderPublicKey", SenderPublicKey.ToBase58()},
                {"assetId", Asset.Id},
                {"amount", Asset.AmountToLong(Quantity)},
                {"fee", Assets.WAVES.AmountToLong(Fee)},
                {"timestamp", Timestamp.ToLong()}
            };

            if (Sender != null)
                result.Add("sender", Sender);

            return result;
        }

        protected override bool SupportsProofs()
        {
            return false;
        }
    }
}