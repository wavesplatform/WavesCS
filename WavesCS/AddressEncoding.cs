using HashLib;
using System.IO;
using Blake2Sharp;

namespace WavesCS
{
    public static class AddressEncoding
    {
        public static char MainNet = 'W';
        public static char TestNet = 'T';

        private static readonly IHash Keccak256 = HashFactory.Crypto.SHA3.CreateKeccak256();

        private static byte[] Hash(byte[] message, int offset, int length, IHash algorithm)
        {            
            algorithm.Initialize();
            algorithm.TransformBytes(message, offset, length);
            return algorithm.TransformFinal().GetBytes();
        }

        public static byte[] FastHash(byte[] message, int offset, int length)
        {
            var blakeConfig = new Blake2BConfig { OutputSizeInBits = 256 };
            return Blake2B.ComputeHash(message, offset, length, blakeConfig);
        }

        public static byte[] SecureHash(byte[] message, int offset, int length)
        {
            var blake2B = FastHash(message, offset, length);
            return Hash(blake2B, 0, blake2B.Length, Keccak256);
        }

        public static string GetAddressFromPublicKey(byte[] publicKey, char scheme)
        {
            var stream = new MemoryStream(26);
            var hash = SecureHash(publicKey, 0, publicKey.Length);
            var writer = new BinaryWriter(stream);
            writer.Write((byte)1);
            writer.Write((byte)scheme);
            writer.Write(hash, 0, 20);
            var checksum = SecureHash(stream.ToArray(), 0, 22);
            writer.Write(checksum, 0, 4);
            return Base58.Encode(stream.ToArray());
        }
    }
}
