using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTRUEncrypt
{
    public class Key
    {
        public Key(ConvModQ? f = null, ConvModQ? g = null)
        {
            _f = f is null ? ConvModQ.RandomTrinary(Params.df + 1, Params.df) : f;
            _g = g is null ? ConvModQ.RandomTrinary(Params.dg, Params.dg) : g;

            if (f is not null)
            {
                _fInvQ = _f.inverse();
                _fInvP = new ConvModQ(_f.centerLift().Coef, Params.p, Params.N).inverse();
            }

            while (_fInvQ is null || _fInvP is null)
            {
                try
                {
                    _fInvQ = _f.inverse();
                    _fInvP = new ConvModQ(_f.centerLift().Coef, Params.p, Params.N).inverse();
                }
                catch (Exception)
                { }
            }
            _h = _fInvQ * _g;
        }

        public ConvModQ PublicKey()
        {
            return _h;
        }

        public (ConvModQ, ConvModQ) PrivateKey()
        {
            return (_f, _fInvP!);
        }

        private ConvModQ _f;
        private ConvModQ _g;
        private ConvModQ _h;
        private ConvModQ? _fInvQ;
        private ConvModQ? _fInvP;
    }
}
