using System.IO;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public class SetScriptTransaction : Transaction
    {
        public byte[] Script { get; }
        public char ChainId { get; }

        public override byte Version { get; set; } = 1;

        public SetScriptTransaction(byte[] senderPublicKey, byte[] script, char chainId, decimal fee = 0.01m) : base(senderPublicKey)
        {
            Script = script;
            ChainId = chainId;
            Fee = fee;
        }

        public SetScriptTransaction(DictionaryObject tx) : base(tx)
        {
            Script = tx.GetString("script").FromBase64();
            Fee = Assets.WAVES.LongToAmount(tx.GetLong("fee"));
            ChainId = tx.GetChar("chainId");
        }

        public override byte[] GetBody()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {                   
                writer.Write(TransactionType.SetScript);                
                writer.Write(Version);
                writer.Write(ChainId);
                writer.Write(SenderPublicKey);
                
                if (Script == null)
                {
                    writer.Write((byte) 0);
                }
                else
                {
                    writer.Write((byte) 1);
                    writer.WriteShort((short)Script.Length);
                    writer.Write(Script);
                }
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
            var result = new DictionaryObject
            {
                {"type", (byte) TransactionType.SetScript},
                {"version", Version},
                {"senderPublicKey", SenderPublicKey.ToBase58()},
                {"script", Script?.ToBase64()},
                {"fee", Assets.WAVES.AmountToLong(Fee)},
                {"timestamp", Timestamp.ToLong()}
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