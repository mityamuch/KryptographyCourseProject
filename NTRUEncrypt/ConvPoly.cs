using System;
using System.Diagnostics;
using System.Linq;


namespace NTRUEncrypt
{
    /// <summary>
    /// Класс Полинома по модулю полинома
    /// </summary>
    public class ConvPoly
    {
        public ConvPoly(int[] coef, int N)
        {
            Debug.Assert(N >= coef.Length);

            _coef = new int[N];
            Array.Copy(coef, _coef, coef.Length);
            _N = N;
        }

        public static ConvPoly operator +(ConvPoly lhs, ConvPoly rhs)
        {
            Debug.Assert(lhs._N == rhs._N);
            int[] coef = new int[lhs._N];
            for (int i = 0; i < lhs._N; ++i)
                coef[i] = lhs._coef[i] + rhs._coef[i];
            return new ConvPoly(coef, lhs._N);
        }

        public static ConvPoly operator +(ConvPoly lhs, int rhs)
        {
            int[] coef = new int[lhs._N];
            Array.Copy(lhs._coef, coef, lhs._N);
            coef[0] += rhs;
            return new ConvPoly(coef, lhs._N);
        }
        public static bool operator ==(ConvPoly lhs, ConvPoly rhs)
        {
            return Enumerable.SequenceEqual(lhs._coef, rhs._coef);
        }

        public static bool operator !=(ConvPoly lhs, ConvPoly rhs)
        {
            return !(lhs == rhs);
        }

        public static ConvPoly operator -(ConvPoly op)
        {
            int[] coef = new int[op._coef.Length];
            for (int i = 0; i < coef.Length; ++i)
                coef[i] = -op._coef[i];
            return new ConvPoly(coef, op._N);
        }

        public static ConvPoly operator -(ConvPoly lhs, ConvPoly rhs)
        {
            return lhs + (-rhs);
        }

        public static ConvPoly operator -(ConvPoly lhs, int rhs)
        {
            return lhs + (-rhs);
        }

        public static ConvPoly operator *(ConvPoly lhs, ConvPoly rhs)
        {
            Debug.Assert(lhs._N == rhs._N);

            int[] coef = new int[lhs._N];
            for (int i = 0; i < lhs._N; ++i)
                for (int j = 0; j < rhs._N; ++j)
                    coef[i] += lhs._coef[j] * rhs._coef[(lhs._N + i - j) % lhs._N];

            return new ConvPoly(coef, lhs._N);
        }

        public static ConvPoly operator *(ConvPoly lhs, int rhs)
        {
            int[] coef = new int[lhs._N];
            for (int i = 0; i < lhs._N; ++i)
                coef[i] = lhs._coef[i] * rhs;
            return new ConvPoly(coef, lhs._N);
        }

        private int[] _coef;
        private int _N;

        public int[] Coef { get => _coef; }
    } //полином по модулю полинома
}
