using System;
using System.IO;
using System.Text;

namespace WavesCS
{
    public static class Utils
    {               
        public static void WriteLong(this BinaryWriter writer, long n)
        {
            n = System.Net.IPAddress.HostToNetworkOrder(n);
            writer.Write(n);            
        }

        public static void WriteInt(this BinaryWriter writer, int n)
        {
            n = System.Net.IPAddress.HostToNetworkOrder(n);
            writer.Write(n);
        }

        public static void WriteByte(this BinaryWriter writer, byte n)
        {
            writer.Write(n);
        }

        public static void Write(this BinaryWriter writer, TransactionType n)
        {            
            writer.Write((byte) n);
        }

        public static void WriteShort(this BinaryWriter writer, int n)
        {
            byte[] shortN = BitConverter.GetBytes((short)n);
            Array.Reverse(shortN);
            writer.Write(shortN);
        }

        public static void WriteObject(this BinaryWriter writer, object o)
        {
            const byte INTEGER = 0;
            const byte BOOLEAN = 1;
            const byte BINARY = 2;
            const byte STRING = 3;

            switch (o)
            {
                case long value:
                    writer.Write(INTEGER);
                    writer.WriteLong(value);
                    break;
                case bool value:
                    writer.Write(BOOLEAN);
                    writer.Write(value ? (byte)1 : (byte)0);
                    break;
                case byte[] value:
                    writer.Write(BINARY);
                    writer.WriteShort((short)value.Length);
                    writer.Write(value);
                    break;
                case string value:
                    writer.Write(STRING);
                    var encoded = Encoding.UTF8.GetBytes(value);
                    writer.WriteShort((short)encoded.Length);
                    writer.Write(encoded);
                    break;
                default:
                    throw new ArgumentException("Only long, bool and byte[] entry values supported");
            }
        }

        public static long CurrentTimestamp()
        {
            long epochTicks = new DateTime(1970, 1, 1).Ticks;
            return (DateTime.UtcNow.Ticks - epochTicks) / (TimeSpan.TicksPerSecond / 1000);
        }
        
        public static long ToLong(this DateTime date)
        {
            return (date - new DateTime(1970, 1, 1)).Ticks / (TimeSpan.TicksPerSecond / 1000);             
        }

        public static string ToBase64(this byte[] data)
        {
            return "base64:" + Convert.ToBase64String(data);
        }
        
        public static byte[] FromBase64(this string data)
        {
            if (data.StartsWith("base64:"))
                data = data.Substring(7);                
            return Convert.FromBase64CharArray(data.ToCharArray(), 0, data.Length);
        }
        
        public static void WriteAsset(this BinaryWriter stream, string assetId)
        {
            if (string.IsNullOrEmpty(assetId) || assetId == "WAVES")
            {
                stream.WriteByte(0);
            }
            else
            {
                stream.WriteByte(1);
                var decoded = Base58.Decode(assetId);
                stream.Write(decoded, 0, decoded.Length);
            }
        }
    }
}
