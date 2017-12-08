using System;
using System.Linq;
using System.Numerics;

namespace WavesCS.Main
{
    public class Base58
    {
        public static readonly char[] ALPHABET = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz".ToCharArray();
        private static readonly char ENCODED_ZERO = ALPHABET[0];
        private static readonly int[] INDEXES = new int[128];

        static Base58()
        {
            int j = 0;
            Array.Clear(INDEXES, 0, INDEXES.Length);                        
            for (int i = 0; i < ALPHABET.Length; i++)
            {
                INDEXES[ALPHABET[i]] = i;
                j++;
            }
        }

        public static string Encode(byte[] input)
        {
            if (input.Length == 0)
            {
                return "";
            }
            int zerosCount = 0;
            while (zerosCount < input.Length && input[zerosCount] == 0)
            {
                ++zerosCount;
            }
            input = input.ToArray();
            char[] encoded = new char[input.Length * 2];
            int outputStart = encoded.Length;
            for (int inputStart = zerosCount; inputStart < input.Length;)
            {
                encoded[--outputStart] = ALPHABET[DivMod(input, inputStart, 256, 58)];
                if (input[inputStart] == 0)
                {
                    ++inputStart;
                }
            }

            while (outputStart < encoded.Length && encoded[outputStart] == ENCODED_ZERO)
            {
                ++outputStart;
            }

            while (--zerosCount >= 0)
            {
                encoded[--outputStart] = ENCODED_ZERO;
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
            try{
                return new BigInteger(Decode(input));
            }
            catch(ArgumentException)
            {
                throw;
            }
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
                char c = input[i];
                int digit = c < 128 ? INDEXES[c] : -1;
                if (digit < 0)
                {
                    throw new ArgumentException("Illegal character " + c + " at position " + i);
                }
                input58[i] = (byte)digit;
            }
            // Count leading zeros.
            int zeros = 0;
            while (zeros < input58.Length && input58[zeros] == 0)
            {
                ++zeros;
            }
            // Convert base-58 digits to base-256 digits.
            byte[] decoded = new byte[input.Length];
            int outputStart = decoded.Length;
            for (int inputStart = zeros; inputStart < input58.Length;)
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
            byte[] result = new byte[decoded.Length - outputStart + zeros];
            
            Array.Copy(decoded, outputStart - zeros, result, 0, decoded.Length - outputStart + zeros);
            return result;
        }
    }
}

