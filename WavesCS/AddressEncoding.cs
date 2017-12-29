using HashLib;
using System.IO;

namespace WavesCS
{
    public static class AddressEncoding
    {
        public static char MainNet = 'W';
        public static char TestNet = 'T';

        private static readonly IHash Keccak256 = HashFactory.Crypto.SHA3.CreateKeccak256();

        public static byte[] Hash(byte[] message, int offset, int lenght, IHash algorithm)
        {
            algorithm.Initialize();
            algorithm.TransformBytes(message, offset, lenght);
            HashResult result = algorithm.TransformFinal();
            return result.GetBytes();
        }

        public static byte[] SecureHash(byte[] message, int offset, int lenght)
        {
            Blake2Sharp.Blake2BConfig config = new Blake2Sharp.Blake2BConfig();
            config.OutputSizeInBits = 256;
            byte[] blake2b = Blake2Sharp.Blake2B.ComputeHash(message, offset, lenght, config);
            return Hash(blake2b, 0, blake2b.Length, Keccak256);
        }

        public static string GetAddressFromPublicKey(byte[] publicKey, char scheme)
        {
            MemoryStream stream = new MemoryStream(26);
            byte[] hash = SecureHash(publicKey, 0, publicKey.Length);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((byte)1);
            writer.Write((byte)scheme);
            writer.Write(hash, 0, 20);
            byte[] checksum = SecureHash(stream.ToArray(), 0, 22);
            writer.Write(checksum, 0, 4);
            return Base58.Encode(stream.ToArray());
        }        
    }
}
