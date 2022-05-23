using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHACAL
{

    internal class KeyGenerator
    {
        private byte[] _key;
        public KeyGenerator(byte[] key)
        {
            _key = key;
        }

        public List<UInt32> GetKeys()//512 BIT!!
        {
            List<UInt32> keys=new List<UInt32>();
            for(int i=0; i < _key.Length; i+=4)
            {
                UInt32 nextKey =  BitConverter.ToUInt32(_key, i);
                keys.Add(nextKey);
            }  
            for(int i = 16; i < 80; i++)
            {
                UInt32 nextKey = ((keys[i - 3] ^ keys[i - 8] ^ keys[i - 14] ^ keys[i - 16]) << 1);
                keys.Add(nextKey);
            }
            return keys;
        }
    }
}
