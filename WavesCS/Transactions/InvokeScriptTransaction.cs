using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public class InvokeScriptTransaction : Transaction
    {
        public string ContractAddress;

        public string FunctionHeader; // Native(Short) | User(String)
        public List<string> FunctionCallArguments;

        public long PaymentAmount;
        public Asset PaymentAsset;

        public override byte Version { get; set; } = 1;

        public InvokeScriptTransaction(DictionaryObject tx) : base(tx) => throw new NotImplementedException();
        public InvokeScriptTransaction(char chainId, byte[] senderPublicKey, string contractAddress,
                                       string FunctionHeader, List<string> functionCallArguments,
                                       long PaymentAmount, Asset PaymentAsset, decimal fee) : base(chainId, senderPublicKey) => throw new NotImplementedException();

        public override byte[] GetBody() => throw new NotImplementedException();
        internal override byte[] GetIdBytes() => throw new NotImplementedException();
        public override DictionaryObject GetJson() => throw new NotImplementedException();
        protected override bool SupportsProofs() => throw new NotImplementedException();
    }
}