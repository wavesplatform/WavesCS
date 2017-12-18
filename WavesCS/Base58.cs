using System;
using System.Linq;
using System.Numerics;

namespace WavesCS
{
    public class Base58
    {
        public static readonly char[] Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz".ToCharArray();
        private static readonly char EncodedZero = Alphabet[0];
        private static readonly int[] Indexes = new int[128];

        static Base58()
        {
            Array.Clear(Indexes, 0, Indexes.Length);                        
            for (int i = 0; i < Alphabet.Length; i++)
            {
                Indexes[Alphabet[i]] = i;                
            }
        }

        public static string Encode(byte[] input)
        {
            if (input.Length == 0)
            {
                return "";
            }
            int leadingZerosCount = input.ToList().TakeWhile(oneByte => oneByte < input.Length && input[oneByte] == 0).Count();          
            input = input.ToArray();
            char[] encoded = new char[input.Length * 2];
            int outputStart = encoded.Length;
            for (int inputStart = leadingZerosCount; inputStart < input.Length;)
            {
                encoded[--outputStart] = Alphabet[DivMod(input, inputStart, 256, 58)];
                if (input[inputStart] == 0)
                {
                    ++inputStart;
                }
            }

            while (outputStart < encoded.Length && encoded[outputStart] == EncodedZero)
            {
                ++outputStart;
            }

            while (--leadingZerosCount >= 0)
            {
                encoded[--outputStart] = EncodedZero;
            }
            return new String(encoded, outputStart, encoded.Length - outputStart);
        }

        private static byte DivMod(byte[] number, int firstDigit, int base_of_representation, int divisor)
        {
            int remainder = 0;
            for (int i = firstDigit; i < number.Length; i++)
            {
                int digit = number[i] & 0xFF;
                int temp = remainder * base_of_representation + digit;
                number[i] = (byte)(temp / divisor);
                remainder = temp % divisor;
            }
            return (byte)remainder;
        }

        public static BigInteger DecodeToBigInteger(String input)
        {
            return new BigInteger(Decode(input));
        }

        public static byte[] Decode(string input)
        {
            if (input.Length == 0)
            {
                return new byte[0];
            }
            // Convert the base58-encoded ASCII chars to a base58 byte sequence (base58 digits).
            byte[] input58 = new byte[input.Length];
            for (int i = 0; i < input.Length; ++i)
            {
                char symbol = input[i];
                int digit = symbol < 128 ? Indexes[symbol] : -1;
                if (digit < 0)
                {
                    throw new ArgumentException("Illegal character " + symbol + " at position " + i);
                }
                input58[i] = (byte)digit;
            }
            int leadingZerosCount = input58.ToList().TakeWhile(oneByte => oneByte < input58.Length && input58[oneByte] == 0).Count();
            // Convert base-58 digits to base-256 digits.
            byte[] decoded = new byte[input.Length];
            int outputStart = decoded.Length;
            for (int inputStart = leadingZerosCount; inputStart < input58.Length;)
            {
                decoded[--outputStart] = DivMod(input58, inputStart, 58, 256);
                if (input58[inputStart] == 0)
                {
                    ++inputStart; // optimization - skip leading zeros
                }
            }
            // Ignore extra leading zeroes that were added during the calculation.
            while (outputStart < decoded.Length && decoded[outputStart] == 0)
            {
                ++outputStart;
            }
            // Return decoded data (including original number of leading zeros).
            byte[] result = new byte[decoded.Length - outputStart + leadingZerosCount];
            
            Array.Copy(decoded, outputStart - leadingZerosCount, result, 0, decoded.Length - outputStart + leadingZerosCount);
            return result;
        }
    }
}

