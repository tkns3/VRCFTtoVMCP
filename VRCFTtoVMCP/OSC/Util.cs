using System.IO;

namespace VRCFTtoVMCP.Osc
{
    internal static class Util
    {
        public static bool IsMultipleOfFour(int num)
        {
            return num == (num & ~0x3);
        }

        public static int GetStringAlignedSize(int size)
        {
            return (size + 4) & ~0x3;
        }

        public static int GetBufferAlignedSize(int size)
        {
            var offset = size & ~0x3;
            return (offset == size) ? size : (offset + 4);
        }

        public static string GetString(this object value)
        {
            switch (value)
            {
                case int i: return i.ToString();
                case float f: return f.ToString();
                case string s: return s;
                case byte[] bytes: return "Byte[" + bytes.Length + "]";
                default: return value.ToString() ?? "";
            };
        }

        public static byte[] GetBuffer(MemoryStream stream)
        {
            return stream.GetBuffer();
        }
    }
}
