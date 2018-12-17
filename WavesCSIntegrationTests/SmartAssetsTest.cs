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
        PrivateKeyAccount Alice = PrivateKeyAccount.CreateFromSeed("seed 1234fu4g6j 5htiyr9te23", 'T');

        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }

        [TestMethod]
        public void Test1()
        {
            var node = new Node();

            var script = @"let targetHeight = 1500000
                            height >= targetHeight";

            var asset = node.IssueAsset(Alice, "freezedAsset",
                "This asset is freezed till the height 1500000",
                100000000m, 4, true, node.CompileScript(script));

            Thread.Sleep(3000);

            Assert.IsNotNull(asset);



        }
    }
}