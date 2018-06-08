using System;
using System.Linq;

namespace WavesCS
{
    public static class Base58
    {
        public static readonly char[] ALPHABET =
            "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz".ToCharArray();

        private static readonly char ENCODED_ZERO = ALPHABET[0];
        private static readonly int[] INDEXES = new int[128];


        static Base58()
        {

            for (int i = 0; i < INDEXES.Length; i++)
            {
                INDEXES[i] = -1;
            }

            for (int i = 0; i < ALPHABET.Length; i++)
            {
                INDEXES[ALPHABET[i]] = i;
            }
        }

        /**
         * Encodes the given bytes as a base58 string (no checksum is appended).
         *
         * @param input the bytes to encode
         * @return the base58-encoded string
         */
        public static string Encode(byte[] input)
        {
            if (input.Length == 0)
            {
                return "";
            }

            // Count leading zeros.
            int zeros = 0;
            while (zeros < input.Length && input[zeros] == 0)
            {
                ++zeros;
            }

            // Convert base-256 digits to base-58 digits (plus conversion to ASCII characters)        
            input = input.ToArray(); // since we modify it in-place
            char[] encoded = new char[input.Length * 2]; // upper bound
            int outputStart = encoded.Length;
            for (int inputStart = zeros; inputStart < input.Length;)
            {
                encoded[--outputStart] = ALPHABET[DivMod(input, inputStart, 256, 58)];
                if (input[inputStart] == 0)
                {
                    ++inputStart; // optimization - skip leading zeros
                }
            }

            // Preserve exactly as many leading encoded zeros in output as there were leading zeros in input.
            while (outputStart < encoded.Length && encoded[outputStart] == ENCODED_ZERO)
            {
                ++outputStart;
            }

            while (--zeros >= 0)
            {
                encoded[--outputStart] = ENCODED_ZERO;
            }

            // Return encoded string (including encoded leading zeros).
            return new String(encoded, outputStart, encoded.Length - outputStart);
        }


        /**
         * Decodes the given base58 string into the original data bytes.
         *
         * @param input the base58-encoded string to decode
         * @return the decoded data bytes
         * @throws IllegalArgumentException if the given string is not a valid base58 string
         */
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

                input58[i] = (byte) digit;
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

            return CopyOfRange(decoded, outputStart - zeros, decoded.Length);
        }

        private static byte[] CopyOfRange(byte[] original, int from, int to)
        {
            int newLength = to - from;
            byte[] copy = new byte[newLength];
            Array.Copy(original, from, copy, 0, Math.Min(original.Length - from, newLength));
            return copy;
        }

        private static byte DivMod(byte[] number, int firstDigit, int encodingBase, int divisor)
        {
            // this is just long division which accounts for the base of the input digits
            int remainder = 0;
            for (int i = firstDigit; i < number.Length; i++)
            {
                int digit = (int) number[i] & 0xFF;
                int temp = remainder * encodingBase + digit;
                number[i] = (byte) (temp / divisor);
                remainder = temp % divisor;
            }

            return (byte) remainder;
        }


        public static string ToBase58(this byte[] data)
        {
            return Encode(data);
        }

        public static byte[] FromBase58(this string data)
        {
            return Decode(data);
        }
    }
}

