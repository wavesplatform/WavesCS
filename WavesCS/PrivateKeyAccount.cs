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
        private static JavaScriptSerializer serializer = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };

        private readonly byte[] privateKey;

        private static List<String> seedWords = null;
        private char scheme;
        private byte[] publicKey;
        private string address;

        public string Address
        {
            get { return address; }
            set { address = value; }
        }

        private PrivateKeyAccount(byte[] privateKey, char scheme)         
        {
            this.scheme = scheme;
            publicKey = GetPublicKeyFromPrivateKey(privateKey);
            address = AddressEncoding.GetAddressFromPublicKey(publicKey, scheme);
            this.privateKey = privateKey;
        }

        public PrivateKeyAccount(byte[] seed, char scheme, int nonce) : this(GeneratePrivateKey(seed, nonce), scheme) { }

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

        public byte[] PrivateKey
        {
            get{ return privateKey.ToArray(); }
        }

        private static byte[] GeneratePrivateKey(byte[] seed, int nonce)
        {
            MemoryStream stream = new MemoryStream(seed.Length + 4);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(nonce);
            writer.Write(seed);            
            byte[] accountSeed = AddressEncoding.SecureHash(stream.ToArray(), 0, stream.ToArray().Length);
            byte[] hashedSeed = SHA256.ComputeHash(accountSeed, 0, accountSeed.Length); 
            byte[] privateKey = new byte[32];
            privateKey = hashedSeed.ToArray();
            privateKey[0] &= 248;
            privateKey[31] &= 127;
            privateKey[31] |= 64;

            return privateKey;
        }

        public override string ToString()
        {
            return  String.Format("Address: {0}, Type: {1}", address, typeof(PrivateKeyAccount));
        }


        private static byte[] GetPublicKeyFromPrivateKey(byte[] privateKey)
        {
            byte[] publicKey = new byte[privateKey.Length];
            Curve_sigs.curve25519_keygen(publicKey, privateKey);
            return publicKey;
        }

        public byte[] PublicKey
        {
            get { return publicKey.ToArray(); }
            set { publicKey = value; }
        }

        /**
     * Generates a 15-word random seed. This method implements the BIP-39 algorithm with 160 bits of entropy.
     * @return the seed as a String
     */
        public static string GenerateSeed()
        {
            byte[] bytes = new byte[160 + 5];
            RandomNumberGenerator random = RandomNumberGenerator.Create();
            random.GetBytes(bytes);
            byte[] rhash = SHA256.ComputeHash(bytes, 0, 160);
            Array.Copy(rhash, 0, bytes, 160, 5);
            BigInteger rand = new BigInteger(bytes);
            if(seedWords == null)
            {
                StreamReader reader = new StreamReader("SeedWords.json");
                string json = reader.ReadToEnd();
                var items = serializer.Deserialize<Dictionary<String, List<String>>>(json);
                seedWords = items["words"];
            }                  
            List<BigInteger> result = new List<BigInteger>();
            for(int i = 0; i < 15; i++)
            {
                result.Add(rand);
                rand = rand >> 11;
            }
            BigInteger mask = new BigInteger(new byte[] { unchecked((byte)-1), 7, 0, 0 }); // 11 lower bits
            return String.Join(" ", result.Select(bigint => seedWords[(int)(bigint & mask)]));         
        }

        public static IEnumerable<T> Iterate<T>(T seed, Func<T, T> unaryOperator)
        {
            while (true)
            {
                yield return seed;
                seed = unaryOperator(seed);
            }
        }
    }
}
