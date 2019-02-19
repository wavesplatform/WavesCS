using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;
using KeyValuePair = System.Collections.Generic.KeyValuePair<string, object>;
using System.Threading;

namespace WavesCSTests
{
    [TestClass]
    public class TagSystem
    {
        PrivateKeyAccount TagAccount = PrivateKeyAccount.CreateFromPrivateKey("Akq6zmjRJzhaWm14psa5HgiP6nRHDBXSDbXNo8BhwHhn", 'T');

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
        readonly String initialWord = "aaa";
        int initialIndex = 0;

        [TestMethod]
        public void SendInitialDataTagSystem()
        {
            var node = new Node();
            DictionaryObject data = new DictionaryObject { };
            string word = "";
            foreach (char symbol in initialWord){
                data.Add(initialIndex.ToString(), symbol.ToString());
                word += symbol;
                initialIndex++;
            }                        
            foreach (KeyValuePair entry in rules)
            {                                
                data.Add(entry.Key, entry.Value);
            }
            data.Add("position", (long)0);
            data.Add("length", (long)initialIndex);
            data.Add("state", word);
            var tx = new DataTransaction(TagAccount.PublicKey, data).Sign(TagAccount);
            node.Broadcast(tx.GetJsonWithSignature());
        }

        [TestMethod]
        public void SendNewDataTransaction()
        {
            var node = new Node();
            var data = node.GetAddressData(TagAccount.Address);
            var tag = data["state"];
            
            long position = (long)data["position"];
            long length = (long)data["lenght"];

            var currentSymbol = data[position.ToString()].ToString();
            data["position"] = position + 2; //2-tag            
                
            string word = data["state"].ToString();
            foreach (char symbol in rules[currentSymbol].ToString())
            {
                data.Add(length.ToString(), symbol.ToString());
                length++;
                word += symbol;
            }
            data["lenght"] = length;
            data["state"] = word.ToString();
            var tx = new DataTransaction(TagAccount.PublicKey, data, 0).Sign(TagAccount);
            node.Broadcast(tx.GetJsonWithSignature());           

        }

        [TestMethod]
        public void SetScriptTagSystem()
        {
            var node = new Node();

            var script = @"match tx {
                                        case tx:DataTransaction =>
                                        let currentLength = extract(getString(tx.sender, ""length""))
                                        let currentPosition = extract(getInteger(tx.sender, ""position""))
                                        let currentSymbol = extract(getString(tx.sender, toString(currentPosition)))
                                        let currentRule = extract(getString(tx.sender, currentSymbol))
                                        let currentState = extract(getString(tx.sender, ""state""))
                                        currentPosition + 2 == extract(getInteger(tx.data, ""position"")) &&
                                        takeRight(extract(getString(tx.data, ""state"")), size(currentRule)) == currentRule
                                        case _ => false
                                    }";

            var compiledScript = node.CompileScript(script);

            var setScriptTx = new SetScriptTransaction(TagAccount.PublicKey, compiledScript, 'T');
            setScriptTx.Sign(TagAccount);
            node.Broadcast(setScriptTx.GetJsonWithSignature());
            Thread.Sleep(10000);
        }

        [TestMethod]
        public void CreateAccount()
        {
            var seed = PrivateKeyAccount.GenerateSeed();
            PrivateKeyAccount account = PrivateKeyAccount.CreateFromSeed(seed, 'T'); ;

            Console.WriteLine($"seed: {seed}");
            Console.WriteLine($"address: {account.Address}");
            Console.WriteLine($"public: {account.PublicKey.ToBase58()}");
            Console.WriteLine($"private: {account.PrivateKey.ToBase58()}");

            Thread.Sleep(2000);

            var node = new Node();
            node.Transfer(Accounts.Carol, account.Address, Assets.WAVES, 1m);

            Thread.Sleep(2000);

        }
    }
}
