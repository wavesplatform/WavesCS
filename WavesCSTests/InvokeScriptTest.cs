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
            var node = new Node('T');

            var account = PrivateKeyAccount.CreateFromSeed("ceededfrgsfdsfgd", 'T');
            // Too big sequences requested
            var key = "abcde";
            for (int i = 0; i < 9; i++)
                key = key + key;

            Console.WriteLine(key.Length);

            var entries1 = new Dictionary<string, object>();
            entries1.Add(key, true);
            node.PutData(account, entries1);
            return;

            var list = new List<string> {
            "3PAF2wY4jWergoFZQTTbrfm2GhgSB9SJYGZ",
            "3PHjcsByieBMnbzN2DXmagpcjVaprXaaXpb",
            "3PKnYbrB3Ut3iyiMoLbugPAUqt4nWQSJBCH",
            "3PFjEXN6cFveHRaKUeaiU3bkgyTJEB6eTL8",
            "3P5FNTdMP6fNHmMPzkPPMVdCf5yF8gj73yN",
            "3PH1pJ5KPW8Pkr96cSzuTMzPmLgpZ454Pt7",
            "3PLkctrmnUGje5Pvnv9AC95bE1Vw3xbeckz",
            "3P3rBo2sVzKZQvbQponRFWXmocCNSL4hGPX",
            "3PDEnVbcLhsYbssbLbo3mHCwZYPAxD9JQQZ",
            "3P5XssF5utyr6iwoE8igARCcasFCoDwMSAQ",
            "3P9jA8yJH1wLso6vVC4VqEC5j8QAQ4kFdfD",
            "3PFqte55VmWVKazeeA3yHaSKKMH8uaDaJai",
            "3PNoMfHGLs165diG6erM3fZAAAWk7DtG5er",
            "3PKPNTzQiQjqxSUqQ5vUUpDrcEdZrNqXuuM",
            "3P9fJjn7Wekh9oYikoRsLMT78ahUJrWi62b",
            "3P3rtPFXLRiLz7NshUeFx3qbBxWj8Pauq83",
            "3PGpajKXFVegL2bjV31pLHMubsX3XSWZLzA",
            "3P9eqFU2fEvdic2657ozmDTsQJRkg71hCtT",
            "3PDEEMRU3tBJffag7sRPnjwEQ8GZGikUWBi",
            "3PK2MrcuSKwP6QTc8XQR9g1PCjqFwiNxzW3",
            "3P2bd2EQ1vojNuCmFYa3d7qrn7JMSz1EXBz",
            "3PPPPPppPSVGZFcnZsfvBRdDzEMiZfWfU1e"
        };
            var entries = new Dictionary<string, object>
            ();
            foreach(var l in list)
            {
                entries.Add(l, "ambassador");
            }

            var sender = PrivateKeyAccount.CreateFromSeed("tourist kidney glove soup admit sword discover order vessel vanish mirror dwarf hole renew impose", 'W');

            var tx = new DataTransaction('W', sender.PublicKey, entries, 0.0001m);
            //var node = new Node('W');
            node.Broadcast(tx.Sign(sender).GetJsonWithSignature());
            return;


            throw new NotImplementedException();

            /*
create contract account with 5 waves
  create caller account with 5 waves
  set contract to contract account
    scriptText =
      """
        |{-# STDLIB_VERSION 3 #-}
        |{-# CONTENT_TYPE CONTRACT #-}
        |
        | @Callable(inv)
        | func foo(a:ByteVector) = {
        |  WriteSet([DataEntry("a", a), DataEntry("sender", inv.caller.bytes)])
        | }
        | 
        | @Verifier(t)
        | func verify() = {
        |  true
        | }
        |
        |
        """

    InvokeScript(functionCallHeader: User("foo"), functionCallArgs: [[(byte)42]]
    getData(contract.address)["a"] shouldBe arg
    getData(contract.address)["sender"] shouldBe caller.toAddress.bytes


    DataTransaction(sender = contract, data: {"a", "OOO"})
    getData(contract.address)["a"] shouldBe "OOO"

         

             */
        }
    }
}