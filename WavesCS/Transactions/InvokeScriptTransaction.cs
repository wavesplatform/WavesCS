using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public class InvokeScriptTransaction : Transaction
    {
        public string DappAddress;

        public string FunctionHeader;
        public List<object> FunctionCallArguments;

        public Dictionary<Asset, decimal> Payment;

        public Asset FeeAsset;

        public override byte Version { get; set; } = 1;

        public InvokeScriptTransaction(DictionaryObject tx, Node node) : base(tx)
        {
            DappAddress = tx.GetString("dApp");
            FunctionHeader = tx.ContainsKey("call") ? tx.GetString("call.function") : null;

            FunctionCallArguments = tx.GetObjects("call.args")
                                        .Select(Node.DataValue)
                                        .ToList();

            Payment = tx.GetObjects("payment")
                        .ToDictionary(o => node.GetAsset(o.GetString("assetId")),
                                      o => node.GetAsset(o.GetString("assetId")).LongToAmount(o.GetLong("amount")));

            FeeAsset = tx.ContainsKey("feeAssetId") && tx.GetString("feeAssetId") != null ? node.GetAsset(tx.GetString("feeAssetId")) : Assets.WAVES;
            Fee = FeeAsset.LongToAmount(tx.GetLong("fee"));
        }

        public InvokeScriptTransaction(char chainId, byte[] senderPublicKey,
            string dappAddress, string functionHeader, List<object> functionCallArguments,
             Dictionary<Asset, decimal> payment, decimal fee, Asset feeAsset) : base(chainId, senderPublicKey)
        {
            DappAddress = dappAddress;
            FunctionHeader = functionHeader;
            FunctionCallArguments = functionCallArguments ?? new List<object>();
            Payment = payment ?? new Dictionary<Asset, decimal>();
            Fee = fee;
            FeeAsset = feeAsset ?? Assets.WAVES;
        }

        public InvokeScriptTransaction(char chainId, byte[] senderPublicKey,
            string dappAddress,
             Dictionary<Asset, decimal> payment, decimal fee, Asset feeAsset) : base(chainId, senderPublicKey)
        {
            DappAddress = dappAddress;
            Payment = payment ?? new Dictionary<Asset, decimal>();
            Fee = fee;
            FeeAsset = feeAsset ?? Assets.WAVES;
            FunctionHeader = null;
            FunctionCallArguments = new List<object>();
        }

        public override byte[] GetBody()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
        
            writer.Write(TransactionType.InvokeScript);
            writer.Write(Version);
            writer.Write((byte)ChainId);

            writer.Write(SenderPublicKey);
            writer.Write(DappAddress.FromBase58());

            if (FunctionHeader != null)
            {
                writer.WriteByte((byte)1);
                writer.WriteByte((byte)9);
                writer.WriteByte((byte)1);

                writer.WriteInt(FunctionHeader.Length);
                writer.Write(Encoding.UTF8.GetBytes(FunctionHeader));
                writer.WriteInt(FunctionCallArguments.Count);
                foreach (var argument in FunctionCallArguments)
                {
                    writer.WriteEvaluatedExpression(argument);
                }
            }
            else
            {
                writer.WriteByte(0);
            }

            writer.WriteShort(Payment.Count);

            foreach(var p in Payment)
            {
                var tmpStream = new MemoryStream();
                var tmpWriter = new BinaryWriter(tmpStream);

                tmpWriter.WriteLong(p.Key.AmountToLong(p.Value));

                var id = p.Key.IdOrNull?.FromBase58();

                if (id == null)
                {
                    tmpWriter.WriteByte(0);
                }
                else
                {
                    tmpWriter.WriteByte(1);
                    // tmpWriter.WriteShort(id.Length);
                    tmpWriter.Write(id);
                }

                var array = tmpStream.ToArray();
                writer.WriteShort(array.Count());
                writer.Write(array);
            }

            writer.WriteLong(FeeAsset.AmountToLong(Fee));
            writer.WriteAsset(FeeAsset.Id);
            writer.WriteLong(Timestamp.ToLong());

            return stream.ToArray();
        }

        public override byte[] GetBytes()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.WriteByte(0);
            writer.Write(GetBody());
            writer.Write(GetProofsBytes());

            return stream.ToArray();
        }

        public override DictionaryObject GetJson()
        {
            return new DictionaryObject {
                {"type", (byte) TransactionType.InvokeScript},
                {"senderPublicKey", SenderPublicKey.ToBase58()},
                {"fee", FeeAsset.AmountToLong(Fee)},
                {"feeAssetId", FeeAsset.IdOrNull},
                {"timestamp", Timestamp.ToLong()},
                {"version", Version},
                {"dApp", DappAddress},
                {"call", FunctionHeader != null ? new DictionaryObject
                {
                    {"function", FunctionHeader},
                    {"args", FunctionCallArguments != null ? FunctionCallArguments.Select(arg => new DictionaryObject
                    {
                        {"type", arg is long ? "integer" : (arg is bool ? "boolean" : (arg is string ? "string"  : "binary"))},
                        {"value", arg is byte[] bytes ? bytes.ToBase64() : arg }
                    }) : new List<Dictionary<string, object>>()}
                } : null},
                { "payment", Payment.Select(p => new DictionaryObject
                    {
                        {"amount", p.Key.AmountToLong(p.Value) },
                        {"assetId", p.Key.IdOrNull }
                    })
                }
            };
        }

        protected override bool SupportsProofs()
        {
            return true;
        }
    }
}
