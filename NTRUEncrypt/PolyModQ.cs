using System;
using System.Diagnostics;
using System.Linq;


namespace NTRUEncrypt
{
    /// <summary>
    /// Класс полинома, где все коэфиценты по модулю q
    /// </summary>
    public class PolyModQ
    {
        protected PolyModQ()
        {
            _coef = new int[] { };
        }

        public PolyModQ(int[] coef, int q)
        {
            int i;
            for (i = coef.Length - 1; i > 0; --i)
                if (coef[i] % q != 0)
                    break;
            _coef = new int[i + 1];
            _degree = i;

            for (i = 0; i < _coef.Length; ++i)
                _coef[i] = (coef[i] + q) % q;
            _q = q;
        }

        public static bool operator ==(PolyModQ lhs, PolyModQ rhs)
        {
            return Enumerable.SequenceEqual(lhs._coef, rhs._coef);
        }

        public static bool operator !=(PolyModQ lhs, PolyModQ rhs)
        {
            return !(lhs == rhs);
        }

        public static PolyModQ operator -(PolyModQ op)
        {
            int[] coef = new int[op._coef.Length];
            for (int i = 0; i < coef.Length; ++i)
                coef[i] = -op._coef[i];
            return new PolyModQ(coef, op._q);
        }

        public static PolyModQ operator +(PolyModQ lhs, PolyModQ rhs)
        {
            Debug.Assert(lhs._q == rhs._q);

            if (lhs.Degree >= rhs.Degree)
            {
                int[] coef = new int[lhs._coef.Length];
                Array.Copy(lhs._coef, coef, lhs._coef.Length);
                for (int i = 0; i < rhs._coef.Length; ++i)
                    coef[i] += rhs._coef[i];
                return new PolyModQ(coef, lhs._q);
            }
            else
                return rhs + lhs;
        }

        public static PolyModQ operator +(PolyModQ lhs, int rhs)
        {
            int[] coef = new int[lhs._coef.Length];
            Array.Copy(lhs._coef, coef, coef.Length);
            coef[0] += rhs;
            return new PolyModQ(coef, lhs._q);
        }

        public static PolyModQ operator -(PolyModQ lhs, PolyModQ rhs)
        {
            return lhs + (-rhs);
        }

        public static PolyModQ operator -(PolyModQ lhs, int rhs)
        {
            return lhs + (-rhs);
        }


        public static PolyModQ operator *(PolyModQ lhs, PolyModQ rhs)
        {
            Debug.Assert(lhs._q == rhs._q);

            int[] coef = new int[lhs._coef.Length + rhs._coef.Length + 1];
            for (int i = 0; i < lhs._coef.Length; ++i)
                for (int j = 0; j < rhs._coef.Length; ++j)
                    coef[i + j] += lhs._coef[i] * rhs._coef[j];

            return new PolyModQ(coef, lhs._q);
        }

        public ConvPoly centerLift()
        {
            int[] coef = new int[_coef.Length];
            for (int i = 0; i < coef.Length; ++i)
            {
                if (_coef[i] > _q / 2.0)
                    coef[i] = _coef[i] - _q;
                else
                    coef[i] = _coef[i];
            }
            return new ConvPoly(coef, coef.Length);
        }


        protected int[] _coef;
        protected int _degree;
        protected int _q;
        public int[] Coef { get => _coef; }
        public int Degree { get => _degree; }
    } 
}
