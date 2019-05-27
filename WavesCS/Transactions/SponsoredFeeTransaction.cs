using System;
using System.IO;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public class SponsoredFeeTransaction : Transaction
    {
        public decimal MinimalFeeInAssets { get; }
        public Asset Asset { get; }

        public override byte Version { get; set; } = 1;

        public SponsoredFeeTransaction(char chainId, byte[] senderPublicKey, Asset asset, decimal minimalFeeInAssets, decimal fee = 1m) :
            base(chainId, senderPublicKey)
        {
            Fee = fee;
            Asset = asset;
            MinimalFeeInAssets = minimalFeeInAssets;
        }

        public SponsoredFeeTransaction(DictionaryObject tx) : base(tx)
        {
            var node = new Node(tx.GetChar("chainId"));
            Asset = node.GetAsset(tx.GetString("assetId"));
            Fee = Assets.WAVES.LongToAmount(tx.GetLong("fee"));
            MinimalFeeInAssets = tx["minSponsoredAssetFee"] != null ? Asset.LongToAmount(tx.GetLong("minSponsoredAssetFee")) : 0;
        }

        public override byte[] GetBody()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(TransactionType.SponsoredFee);
                writer.Write(Version);
                writer.Write(SenderPublicKey);
                writer.Write(Asset.Id.FromBase58());
                writer.WriteLong(Asset.AmountToLong(MinimalFeeInAssets));
                writer.WriteLong(Assets.WAVES.AmountToLong(Fee));
                writer.WriteLong(Timestamp.ToLong());

                return stream.ToArray();
            }
        }

        public override byte[] GetBytes()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.WriteByte(0);
            writer.WriteByte((byte)TransactionType.SponsoredFee);
            writer.WriteByte(Version);
            writer.Write(GetBody());
            writer.Write(GetProofsBytes());

            return stream.ToArray();
        }

        protected override bool SupportsProofs()
        {
            return true;
        }

        public override DictionaryObject GetJson()
        {
            var result = new DictionaryObject
            {
                {"type", (byte) TransactionType.SponsoredFee},
                {"version", Version},
                {"senderPublicKey", Base58.Encode(SenderPublicKey)},

                {"assetId", Asset.IdOrNull},
                {"fee", Assets.WAVES.AmountToLong(Fee)},
                {"timestamp", Timestamp.ToLong()},
                {"minSponsoredAssetFee", Asset.AmountToLong(MinimalFeeInAssets)}
            };

            if (Sender != null)
                result.Add("sender", Sender);

            return result;
        }
    }
}
