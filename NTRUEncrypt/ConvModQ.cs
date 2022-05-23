using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace NTRUEncrypt
{
    /// <summary>
    /// Класс полинома по модулю полинома где все коэфиценты по модулю q
    /// </summary>
    public class ConvModQ : PolyModQ
    {
        public ConvModQ(int[] coef, int q, int N)
        {
            Debug.Assert(N >= coef.Length);
            _coef = new int[N];
            for (int i = 0; i < coef.Length; ++i)
                _coef[i] = (coef[i] + q) % q;
            _N = N;
            _degree = N - 1;
            _q = q;
        }

        public static ConvModQ operator +(ConvModQ lhs, ConvModQ rhs)
        {
            PolyModQ res = (lhs as PolyModQ) + (rhs as PolyModQ);
            return new ConvModQ(res.Coef, lhs._q, lhs._N);
        }

        public static ConvModQ operator -(ConvModQ lhs, ConvModQ rhs)
        {
            PolyModQ res = (lhs as PolyModQ) - (rhs as PolyModQ);
            return new ConvModQ(res.Coef, lhs._q, lhs._N);
        }

        public static ConvModQ operator *(ConvModQ lhs, ConvModQ rhs)
        {
            Debug.Assert(lhs._N == rhs._N);

            int[] coef = new int[lhs._N];
            for (int i = 0; i < lhs._N; ++i)
                for (int j = 0; j < rhs._N; ++j)
                    coef[i] += lhs._coef[j] * rhs._coef[(lhs._N + i - j) % lhs._N];

            return new ConvModQ(coef, lhs._q, lhs._N);
        }

        public static ConvModQ operator *(ConvModQ lhs, ConvPoly rhs)
        {
            return lhs * new ConvModQ(rhs.Coef, lhs._q, lhs._N);
        }

        public static ConvModQ operator *(ConvModQ lhs, int rhs)
        {
            int[] coef = new int[lhs._N];
            for (int i = 0; i < lhs._N; ++i)
                coef[i] = lhs._coef[i] * rhs;
            return new ConvModQ(coef, lhs._q, lhs._N);
        }

        private int invModQ(int d)
        {
            for (int i = 1; i < _q; ++i)
                if ((d * i) % _q == 1)
                    return i;
            throw new Exception("Not invertible");
        }

        public static ConvModQ operator /(ConvModQ lhs, int rhs)
        {
            return lhs * lhs.invModQ(rhs);
        }
        public ConvModQ inverse()
        {
            if (_q == 2048)
                _q = 2;
            const int FAIL = 100000;
            int i = 0;
            List<PolyModQ> quotients = new List<PolyModQ>();
            //  Extended Euclidian Algorithm
            //  q = b*k + r
            int[] q_coef = new int[_N + 1];
            q_coef[0] = -1;
            q_coef[_N] = 1;
            PolyModQ q = new PolyModQ(q_coef, _q);

            int[] k_coef = new int[1];
            k_coef[0] = 0;
            PolyModQ k = new PolyModQ(k_coef, _q);

            PolyModQ b = new PolyModQ(_coef, _q);
            PolyModQ r = q;

            //  repeat below while r!=0 for gcd/inverse
            int bdinv = invModQ(b.Coef[b.Coef.Length - 1]);

            while (r.Degree >= b.Degree && i < FAIL)
            {
                int[] kp_coef = new int[r.Degree - b.Degree + 1];
                kp_coef[kp_coef.Length - 1] = r.Coef[r.Degree] * bdinv;
                PolyModQ kp = new PolyModQ(kp_coef, _q);
                k = k + kp;
                r = r - kp * b;
                i++;
            }
            quotients.Add(k);

            while (r != new PolyModQ(new int[] { 0 }, _q) && i < FAIL)
            {
                q = b;
                b = r;
                k = new PolyModQ(new int[_N + 1], _q);
                r = q;
                bdinv = invModQ(b.Coef[b.Coef.Length - 1]);
                while (r.Degree >= b.Degree && r != new PolyModQ(new int[] { 0 }, _q) && i < FAIL)
                {
                    int[] kp_coef = new int[r.Degree - b.Degree + 1];
                    kp_coef[kp_coef.Length - 1] = r.Coef[r.Degree] * bdinv;
                    PolyModQ kp = new PolyModQ(kp_coef, _q);
                    k = k + kp;
                    r = r - kp * b;
                    i++;
                }
                quotients.Add(k);
                i++;
            }
            if (i >= FAIL)
                throw new Exception("Failed to generate inverse in 10000 steps, stopping.");

            List<PolyModQ> x = new List<PolyModQ>(new PolyModQ[] { new PolyModQ(new int[] { 0 }, _q), new PolyModQ(new int[] { 1 }, _q) });
            List<PolyModQ> y = new List<PolyModQ>(new PolyModQ[] { new PolyModQ(new int[] { 1 }, _q), new PolyModQ(new int[] { 0 }, _q) });
            for (int j = 0; j < quotients.Count; j++)
            {
                x.Add(quotients[j] * x[j + 1] + x[j]);
                y.Add(quotients[j] * y[j + 1] + y[j]);
            }
            if (_q == 2)
            {
                int n = 2;
                _q = 2048;
                ConvModQ tinv2048 = new ConvModQ(x[x.Count - 2].Coef, _q, _N);
                while (n <= 2048)
                {
                    tinv2048 = tinv2048 * 2 - this * tinv2048 * tinv2048;
                    n *= 2;
                }
                return tinv2048;
            }
            ConvModQ tinv = new ConvModQ(x[x.Count - 2].Coef, _q, _N);
            tinv = tinv * _q - this * tinv * tinv;
            return tinv * 2;
        }
        public static ConvModQ RandomTrinary(int oneCount, int minusOneCount)
        {
            int[] coef = new int[Params.N];
            int i = 0;
            for (; i < oneCount; i++)
                coef[i] = 1;
            for (; i < oneCount + minusOneCount; i++)
                coef[i] = -1;
            int[] sortBy = new int[Params.N];
            Random rnd = new Random();
            for (i = 0; i < Params.N; i++)
                sortBy[i] = rnd.Next();
            Array.Sort(sortBy, coef);
            return new ConvModQ(coef, Params.q, Params.N);
        }



        private int _N;
    }
}
