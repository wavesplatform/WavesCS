using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WavesCS
{
    public class AliasTransaction : Transaction
    {
        public string Alias { get; }
        public char Scheme { get; }
 
        public AliasTransaction(byte[] senderPublicKey, string alias, char scheme, decimal fee = 0.001m) : 
            base(senderPublicKey)
        {
            Alias = alias;
            Scheme = scheme;
            Fee = fee;
        }

        public AliasTransaction(Dictionary<string, object> tx) : base(tx)
        {
            Alias = tx.GetString("alias");
            Fee = Assets.WAVES.LongToAmount(tx.GetLong("fee"));
            Scheme = 'W';
        }

        public override byte[] GetBody()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(TransactionType.Alias);
                writer.Write(SenderPublicKey);
                writer.WriteShort(Alias.Length + 4);
                writer.Write((byte) 0x02);
                writer.Write((byte) Scheme);
                writer.WriteShort(Alias.Length);
                writer.Write(Encoding.ASCII.GetBytes(Alias));
                writer.WriteLong(Assets.WAVES.AmountToLong(Fee));
                writer.WriteLong(Timestamp.ToLong());
                return stream.ToArray();
            }
        }

        public byte[] GetIdBytes()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(TransactionType.Alias);
                writer.Write((byte)0x02);
                writer.Write((byte)Scheme);
                writer.WriteShort(Alias.Length);
                writer.Write(Encoding.UTF8.GetBytes(Alias));
                return stream.ToArray();
            }
        }

        public override Dictionary<string, object> GetJson()
        {
            return new Dictionary<string, object>
                {
                    {"type", TransactionType.Alias},
                    {"senderPublicKey", SenderPublicKey.ToBase58()},
                    {"alias", Alias},
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