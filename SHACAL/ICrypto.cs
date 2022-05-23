using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHACAL
{
    public interface ICrypto
    {
        public byte[] Encrypt(byte[] data);

        public byte[] Decrypt(byte[] data);
    }
}
