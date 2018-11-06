using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WavesCS
{
    public class DataTransaction : Transaction
    {
        public Dictionary<string, object> Entries { get; }
        public override byte Version { get; set; } = 1;

        public DataTransaction(byte[] senderPublicKey, Dictionary<string, object> entries,
            decimal? fee = null) : base(senderPublicKey)
        {
            Entries = entries;
            Fee = fee ?? ((GetBody().Length + 70) / 1024 + 1) * 0.001m;
        }

        public DataTransaction(Dictionary<string, object> tx) : base(tx)
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

        public override Dictionary<string, object> GetJson()
        {
            return new Dictionary<string, object>
            {
                {"type", (byte) TransactionType.DataTx},
                {"version", Version},
                {"senderPublicKey", SenderPublicKey.ToBase58() },
                {"sender", Sender},
                {"data", Entries.Select(pair => new Dictionary<string, object>
                {
                    {"key", pair.Key},
                    {"type", pair.Value is long ? "integer" : (pair.Value is bool ? "boolean" : (pair.Value is string ? "string"  : "binary"))},
                    {"value", pair.Value is byte[] bytes ? bytes.ToBase64() : pair.Value }                    
                })},
                {"fee", Assets.WAVES.AmountToLong(Fee)},
                {"timestamp", Timestamp.ToLong()},                
            };
        }

        protected override bool SupportsProofs()
        {
            return true;
        }
    }
}