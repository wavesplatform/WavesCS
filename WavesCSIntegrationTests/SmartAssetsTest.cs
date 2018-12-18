using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCSIntegrationTests
{

    [TestClass]
    public class SmartAssetsTest
    {
        PrivateKeyAccount Alice = PrivateKeyAccount.CreateFromSeed("seed4Alice", 'T');

        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }

        [TestMethod]
        public void TestIssueFrozenAsset()
        {
            var node = new Node();

            var script = @"let targetHeight = 420000
let issuer = Address(base58'3N8S8VUbDJuqtUP8wAraWhMMeqcYHH5EWcF')
tx.sender == issuer || height > targetHeight";

            Asset smartAsset = node.IssueAsset(Alice, "FrozenAsset",
                                               "FrozenAsset is a sample smart asset. This asset cannot be traded until the blockchain height becomes more than the target height. The detailed info can be found on https://sway.office.com/l8LfbtS0XRnCImuN?ref=Link",
                                               100000m, 8,
                                               true, node.CompileScript(script));
            Thread.Sleep(3000);
            Assert.IsNotNull(smartAsset);
        }

        [TestMethod]
        public void TestIssueIntervalAsset()
        {
            var node = new Node();

            var script = @"let startHeight = 0
let interval = 10000
let limit = 1000
match tx {
    case t: TransferTransaction | MassTransferTransaction | ExchangeTransaction =>
        (height - startHeight) % interval < limit
    case _ => true
}";
            var compiledScript = node.CompileScript(script);
            Asset smartAsset = node.IssueAsset(Alice, "IntervalAsset",
                                               "IntervalAsset is a sample smart asset that can only be transferred or traded during the certain intervals. The trading starts every 10 000 blocks and lasts for 1000 blocks. During the next 9000 blocks, the asset cannot be traded, and so on. The detailed info can be found on https://sway.office.com/61KiYmHfMQW22J4n?ref=Link",
                                               100000m, 8,
                                               true, compiledScript);
            Thread.Sleep(3000);
            Assert.IsNotNull(smartAsset);
        }
    }
}