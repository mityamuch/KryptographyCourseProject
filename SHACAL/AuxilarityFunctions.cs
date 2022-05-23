using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHACAL
{
    internal static  class AuxilarityFunctions
    {
        public static UInt32 ShiftL(UInt32 a, int b) 
        {
            return (a << b) | (a >> (32 - b));
        }

        public static UInt32 ShiftR(UInt32 a, int b)
        {
            return (a >> b) | (a <<( 32 - b));
        }


        public static UInt32 F(int i,UInt32 x, UInt32 y, UInt32 z)
        {
            if (i < 20)
            {
                return ((x & y) | (~x & z));
            }
            if (i < 40)
                return x ^ y ^ z;
            if (i < 60)
                return (x ^ y) | (x ^ z) | (y ^ z);
            return x ^ y ^ z;
        }


        public static byte[] XOR(byte[] part1, byte[] part2)
        {
            var size = Math.Min(part1.Length, part2.Length);
            byte[] result = new byte[size];
            for (int i = 0; i < size; i++)
            {
                result[i] = (byte)(part1[i] ^ part2[i]);
            }
            return result;
        }

    }
}
