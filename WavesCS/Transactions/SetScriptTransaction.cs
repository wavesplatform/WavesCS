using System.Collections.Generic;
using System.IO;

namespace WavesCS
{
    public class SetScriptTransaction : Transaction
    {
        public byte[] Script { get; }
        public char ChainId { get; }
        public decimal Fee { get; }
        
        public readonly byte Version = 1;

        public SetScriptTransaction(byte[] senderPublicKey, byte[] script, char chainId, decimal fee = 0.005m) : base(senderPublicKey)
        {
            Script = script;
            ChainId = chainId;
            Fee = fee;
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
                    writer.WriteShort(Script.Length);
                    writer.Write(Script);
                }
                writer.WriteLong(Assets.WAVES.AmountToLong(Fee));
                writer.WriteLong(Timestamp.ToLong());                                               

                return stream.ToArray();
            }            
    
        }

        public override Dictionary<string, object> GetJson()
        {
            return new Dictionary<string, object>
            {
                {"type", TransactionType.SetScript},
                {"version", Version},
                {"senderPublicKey", SenderPublicKey.ToBase58()},
                {"script", Script == null ? null : Script.ToBase64()},
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