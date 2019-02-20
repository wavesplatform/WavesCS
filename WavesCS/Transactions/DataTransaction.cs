using System;
using System.IO;
using System.Linq;
using System.Text;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public class DataTransaction : Transaction
    {
        public DictionaryObject Entries { get; }
        public override byte Version { get; set; } = 1;

        public DataTransaction(char chainId, byte[] senderPublicKey, DictionaryObject entries,
            decimal? fee = null) : base(chainId, senderPublicKey)
        {
            Entries = entries;
            Fee = fee ?? ((GetBody().Length + 70) / 1024 + 1) * 0.001m;
        }

        public DataTransaction(DictionaryObject tx) : base(tx)
        {
            Entries = tx.GetObjects("data")
                        .ToDictionary(o => o.GetString("key"), Node.DataValue);

            Fee = Assets.WAVES.LongToAmount(tx.GetLong("fee"));
        }

        public override byte[] GetBody()
        {
            using(var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                const byte INTEGER = 0;
                const byte BOOLEAN = 1;
                const byte BINARY = 2;
                const byte STRING = 3;                

                writer.Write(TransactionType.DataTx);
                writer.Write(Version);
                writer.Write(SenderPublicKey);
                writer.WriteShort((short) Entries.Count);
                foreach (var pair in Entries)
                {
                    var key = Encoding.UTF8.GetBytes(pair.Key);
                    writer.WriteShort((short) key.Length);
                    writer.Write(key);
                    switch (pair.Value)
                    {
                        case long value:
                            writer.Write(INTEGER);
                            writer.WriteLong(value);
                            break;
                        case bool value:
                            writer.Write(BOOLEAN);
                            writer.Write(value ? (byte) 1 : (byte) 0);
                            break;
                        case byte[] value:
                            writer.Write(BINARY);
                            writer.WriteShort((short) value.Length);
                            writer.Write(value);
                            break;
                        case string value:
                            writer.Write(STRING);
                            var encoded = Encoding.UTF8.GetBytes(value);
                            writer.WriteShort((short) encoded.Length);
                            writer.Write(encoded);
                            break;
                        default:
                            throw new ArgumentException("Only long, bool and byte[] entry values supported",
                                nameof(Entries));
                    }
                }

                writer.WriteLong(Timestamp.ToLong());
                writer.WriteLong(Assets.WAVES.AmountToLong(Fee));
                return stream.ToArray();
            }
        }

        public override byte[] GetIdBytes()
        {
            return GetBody();
        }

        public override DictionaryObject GetJson()
        {
            var result = new DictionaryObject
            {
                {"type", (byte) TransactionType.DataTx},
                {"version", Version},
                {"senderPublicKey", SenderPublicKey.ToBase58() },
                {"data", Entries.Select(pair => new DictionaryObject
                {
                    {"key", pair.Key},
                    {"type", pair.Value is long ? "integer" : (pair.Value is bool ? "boolean" : (pair.Value is string ? "string"  : "binary"))},
                    {"value", pair.Value is byte[] bytes ? bytes.ToBase64() : pair.Value }                    
                })},
                {"fee", Assets.WAVES.AmountToLong(Fee)},
                {"timestamp", Timestamp.ToLong()},                
            };

            if (Sender != null)
                result.Add("sender", Sender);

            return result;
        }

        protected override bool SupportsProofs()
        {
            return true;
        }
    }
}