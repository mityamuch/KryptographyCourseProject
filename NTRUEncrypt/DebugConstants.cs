using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTRUEncrypt
{
    public class Params
    {
        public const int N = 1499;//максимальная степень
        public const int df = 79;
        public const int q = 2048;//модуль для коэфицентов
        public const int p = 3;
        public const int dg = N / 3;
        public const int dr = 79;
    }
}
