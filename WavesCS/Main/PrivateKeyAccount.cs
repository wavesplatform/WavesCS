using System.Text;
using System.Linq;
using System.IO;
using org.whispersystems.curve25519.csharp;
using System.Security.Cryptography;

namespace WavesCS.Main
{
    public class PrivateKeyAccount: PublicKeyAccount
    {
        private static readonly SHA256Managed SHA256 = new SHA256Managed();

        private readonly byte[] privateKey;

        private PrivateKeyAccount(byte[] privateKey, char scheme) :
            base(PublicKey(privateKey), scheme)
        {
            this.privateKey = privateKey;
        }

        public PrivateKeyAccount(string seed, int nonce, char scheme) : this(GeneratePrivateKey(seed, nonce), scheme) { }

        public PrivateKeyAccount(string privateKey, char scheme) : this(Base58.Decode(privateKey), scheme) { }

        public byte[] PrivateKey
        {
            get{ return privateKey.ToArray(); }
        }

        private static byte[] GeneratePrivateKey(string seed, int nonce) //TODO(tonya): rename to address, and address rename to smth else
        {
            MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(seed).Length + 4);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(nonce);
            writer.Write(Encoding.Default.GetBytes(seed));
            byte[] accountSeed = SecureHash(stream.ToArray(), 0, stream.ToArray().Length);            
            byte[] hashedSeed = SHA256.ComputeHash(accountSeed, 0, accountSeed.Length);
            byte[] privateKey = new byte[32];
            privateKey = hashedSeed.ToArray();
            privateKey[0] &= 248;
            privateKey[31] &= 127;
            privateKey[31] |= 64;

            return privateKey;
        }

        new private static byte[] PublicKey(byte[] privateKey)
        {
            byte[] publicKey = new byte[privateKey.Length];
            Curve_sigs.curve25519_keygen(publicKey, privateKey);
            return publicKey;
        }
       
    }
}
