using System;
using System.Collections.Generic;
using System.Linq;


namespace SHACAL
{
    public sealed class ShacalService:ICrypto
    {
        private KeyGenerator _keyGenerator;
        private List<UInt32> _keys;

        public ShacalService(byte[] key)
        {
            _keyGenerator=new KeyGenerator(key);
            _keys = _keyGenerator.GetKeys();
        }

        public byte[] Encrypt(byte[] indata)//160 BIT!!
        {
            UInt32 A=BitConverter.ToUInt32(indata,0);
            UInt32 B=BitConverter.ToUInt32(indata, 4);
            UInt32 C= BitConverter.ToUInt32(indata, 8);
            UInt32 D= BitConverter.ToUInt32(indata, 12);
            UInt32 E= BitConverter.ToUInt32(indata, 16);

            for(int i = 0; i < 80; i++)
            {
                UInt32 AI = _keys[i] + (AuxilarityFunctions.ShiftL(A, 5)) + AuxilarityFunctions.F(i, B, C, D) + E + Constants.Kconst[i / 20];
                UInt32 BI = A;
                UInt32 CI = AuxilarityFunctions.ShiftL( B , 30);
                UInt32 DI = C;
                UInt32 EI = D;
                (A, B, C, D, E) = (AI, BI, CI, DI,EI); 
            }

            return BitConverter.GetBytes(A)
                .Concat(BitConverter.GetBytes(B))
                .Concat(BitConverter.GetBytes(C))
                .Concat(BitConverter.GetBytes(D))
                .Concat(BitConverter.GetBytes(E))
                .ToArray();
        }

        public byte[] Decrypt(byte[] indata)//160
        {
            UInt32 A = BitConverter.ToUInt32(indata, 0);
            UInt32 B = BitConverter.ToUInt32(indata, 4);
            UInt32 C = BitConverter.ToUInt32(indata, 8);
            UInt32 D = BitConverter.ToUInt32(indata, 12);
            UInt32 E = BitConverter.ToUInt32(indata, 16);

            for (int i = 79; i >=0; i--)
            {
                UInt32 AI = B;
                UInt32 BI = AuxilarityFunctions.ShiftR(C, 30);
                UInt32 CI = D;
                UInt32 DI = E;
                UInt32 EI = A + (~AuxilarityFunctions.ShiftL(B, 5)) +
                    (~AuxilarityFunctions.F(i, AuxilarityFunctions.ShiftR(C, 30), D, E)) +
                    (~Constants.Kconst[i / 20]) + (~_keys[i]) + 4;

                (A, B, C, D, E) = (AI, BI, CI, DI, EI);
            }

            return BitConverter.GetBytes(A)
                .Concat(BitConverter.GetBytes(B))
                .Concat(BitConverter.GetBytes(C))
                .Concat(BitConverter.GetBytes(D))
                .Concat(BitConverter.GetBytes(E))
                .ToArray();
        }
    }
}
