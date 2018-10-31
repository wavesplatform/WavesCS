using System.Collections.Generic;
using System.IO;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public class SetAssetScriptTransaction : Transaction
    {
        public Asset Asset;
        public byte[] Script { get; }
        public char ChainId { get; }

        public override byte Version { get; set; } = 1;

        public SetAssetScriptTransaction(byte[] senderPublicKey, Asset asset, byte[] script, char chainId, decimal fee = 1m) : base(senderPublicKey)
        {
            Asset = asset;
            Script = script;
            ChainId = chainId;
            Fee = fee;
        }

        public SetAssetScriptTransaction(DictionaryObject tx) : base(tx)
        {
            Script = tx.GetString("script").FromBase64();
            Fee = Assets.WAVES.LongToAmount(tx.GetLong("fee"));
            ChainId = tx.GetChar("chainId");
            Asset = Assets.GetById(tx.GetString("assetId"));
        }

        public override byte[] GetBody()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(TransactionType.SetAssetScript);
                writer.Write(Version);
                writer.Write(ChainId);
                writer.Write(SenderPublicKey);

                writer.Write(Asset.Id.FromBase58());

                writer.WriteLong(Assets.WAVES.AmountToLong(Fee));
                writer.WriteLong(Timestamp.ToLong());

                if (Script == null)
                {
                    writer.Write((byte)0);
                }
                else
                {
                    writer.Write((byte)1);
                    writer.WriteShort((short)Script.Length);
                    writer.Write(Script);
                }

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
                {"type", TransactionType.SetAssetScript},
                {"assetId", Asset.Id},
                {"version", Version},
                {"senderPublicKey", SenderPublicKey.ToBase58()},
                {"script", Script?.ToBase64()},
                {"fee", Assets.WAVES.AmountToLong(Fee)},
                {"timestamp", Timestamp.ToLong()}
            };
        }

        protected override bool SupportsProofs()
        {
            return true;
        }
    }
}