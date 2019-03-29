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

        public InvokeScriptTransaction(DictionaryObject tx) : base(tx)
        {
            var node = new Node(tx.GetChar("chainId"));

            DappAddress = tx.GetString("dappAddress");
            FunctionHeader = tx.GetString("call.function");

            FunctionCallArguments = tx.GetObjects("call.args")
                                        .Select(Node.DataValue)
                                        .ToList();

            Payment = tx.GetObjects("payment")
                        .ToDictionary(o => node.GetAsset(o.GetString("asset")),
                                      o => node.GetAsset(o.GetString("asset")).LongToAmount(o.GetLong("amount")));

            FeeAsset = tx.ContainsKey("feeAssetId") && tx.GetString("feeAssetId") != null ? node.GetAsset(tx.GetString("feeAssetId")) : Assets.WAVES;
            Fee = FeeAsset.LongToAmount(tx.GetLong("fee"));
        }

        public InvokeScriptTransaction(char chainId, byte[] senderPublicKey,
            string dappAddress, string functionHeader, List<object> functionCallArguments,
             Dictionary<Asset, decimal> payment, decimal fee, Asset feeAsset) : base(chainId, senderPublicKey)
        {
            DappAddress = dappAddress;
            FunctionHeader = functionHeader;
            FunctionCallArguments = functionCallArguments;
            Payment = payment ?? new Dictionary<Asset, decimal>();
            Fee = fee;
            FeeAsset = feeAsset ?? Assets.WAVES;
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

            writer.WriteByte((byte)9);
            writer.WriteByte((byte)1);

            writer.WriteInt(FunctionHeader.Length);
            writer.Write(Encoding.UTF8.GetBytes(FunctionHeader));
            writer.WriteInt(FunctionCallArguments.Count);

            foreach (var argument in FunctionCallArguments)
            {
                writer.WriteObject(argument);
            }

            writer.WriteShort(Payment.Count);
                
            foreach(var p in Payment)
            {
                writer.Write(p.Key.AmountToLong(p.Value));
                writer.WriteAsset(p.Key.Id);
            }

            writer.WriteLong(FeeAsset.AmountToLong(Fee));
            writer.WriteAsset(FeeAsset.Id);
            writer.WriteLong(Timestamp.ToLong());

            return stream.ToArray();
        }

        internal override byte[] GetIdBytes()
        {
            return GetBody();
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
                {"dappAddress", DappAddress},
                {"call", new DictionaryObject
                {
                    {"function", FunctionHeader},
                    {"args", FunctionCallArguments.Select(arg => new DictionaryObject
                    {
                        {"type", arg is long ? "integer" : (arg is bool ? "boolean" : (arg is string ? "string"  : "binary"))},
                        {"value", arg is byte[] bytes ? bytes.ToBase64() : arg }
                    })}
                }},
                { "payment", Payment.Select(p => new DictionaryObject
                    {
                        {"amount", p.Key.AmountToLong(p.Value) },
                        {"asset", p.Key.IdOrNull }
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
