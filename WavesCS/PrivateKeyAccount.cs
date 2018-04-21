using System.Text;
using System;
using System.Linq;
using System.IO;
using org.whispersystems.curve25519.csharp;
using System.Security.Cryptography;
using System.Numerics;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace WavesCS
{
    public class PrivateKeyAccount
    {
        private static readonly SHA256Managed SHA256 = new SHA256Managed();
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };

                
        private readonly byte[] _privateKey;
        private readonly byte[] _publicKey;

        public byte[] PrivateKey => _privateKey.ToArray();
        public byte[] PublicKey => _publicKey.ToArray();

        private static List<string> _seedWords;

        public string Address { get; }

        private PrivateKeyAccount(byte[] privateKey, char scheme)         
        {
            _publicKey = GetPublicKeyFromPrivateKey(privateKey);
            Address = AddressEncoding.GetAddressFromPublicKey(PublicKey, scheme);
            _privateKey = privateKey;
        }

        private PrivateKeyAccount(byte[] seed, char scheme, int nonce) : this(GeneratePrivateKey(seed, nonce), scheme) { }
        private PrivateKeyAccount(string privateKey, char scheme) : this(Base58.Decode(privateKey), scheme) { }

        public static PrivateKeyAccount CreateFromSeed(string seed, char scheme, int nonce = 0)
        {
            return new PrivateKeyAccount(Encoding.UTF8.GetBytes(seed), scheme, nonce);
        }

        public static PrivateKeyAccount CreateFromSeed(byte[] seed, char scheme, int nonce = 0)
        {
            return new PrivateKeyAccount(seed, scheme, nonce);
        }

        public static PrivateKeyAccount CreateFromPrivateKey(string privateKey, char scheme)
        {
            return new PrivateKeyAccount(privateKey, scheme);
        }        

        private static byte[] GeneratePrivateKey(byte[] seed, int nonce)
        {
            var stream = new MemoryStream(seed.Length + 4);
            var writer = new BinaryWriter(stream);
            writer.Write(nonce);
            writer.Write(seed);            
            var accountSeed = AddressEncoding.SecureHash(stream.ToArray(), 0, stream.ToArray().Length);
            var hashedSeed = SHA256.ComputeHash(accountSeed, 0, accountSeed.Length);             
            var privateKey = hashedSeed.ToArray();
            privateKey[0] &= 248;
            privateKey[31] &= 127;
            privateKey[31] |= 64;

            return privateKey;
        }

        public override string ToString()
        {
            return $"Address: {Address}, Type: {typeof(PrivateKeyAccount)}";
        }

        private static byte[] GetPublicKeyFromPrivateKey(byte[] privateKey)
        {
            var publicKey = new byte[privateKey.Length];
            Curve_sigs.curve25519_keygen(publicKey, privateKey);
            return publicKey;
        }        

        public static string GenerateSeed()
        {
            var bytes = new byte[160 + 5];
            var generator = RandomNumberGenerator.Create();
            generator.GetBytes(bytes);
            var rhash = SHA256.ComputeHash(bytes, 0, 160);
            Array.Copy(rhash, 0, bytes, 160, 5);
            var rand = new BigInteger(bytes);
            if (_seedWords == null)
            {
                var reader = new StreamReader("SeedWords.json");
                var json = reader.ReadToEnd();
                var items = Serializer.Deserialize<Dictionary<string, List<string>>>(json);
                _seedWords = items["words"];
            }
            var result = new List<BigInteger>();
            for (int i = 0; i < 15; i++)
            {
                result.Add(rand);
                rand = rand >> 11;
            }                       
            var mask = new BigInteger(new byte[] { 255, 7, 0, 0 }); // 11 lower bits
            return string.Join(" ", result.Select(bigint => _seedWords[ (int) (bigint & mask)]));         
        }
    }
}
