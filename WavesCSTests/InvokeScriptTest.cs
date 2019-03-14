using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS;

namespace WavesCSTests
{
    [TestClass]
    public class InvokeScriptTest
    {
        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }

        [TestMethod]
        public void TestInvokeScript()
        {
            var node = new Node("http://1.devnet.wavesnodes.com:6869", 'D');

            var Alice = PrivateKeyAccount.CreateFromSeed("seedAlice123", 'D');
            var Bob = PrivateKeyAccount.CreateFromSeed("seedBob123", 'D');

            var script = @"{-# STDLIB_VERSION 3 #-}
{-# CONTENT_TYPE CONTRACT #-}

@Callable(inv)
func foo (a:ByteVector) = {
    WriteSet([DataEntry(""a"", a),
    DataEntry(""sender"", inv.caller.bytes)])
}";
            var compiledScript = node.CompileScript(script);

            node.SetScript(Accounts.Alice, compiledScript);
            Thread.Sleep(3000);

            node.InvokeScript(Bob, Alice.Address, "foo", new List<object> { 42L }, 0, null);

            Assert.AreEqual((long)node.GetAddressData(Alice.Address)["a"], 42L);
            Assert.AreEqual(node.GetAddressData(Alice.Address)["sender"], Bob.Address);

            node.PutData(Alice, new Dictionary<string, object> { { "a", "OOO" } });

            Assert.AreEqual((string)node.GetAddressData(Alice.Address)["a"], "OOO");

            node.SetScript(Accounts.Alice, null);
            Thread.Sleep(3000);

            var scriptInfo = node.GetObject("addresses/scriptInfo/{0}", Alice.Address);
            Assert.IsFalse(scriptInfo.ContainsKey("scriptText"));
        }
    }
}