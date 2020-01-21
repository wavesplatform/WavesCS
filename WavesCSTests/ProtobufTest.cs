using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grpc.Core;
using Waves;
using Waves.Node.Grpc;
using WavesCS;

namespace WavesCSTests.Protobuf
{
    [TestClass]
    public class ProtobufTest
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Init()
        {
            Http.Tracing = true;
        }

        [TestMethod]
        public void TestGrpc()
        {
            Channel channel = new Channel("mainnet-aws-fr-1.wavesnodes.com:6871", ChannelCredentials.Insecure);
            BlocksApi.BlocksApiClient client = new BlocksApi.BlocksApiClient(channel);
            BlockWithHeight block = client.GetBlock(new BlockRequest() { Height = -1 });
            TestContext.WriteLine(block.ToString());
        }
    }
}