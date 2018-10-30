using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WavesCS;

namespace WavesCSTests
{
    [TestClass]
    public class SmartAssetsTest
    {
        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }

        [TestMethod]
        public void TestIssueSmartAsset()
        {
            var node = new Node();

            var script = "true";
            var compiledScript = node.CompileScript(script);


            Asset asset = node.IssueAsset(Accounts.Alice, "testSmartAsset",
                                          "Smart Asset for c# testing", 2, 6,
                                          true, compiledScript);
            Assert.IsNotNull(asset);

            Thread.Sleep(15000);

            var quantity = node.GetBalance(Accounts.Alice.Address, asset);
            Assert.AreEqual(quantity, 2);
        }


        [TestMethod]
        public void TestSetAssetScript()
        {
            var node = new Node();

        }
    }
}