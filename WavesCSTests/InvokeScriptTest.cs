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
        PrivateKeyAccount Alice = PrivateKeyAccount.CreateFromSeed("seedAlice123", Node.TestNetChainId);
        PrivateKeyAccount Bob = PrivateKeyAccount.CreateFromSeed("seedBob123", Node.TestNetChainId);

        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }

        [TestMethod]
        public void TestInvokeScript()
        {
            var node = new Node(Node.TestNetChainId);

            var script = @"{-# STDLIB_VERSION 3 #-}
{-# CONTENT_TYPE DAPP #-}
{-# SCRIPT_TYPE ACCOUNT #-}

@Callable(inv)
func foo (a:ByteVector) = {
    WriteSet([DataEntry(""a"", a),
    DataEntry(""sender"", inv.caller.bytes)])
}";
            var compiledScript = node.CompileCode(script);

            var response = node.SetScript(Alice, compiledScript);
            node.WaitTransactionConfirmationByResponse(response);

            response = node.InvokeScript(Bob, Alice.Address, "foo", new List<object> { 42L }, null);
            node.WaitTransactionConfirmationByResponse(response); 

            Assert.AreEqual((long)node.GetAddressData(Alice.Address)["a"], 42L);
            Assert.AreEqual(((byte[])node.GetAddressData(Alice.Address)["sender"]).ToBase58(), Bob.Address);

            var dataTx = new DataTransaction(
                chainId: node.ChainId,
                senderPublicKey: Alice.PublicKey,
                entries: new Dictionary<string, object> { { "a", "OOO" } },
                fee: 0.005m
            ).Sign(Alice);

            node.BroadcastAndWait(dataTx);

            Assert.AreEqual(node.GetAddressData(Alice.Address)["a"], "OOO");

            response = node.SetScript(Alice, null);
            node.WaitTransactionConfirmationByResponse(response);

            var scriptInfo = node.GetObject("addresses/scriptInfo/{0}", Alice.Address);
            Assert.IsFalse(scriptInfo.ContainsKey("scriptText"));
        }

        [TestMethod]
        public void TestInvokeScriptArguments()
        {
            var node = new Node(Node.TestNetChainId);

            var script = @"{-# STDLIB_VERSION 3 #-}
{-# CONTENT_TYPE DAPP #-}
{-# SCRIPT_TYPE ACCOUNT #-}


@Callable(inv)
func c (x: Int) = {
    WriteSet([DataEntry(""c"", x + 1)])
}

@Callable(inv)
func d (x1: Boolean, x2: Int, x3: String, x4: ByteVector) = {
    WriteSet(
    [DataEntry(""d1"", x1),
    DataEntry(""d2"", x2 + x2),
    DataEntry(""d3"", x3 + x3),
    DataEntry(""d4"", x4)])
}

@Callable(inv)
func e () = {
    WriteSet([DataEntry(""e"", ""e"")])
}";

            var compiledScript = node.CompileCode(script);

            var response = node.SetScript(Alice, compiledScript);
            node.WaitTransactionConfirmationByResponse(response);

            response = node.InvokeScript(Bob, Alice.Address, "e", null, null);
            node.WaitTransactionConfirmationByResponse(response);
            Assert.AreEqual((string)node.GetAddressData(Alice.Address)["e"], "e");

            response = node.InvokeScript(Bob, Alice.Address, "c", new List<object> { 150L }, null);
            node.WaitTransactionConfirmationByResponse(response);
            Assert.AreEqual((long)node.GetAddressData(Alice.Address)["c"], 151L);

            response = node.InvokeScript(Bob, Alice.Address, "d", new List<object> { true, 150L, "hello!", Bob.Address.FromBase58() }, null);
            node.WaitTransactionConfirmationByResponse(response);
            Assert.AreEqual((bool)node.GetAddressData(Alice.Address)["d1"], true);
            Assert.AreEqual((long)node.GetAddressData(Alice.Address)["d2"], 300L);
            Assert.AreEqual((string)node.GetAddressData(Alice.Address)["d3"], "hello!hello!");
            Assert.AreEqual(((byte[])node.GetAddressData(Alice.Address)["d4"]).ToBase58(), Bob.Address);

            response = node.SetScript(Alice, null);
            node.WaitTransactionConfirmationByResponse(response);

            var scriptInfo = node.GetObject("addresses/scriptInfo/{0}", Alice.Address);
            Assert.IsFalse(scriptInfo.ContainsKey("scriptText"));

        }

        [TestMethod]
        public void TestInvokeScriptDefaultFunction()
        {
            
            var node = new Node(Node.TestNetChainId);

            var script = @"{-# STDLIB_VERSION 3 #-}
{-# CONTENT_TYPE DAPP #-}

@Callable(inv)
func foo (a:ByteVector) = {
    WriteSet([DataEntry(""a"", a),
    DataEntry(""sender"", inv.caller.bytes)])
}

@Callable(inv)
          func default() = {
            WriteSet([DataEntry(""aa"", ""aa""),
            DataEntry(""sender"", inv.caller.bytes)])
          }";
            var compiledScript = node.CompileCode(script);

            var response = node.SetScript(Alice, compiledScript);
            node.WaitTransactionConfirmationByResponse(response);

            response = node.InvokeScript(Bob, Alice.Address, null);
            node.WaitTransactionConfirmationByResponse(response);

            Assert.AreEqual((string)node.GetAddressData(Alice.Address)["a"], "aaa");
            Assert.AreEqual(((byte[])node.GetAddressData(Alice.Address)["sender"]).ToBase58(), Bob.Address);

            response = node.SetScript(Alice, null);
            node.WaitTransactionConfirmationByResponse(response);

            var scriptInfo = node.GetObject("addresses/scriptInfo/{0}", Alice.Address);
            Assert.IsFalse(scriptInfo.ContainsKey("scriptText"));
        }
    }
}