using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS;

namespace WavesCSTests
{
    [TestClass]
    public class SetScriptTest
    {        
        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }

        [TestMethod]
        public void TestSetScript()
        {
            var node = new Node();

            var script = "true";
            var compiledScript = node.Post("/utils/script/compile", script).ParseJsonObject().Get<string>("script");
            
            Console.WriteLine("Compiled script: {0}", compiledScript);
       
            var setScriptTx = new SetScriptTransaction(Accounts.Carol.PublicKey, compiledScript.FromBase64(), 'T');            
            setScriptTx.Sign(Accounts.Carol);
            node.Broadcast(setScriptTx.GetJsonWithSignature());

            Thread.Sleep(10000);
            
            var scriptInfo = node.GetObject("addresses/scriptInfo/{0}", Accounts.Carol.Address);
            Assert.AreEqual("TRUE", scriptInfo["scriptText"]);
            Assert.AreEqual(compiledScript, scriptInfo["script"]);
            Assert.IsTrue(scriptInfo.GetInt("complexity") > 0);
            Assert.IsTrue(scriptInfo.GetInt("extraFee") > 0);
            
            var cleanScriptTx = new SetScriptTransaction(Accounts.Carol.PublicKey, null, 'T');                        
            node.Broadcast(cleanScriptTx.GetJsonWithSignature());
            
            Thread.Sleep(10000);
            
            scriptInfo = node.GetObject("addresses/scriptInfo/{0}", Accounts.Carol.Address);
            Assert.IsFalse(scriptInfo.ContainsKey("scriptText"));
            Assert.IsFalse(scriptInfo.ContainsKey("script"));
            Assert.AreEqual(0, scriptInfo.GetInt("complexity"));
            Assert.AreEqual(0, scriptInfo.GetInt("extraFee"));
        }
        
    }
}