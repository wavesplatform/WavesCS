using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WavesCS
{
    public class IssueTransaction : Transaction
    {
        public string Name { get; }
        public string Description { get; }
        public decimal Quantity { get; }
        public byte Decimals { get; }
        public bool Reissuable { get; }
        public Asset Asset { get; }

        public override byte Version { get; set; } = 2;

        public char ChainId { get; }
        public byte[] Script { get; }

        public IssueTransaction(byte[] senderPublicKey,
            string name, string description, decimal quantity, byte decimals, bool reissuable, decimal fee = 1m, byte[] script = null, char chainId = 'T') : base(senderPublicKey)
        {
            Name = name ?? "";
            Description = description ?? "";
            Quantity = quantity;
            Decimals = decimals;
            Reissuable = reissuable;
            Fee = fee;
            Asset = new Asset("", "", Decimals);
            Script = script;
            ChainId = chainId;
        }

        public IssueTransaction(Dictionary<string, object> tx): base(tx)
        {
            Name = tx.GetString("name");
            Description = tx.GetString("description");
            Decimals = (byte)tx.GetInt("decimals");
            Quantity = Assets.WAVES.LongToAmount(tx.GetLong("quantity"));
            Reissuable = tx.GetBool("reissuable");
            Fee = Assets.WAVES.LongToAmount(tx.GetLong("fee"));
            Asset = Assets.GetById(tx.GetString("assetId"));
        }

        public void WriteType(BinaryWriter writer)
        {
            writer.Write(TransactionType.Transfer);
        }

        public void WriteVersion(BinaryWriter writer)
        {
            if (Version > 1)
                writer.Write(Version);
        }


        public void WriteBytes(BinaryWriter writer)
        {
            var asset = new Asset("", "", Decimals);

            writer.Write(SenderPublicKey);
            writer.WriteShort(Name.Length);
            writer.Write(Encoding.ASCII.GetBytes(Name));
            writer.WriteShort(Description.Length);
            writer.Write(Encoding.ASCII.GetBytes(Description));
            writer.WriteLong(asset.AmountToLong(Quantity));
            writer.Write(Decimals);
            writer.Write((byte)(Reissuable ? 1 : 0));
            writer.WriteLong(Assets.WAVES.AmountToLong(Fee));
            writer.WriteLong(Timestamp.ToLong());
        }


        public override byte[] GetBody()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write(TransactionType.Issue);

            if (Version > 1) {
                writer.Write(Version);
                writer.Write((byte)ChainId);
            }

            WriteBytes(writer);

            if (Version > 1)
            {
                if (Script == null)
                    writer.Write((byte)0);
                else
                {
                    writer.Write((byte)1);
                    writer.WriteShort(Script.Length);
                    writer.Write(Script);
                }
            }

            return stream.ToArray();
        }

        public byte[] GetIdBytes()
        {
            var asset = new Asset("", "", Decimals);
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write(TransactionType.Issue);
            WriteBytes(writer);

            return stream.ToArray();
        }


        public override Dictionary<string, object> GetJson()
        {
            var result = new Dictionary<string, object>
            {
                {"type", TransactionType.Issue},
                {"senderPublicKey", Base58.Encode(SenderPublicKey)},
                {"name", Name},
                {"description", Description},
                {"quantity", Asset.AmountToLong(Quantity)},
                {"decimals", Decimals},
                {"reissuable", Reissuable},
                {"fee", Assets.WAVES.AmountToLong(Fee)},
                {"timestamp", Timestamp.ToLong()},
                {"script", Script?.ToBase64()}
            };
            if (Version > 1)
                result.Add("version", Version);
            return result;
        }

        protected override bool SupportsProofs()
        {
            return Version > 1;
        }        
    }
}