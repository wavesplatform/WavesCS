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

        public decimal PaymentAmount;
        public Asset PaymentAsset;
        // public Asset FeeAsset;

        public override byte Version { get; set; } = 1;

        public InvokeScriptTransaction(DictionaryObject tx) : base(tx)
        {
            var node = new Node(tx.GetChar("chainId"));

            ContractAddress = tx.GetString("contractAddress");
            FunctionHeader = tx.GetString("call.function");

            FunctionCallArguments = (List<object>) tx.GetValue("call.args");
            PaymentAsset = Assets.WAVES;

            if (tx.ContainsKey("payment.assetId")
                && tx.GetString("payment.assetId") != null
                && tx.GetString("payment.assetId") != "")
            {
                PaymentAsset = node.GetAsset(tx.GetString("payment.assetId"));
            }

            PaymentAmount = PaymentAsset.LongToAmount(tx.GetLong("paymentAmount"));
            Fee = Assets.WAVES.LongToAmount(tx.GetLong("fee"));
        }

        public InvokeScriptTransaction(char chainId, byte[] senderPublicKey,
            string contractAddress, string functionHeader, List<object> functionCallArguments,
            decimal paymentAmount, Asset paymentAsset, decimal fee) : base(chainId, senderPublicKey)
        {
            ContractAddress = contractAddress;
            FunctionHeader = functionHeader;
            FunctionCallArguments = functionCallArguments;
            PaymentAmount = paymentAmount;
            PaymentAsset = paymentAsset;
            Fee = fee;
        }

        public override byte[] GetBody()
        {
            throw new NotImplementedException();
        }

        internal override byte[] GetIdBytes()
        {
            throw new NotImplementedException();
        }

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
                { "payment", PaymentAmount > 0 ? new DictionaryObject { { "amount", PaymentAsset.AmountToLong(PaymentAmount) }, { "assetId", PaymentAsset.IdOrNull } } : null}
            };
        }

        protected override bool SupportsProofs()
        {
            return true;
        }
    }
}
