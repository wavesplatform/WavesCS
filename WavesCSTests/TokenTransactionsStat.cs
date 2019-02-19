using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS;

namespace WavesCSTests
{
    [TestClass]
    public class TokenTransactionsStat
    {
        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }

        [TestMethod]
        public void TestGetTransactionsByHeight()
        {
            var node = new Node(Node.MainNetHost);
           // var transactions = node.GetTransactionsByAddress(Accounts.Alice.Address, 10);
            int currentHeight = node.GetHeight();
            int count = 0;            
            for (int i = 1340000; i <= currentHeight; i++)
            {
               var tsx = node.GetBlockTransactionsAtHeight(i);
                if (tsx != null)
                {
                    foreach (Transaction tx in tsx)
                    {
                        if (tx.GetType().Name == "IssueTransaction")
                        {
                            IssueTransaction itx = (IssueTransaction)tx;
                            if (itx.Version == 2 && itx.Script != null)
                                count++;
                        }
                    }                    
                }
                Thread.Sleep(1000);
            }
            Console.WriteLine(count);
        }
    }
}
