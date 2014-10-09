using System;
using System.Collections.Generic;

namespace ParsersLib
{
    // Will this class become a monad eventually ?

    public class Parser<TA>
    {
        private readonly ParsersBase _parsersBase;
        private readonly Func<Location, Result<TA>> _runFunc;

        internal Parser(ParsersBase parsersBase, Func<Location, Result<TA>> runFunc)
        {
            _parsersBase = parsersBase;
            _runFunc = runFunc;
        }

        public Result<TA> Run(Location location)
        {
            return _runFunc(location);
        }

        public Parser<TA> Or(Func<Parser<TA>> p2Func)
        {
            return _parsersBase.Or(this, p2Func);
        }

        public static Parser<TA> operator |(Parser<TA> p1, Func<Parser<TA>> p2Func)
        {
            return p1.Or(p2Func);
        }

        public Parser<TB> Map<TB>(Func<TA, TB> f)
        {
            return _parsersBase.Map(this, f);
        }

        public Parser<IEnumerable<TA>> Many()
        {
            return _parsersBase.Many(this);
        }

        public Parser<string> Slice()
        {
            return _parsersBase.Slice(this);
        }

        public Parser<Tuple<TA, TB>> Product<TB>(Func<Parser<TB>> p2Func)
        {
            return _parsersBase.Product(this, p2Func);
        }

        public Parser<TB> FlatMap<TB>(Func<TA, Parser<TB>> f)
        {
            return _parsersBase.FlatMap(this, f);
        }

        public Parser<TA> Label(string message)
        {
            return _parsersBase.Label(message, this);
        }

        public Parser<TA> Scope(string message)
        {
            return _parsersBase.Scope(message, this);
        }

        public Parser<TB> SkipL<TB>(Func<Parser<TB>> p2Func)
        {
            return _parsersBase.SkipL(this, p2Func);
        }

        public Parser<TA> SkipR<TB>(Func<Parser<TB>> p2Func)
        {
            return _parsersBase.SkipR(this, p2Func);
        }

        public Parser<TA> Token()
        {
            return _parsersBase.Token(this);
        }

        public Parser<IEnumerable<TA>> Sep<TB>(Parser<TB> p2)
        {
            return _parsersBase.Sep(this, p2);
        }

        public Parser<IEnumerable<TA>> Sep1<TB>(Parser<TB> p2)
        {
            return _parsersBase.Sep1(this, p2);
        }

        public Parser<TB> As<TB>(TB b)
        {
            return _parsersBase.As(this, b);
        }

        public Parser<IEnumerable<TA>> Many1()
        {
            return _parsersBase.Many1(this);
        }

        public Parser<IEnumerable<TA>> ListOfN(int n)
        {
            return _parsersBase.ListOfN(n, this);
        }
    }
}
