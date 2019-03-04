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
        public string ContractAddress;

        public string FunctionHeader; // Native(Short) | User(String)
        public List<object> FunctionCallArguments;

        public long PaymentAmount;
        public Asset PaymentAsset;

        public override byte Version { get; set; } = 1;

        public InvokeScriptTransaction(DictionaryObject tx) : base(tx) => throw new NotImplementedException();
        public InvokeScriptTransaction(char chainId, byte[] senderPublicKey, string contractAddress,
                                       string FunctionHeader, List<string> functionCallArguments,
                                       long PaymentAmount, Asset PaymentAsset, decimal fee) : base(chainId, senderPublicKey) => throw new NotImplementedException();

        public override byte[] GetBody() => throw new NotImplementedException();
        internal override byte[] GetIdBytes() => throw new NotImplementedException();
        public override DictionaryObject GetJson()
        {
            return new DictionaryObject {
                {"type", (byte) TransactionType.InvokeScript},
                {"senderPublicKey", SenderPublicKey.ToBase58()},
                {"fee", Assets.WAVES.AmountToLong(Fee)},
                {"timestamp", Timestamp.ToLong()},
                {"version", Version},
                {"contractAddress", ContractAddress},
                {"call", new DictionaryObject
                {
                    {"function", FunctionHeader},
                    {"args", FunctionCallArguments.Select(arg => new DictionaryObject
                    {
                        {"type", arg is long ? "integer" : (arg is bool ? "boolean" : (arg is string ? "string"  : "binary"))},
                        {"value", arg is byte[] bytes ? bytes.ToBase64() : arg }
                    })}
                }},
                { "payment", PaymentAmount > 0 ? new DictionaryObject { { "amount", PaymentAmount }, { "assetId", PaymentAsset.IdOrNull } } : null}
            };
        }

        protected override bool SupportsProofs()
        {
            return true;
        }
    }
}
