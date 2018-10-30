using System.Text;
using System;
using System.Linq;
using System.IO;
using org.whispersystems.curve25519.csharp;
using System.Security.Cryptography;
using System.Numerics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;
using org.whispersystems.curve25519;

namespace WavesCS
{
    public class PrivateKeyAccount
    {
        private static readonly SHA256Managed SHA256 = new SHA256Managed();

        private static readonly Curve25519 Cipher = Curve25519.getInstance(Curve25519.BEST);
                
        private readonly byte[] _privateKey;
        private readonly byte[] _publicKey;

        public byte[] PrivateKey => _privateKey.ToArray();
        public byte[] PublicKey => _publicKey.ToArray();

        private static List<string> _seedWords;

        public string Address { get; }

        private PrivateKeyAccount(byte[] privateKey, char chainId)         
        {
            _publicKey = GetPublicKeyFromPrivateKey(privateKey);
            Address = AddressEncoding.GetAddressFromPublicKey(PublicKey, chainId);
            _privateKey = privateKey;
        }

        private PrivateKeyAccount(byte[] seed, char chainId, int nonce) : this(GeneratePrivateKey(seed, nonce), chainId) { }
        private PrivateKeyAccount(string privateKey, char chainId) : this(Base58.Decode(privateKey), chainId) { }

        public static PrivateKeyAccount CreateFromSeed(string seed, char chainId, int nonce = 0)
        {
            return new PrivateKeyAccount(Encoding.UTF8.GetBytes(seed), chainId, nonce);
        }

        public static PrivateKeyAccount CreateFromSeed(byte[] seed, char chainId, int nonce = 0)
        {
            return new PrivateKeyAccount(seed, chainId, nonce);
        }

        public static PrivateKeyAccount CreateFromPrivateKey(string privateKey, char chainId)
        {
            return new PrivateKeyAccount(privateKey, chainId);
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
                var assembly = Assembly.GetExecutingAssembly();                               
                using (var stream = assembly.GetManifestResourceStream("WavesCS.Resources.SeedWords.txt"))
                using (var reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    var items = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
                    _seedWords = items["words"];
                }
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

        public byte[] Sign(byte[] data)
        {            
            return Cipher.calculateSignature(PrivateKey, data);
        }
        
        public byte[] Sign(MemoryStream stream)
        {
            return Sign(stream.ToArray());
        }
    }
}
