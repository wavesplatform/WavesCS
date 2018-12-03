using System.IO;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public class SponsoredFeeTransaction : Transaction
    {
        public decimal MinimalFeeInAssets { get; }
        public Asset Asset { get; }

        public override byte Version { get; set; } = 1;

        public SponsoredFeeTransaction(byte[] senderPublicKey, Asset asset, decimal minimalFeeInAssets, decimal fee = 1m) :
            base(senderPublicKey)
        {
            Fee = fee;
            Asset = asset;
            MinimalFeeInAssets = minimalFeeInAssets;
        }

        public SponsoredFeeTransaction(DictionaryObject tx) : base(tx)
        {
            Asset = Assets.GetById(tx.GetString("assetId"));
            Fee = Assets.WAVES.LongToAmount(tx.GetLong("fee"));
            MinimalFeeInAssets = Asset.LongToAmount(tx.GetLong("minSponsoredAssetFee"));
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

        public override byte[] GetIdBytes()
        {
            return GetBody();
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
