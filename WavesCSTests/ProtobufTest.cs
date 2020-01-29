using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grpc.Core;
using Waves;
using Waves.Node.Grpc;
using WavesCS;
using Google.Protobuf;
using System.Threading.Tasks;

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
        public void TestGrpcBlocksApiClient()
        {
            Channel channel = new Channel("mainnet-aws-fr-1.wavesnodes.com:6871", ChannelCredentials.Insecure);
            BlocksApi.BlocksApiClient client = new BlocksApi.BlocksApiClient(channel);
            BlockWithHeight block = client.GetBlock(new BlockRequest() { Height = -1 });
            TestContext.WriteLine(block.ToString());
        }

        [TestMethod]
        public void TestGrpcTransactionsApiClient()
        {
            Channel channel = new Channel("mainnet-aws-fr-1.wavesnodes.com:6871", ChannelCredentials.Insecure);
            
            var client = new TransactionsApi.TransactionsApiClient(channel);

            var txId = "76TxtthzU6YxLMAEVpPj7RJtJcXAG8FCRvDZy4KR4zW2";
            var request = new TransactionsRequest()
            {
                TransactionIds = { ByteString.CopyFrom(txId.FromBase58()) }
            };

            var t = client.GetTransactions(request);
            var task = Task.Run(async () => { await t.ResponseStream.MoveNext(); });
            task.Wait();

            TestContext.WriteLine(t.ResponseStream.Current.ToString());

        }


    }
}