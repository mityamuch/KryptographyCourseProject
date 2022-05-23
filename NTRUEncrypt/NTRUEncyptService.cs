using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace NTRUEncrypt
{
    


    public class NTRUEncyptService
    {
        private static int[] MsgToCoef(byte[] msg)
        {
            int[] coef = new int[Params.N];
            for (int i = 0; i < msg.Length * 8; i++)
                coef[i] = (msg[i / 8] >> (i % 8)) & 1;
            coef[msg.Length * 8] = 1;
            return coef;
        }

        private static byte[] CoefToMsg(int[] coef)
        {
            int msgLen = Array.FindLastIndex(coef, x => x == 1);
            Debug.Assert(msgLen % 8 == 0);
            byte[] msg = new byte[msgLen / 8];
            for (int i = 0; i < msgLen; i++)
                msg[i/8] |= (byte)(coef[i] << (i % 8));
            return msg;
        }

        public static byte[] ConvModQToBytes(ConvModQ e)
        {
            int bitsPerCoef = (int)Math.Ceiling(Math.Log2(Params.q));
            byte[] res = new byte[(int)Math.Ceiling(e.Coef.Length * bitsPerCoef / 8.0)];
            for (int i = 0; i < e.Coef.Length * bitsPerCoef; ++i)
                res[i / 8] |= (byte)(((e.Coef[i / bitsPerCoef] >> (i % bitsPerCoef)) & 1) << (i % 8));
            return res;
        }

        public static ConvModQ BytesToConvModQ(byte[] e)
        {
            int bitsPerCoef = (int)Math.Ceiling(Math.Log2(Params.q));
            int[] coef = new int[e.Length * 8 / bitsPerCoef];
            for (int i = 0; i < coef.Length * bitsPerCoef; ++i)
                coef[i / bitsPerCoef] |= ((e[i / 8] >> (i % 8)) & 1) << (i % bitsPerCoef);
            return new ConvModQ(coef, Params.q, Params.N);
        }

        public static byte[] Encrypt(ConvModQ pub, byte[] msg)
        {
            if (msg.Length > Params.N / 8)
                throw new Exception("Too long msg");
            ConvModQ r = ConvModQ.RandomTrinary(Params.dr, Params.dr);
            ConvModQ m = new ConvModQ(MsgToCoef(msg), Params.q, Params.N);
            ConvModQ encrypted = r * Params.p * pub + m;
            return ConvModQToBytes(encrypted);
        }


        public static byte[] Decrypt(Key key, byte[] cipher)
        {
            ConvModQ c = BytesToConvModQ(cipher);
            (ConvModQ f, ConvModQ fInvP) = key.PrivateKey();
            ConvModQ a = f * c;
            ConvPoly aPrime = a.centerLift();
            ConvModQ m = fInvP * aPrime;
            return CoefToMsg(m.centerLift().Coef);
        }
    
    
    }
}
