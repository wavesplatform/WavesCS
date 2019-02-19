using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS;

namespace WavesPoWMiner
{
    [TestClass]
    public class MainClass
    {
        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }

        static Node node = new Node(Node.MainNetHost, 'W');
        static Asset asset = node.GetAsset("BNCfdmEV8nuEnB66ppqHU3nQXjqkCpieEKiH2w6b2FJG");
        static byte[] senderPublicKey = node.GetTransactionById(asset.Id).SenderPublicKey;
        static string sender = AddressEncoding.GetAddressFromPublicKey(senderPublicKey, node.ChainId);
        static string recipient = "3PFL5threY8a6CUaqUmpSGtYvnncEUc8oKz"; // put your address here

        public static void UpdateComplexity()
        {
            var currentHeight = (long)node.GetHeight();
            var lastUpdateHeight = (long)node.GetAddressData(sender)["lastUpdateHeight"];

            if (currentHeight - lastUpdateHeight >= 1000)
            {
                var currentBalance = asset.AmountToLong(node.GetBalance(sender, asset));
                var lastUpdateBalance = (long)node.GetAddressData(sender)["lastUpdateBalance"];

                var N = (long)node.GetAddressData(sender)["N"];

                var newN = N + (lastUpdateBalance - currentBalance) / 300000 - 1;
                newN = Math.Max(1, newN);
                newN = Math.Min(32, newN);

                try
                {
                    var data = new Dictionary<string, object>
                    {
                        { "N", newN },
                        { "lastUpdateHeight", currentHeight },
                        { "lastUpdateBalance", currentBalance }
                    };

                    var tx = new DataTransaction(senderPublicKey, data, 0.005m);

                    node.Broadcast(tx.GetJsonWithSignature());

                    Console.WriteLine($"Mining complexity was updated");
                    Console.WriteLine($"Current mining complexity is N = {newN}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        [TestMethod]
        public void Main()
        {
            var currentBalance = asset.AmountToLong(node.GetBalance(sender, asset));

            var begin = DateTime.UtcNow;
            Console.WriteLine($"Started mining at {begin.ToString()}");

            var N = (long)node.GetAddressData(sender)["N"];
            Console.WriteLine($"Current mining complexity is N = {N}");

            while (true)
            {
                // UpdateComplexity();
                var transferAmount = currentBalance / 1000000 + 1;

                var tx = new TransferTransaction
                (
                    senderPublicKey: senderPublicKey,
                    recipient: recipient,
                    asset: asset,
                    amount: asset.LongToAmount(transferAmount),
                    fee: 0.005m
                );

                var id = tx.GenerateId();

                if (id.FromBase58().Take((int)N).Count(b => b == 0) == N)
                {
                    Console.WriteLine("\nTransfer transaction was generated!");
                    Console.WriteLine($"Elapsed time = {(DateTime.UtcNow - begin).TotalMilliseconds} ms");
                    Console.WriteLine($"Generated transaction id: {id}");

                    Http.Tracing = true;
                    try
                    {
                        node.Broadcast(tx);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    finally
                    {
                        N = (long)node.GetAddressData(sender)["N"];
                        currentBalance = asset.AmountToLong(node.GetBalance(sender, asset));
                        begin = DateTime.UtcNow;
                        Http.Tracing = false;
                    }
                }
            }
        }
    }
}