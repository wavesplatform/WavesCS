using System;
using System.Linq;
using HashLib;
using System.IO;

namespace WavesCS.Main
{
    public class PublicKeyAccount: Account
    {
        private char scheme;
        private byte[] publicKey;
        private string address;
        
        private static readonly IHash KECCAK256 = HashFactory.Crypto.SHA3.CreateKeccak256();

        public PublicKeyAccount(byte[] publicKey, char scheme)
        {
            this.scheme = scheme;
            this.publicKey = publicKey;
            this.address = Base58.Encode(GenerateAddress(publicKey, scheme));
        }

        public PublicKeyAccount(String publicKey, char scheme) : this(Base58.Decode(publicKey), scheme)
        {
        }

        public byte[] PublicKey
        {
            get { return publicKey.ToArray(); }
            set { publicKey = value; }
        }

        public string Address
        {
            get { return address; }
        }

        public char Scheme
        {
            get { return scheme; }
        }

        protected static byte[] Hash(byte[] message, int offset, int lenght, IHash algorithm)
        {
            algorithm.Initialize();            
            algorithm.TransformBytes(message, offset, lenght);
            HashResult result = algorithm.TransformFinal();            
            return result.GetBytes(); 
        }

        protected static byte[] SecureHash(byte[] message, int offset, int lenght)
        {
            Blake2Sharp.Blake2BConfig config = new Blake2Sharp.Blake2BConfig();
            config.OutputSizeInBits = 256;
            byte[] blake2b = Blake2Sharp.Blake2B.ComputeHash(message, offset, lenght, config);
            return Hash(blake2b, 0, blake2b.Length, KECCAK256);
        }

        private static byte[] GenerateAddress(byte[] publicKey, char scheme) 
        {
            MemoryStream stream = new MemoryStream(26);
            byte[] hash = SecureHash(publicKey, 0, publicKey.Length);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((byte)1);
            writer.Write((byte)scheme);
            writer.Write(hash, 0, 20);
            byte[] checksum = SecureHash(stream.ToArray(), 0, 22);
            writer.Write(checksum, 0, 4);
            return stream.ToArray();
        }
    }
}
