using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace WavesCS
{
    public static class Utils
    {
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
        
        public static void WriteLong(this BinaryWriter writer, long n)
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
            byte[] shortN = BitConverter.GetBytes((short) n);
            Array.Reverse(shortN);
            writer.Write(shortN);          
        }

        public static long CurrentTimestamp()
        {
            long epochTicks = new DateTime(1970, 1, 1).Ticks;
            return (DateTime.UtcNow.Ticks - epochTicks) / TimeSpan.TicksPerSecond * 1000;
        }

        public static string ToJson(this Dictionary<string, object> data)
        {
            return Serializer.Serialize(data);
        }
    }
}
