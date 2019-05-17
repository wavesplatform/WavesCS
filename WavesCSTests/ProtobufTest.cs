using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grpc.Core;
using Waves.Protobuf;
using WavesCS;
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
        public async Task TestGrpc()
        {
            Channel channel = new Channel("mainnet-aws-fr-3.wavesnodes.com:6870", ChannelCredentials.Insecure);
            //TransactionsApi.TransactionsApiClient client = new TransactionsApi.TransactionsApiClient(channel);
            BlocksApi.BlocksApiClient client = new BlocksApi.BlocksApiClient(channel);
            BlockWithHeight block = client.GetBlock(new BlockRequest() { Height = -1 });
            TestContext.WriteLine(block.ToString());
            // {
            //    Recipient = new Recipient() { Address = Google.Protobuf.ByteString.CopyFrom(Base58.Decode("3N1atv1SuhTC3sQwqrR6BkK2PNwrC8LKLqp")) },
            //    Sender = Google.Protobuf.ByteString.CopyFrom(Base58.Decode("3N8S8VUbDJuqtUP8wAraWhMMeqcYHH5EWcF"))
            //}).ResponseStream;
            //int i = 0;
            //while (await txs.MoveNext() && i < 10)
            //{
            //    TestContext.WriteLine(txs.Current.ToString());
            //    i++;
            //}
        }
    }
}
