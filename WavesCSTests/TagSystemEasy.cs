using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;
using KeyValuePair = System.Collections.Generic.KeyValuePair<string, object>;
using System.Threading;

namespace WavesCSTests
{
    [TestClass]
    public class TagSystemEasy
    {
        PrivateKeyAccount TagAccount = PrivateKeyAccount.CreateFromPrivateKey("YubyC2Q7kLEeVLVMmzmYH9QgsaQTfYjw5wTG8tMeMdZ", 'T');

        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
            //TransferTransaction.Version = 2;
        }

        readonly DictionaryObject rules = new DictionaryObject
            {            
                { "a", "bc" },
                { "b", "a" },
                { "c", "aaa" }

            };
        readonly String initialWord = "a";
        readonly int tag = 2;

        [TestMethod]
        public void SendInitialDataTagSystem()
        {
            var node = new Node();
            DictionaryObject data = new DictionaryObject { };  
            foreach (KeyValuePair entry in rules)
            {                                
                data.Add(entry.Key, entry.Value);
            }
            data.Add("tag", (long)tag);
            data.Add("state", initialWord);
            var tx = new DataTransaction(TagAccount.PublicKey, data).Sign(TagAccount);
            node.Broadcast(tx.GetJsonWithSignature());
        }

        [TestMethod]
        public void SendNewDataTransaction()
        {
            var node = new Node();
            var data = node.GetAddressData(TagAccount.Address);
            var state = (String)data["state"];
            var rule = rules[state[0].ToString()];
            state = state.Remove(0, (int)(long)data["tag"]);
            
            state = state + rule;

            DictionaryObject newData = new DictionaryObject();
            newData.Add("state", state);
            var tx = new DataTransaction(TagAccount.PublicKey, newData, 0).Sign(TagAccount);
            node.Broadcast(tx.GetJsonWithSignature());           

        }

        [TestMethod]
        public void SetScriptTagSystem()
        {
            var node = new Node();

            var script = @"match tx {
                                        case tx:DataTransaction =>       
                                        let currentState = extract(getString(tx.sender, ""state""))
                                        let currentRule = extract(getString(tx.sender, take(currentState, 1)))
                                        let newState = extract(getString(tx.data, ""state""))
                                        let tag = extract(getInteger(tx.sender, ""tag""))
                                        size(currentState) >= tag && newState == drop(currentState, tag) + currentRule && size(tx.data) == 1                                             
                                        case _ => false
                                    }";

            var compiledScript = node.CompileScript(script);

            var setScriptTx = new SetScriptTransaction(TagAccount.PublicKey, compiledScript, 'T');
            setScriptTx.Sign(TagAccount);
            node.Broadcast(setScriptTx.GetJsonWithSignature());
            Thread.Sleep(10000);
        }      
    }
}
