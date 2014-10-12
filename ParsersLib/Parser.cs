using System;
using System.Collections.Generic;
using MonadLib;

namespace ParsersLib
{
    // Will this class become a monad eventually ?

    public class Parser<TA> : IMonad<TA>
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

        public Parser<IEnumerable<TA>> SepBy<TB>(Parser<TB> p2)
        {
            return _parsersBase.SepBy(this, p2);
        }

        public Parser<IEnumerable<TA>> SepBy1<TB>(Parser<TB> p2)
        {
            return _parsersBase.SepBy1(this, p2);
        }

        public Parser<IEnumerable<TA>> EndBy<TB>(Parser<TB> p2)
        {
            return _parsersBase.EndBy(this, p2);
        }

        public Parser<IEnumerable<TA>> EndBy1<TB>(Parser<TB> p2)
        {
            return _parsersBase.EndBy1(this, p2);
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

        private MonadAdapter _monadAdapter;

        public MonadAdapter GetMonadAdapter()
        {
            return _monadAdapter ?? (_monadAdapter = new ParserMonadAdapter());
        }
    }

    public static class Parser
    {
        public static Parser<TA> Return<TA>(TA a)
        {
            // TODO: it would be nice to find a way to remove this hard dependency on MyParserImpl.
            return new MyParserImpl().Succeed(a);
        }

        public static Parser<TB> Bind<TA, TB>(this Parser<TA> ma, Func<TA, Parser<TB>> f)
        {
            var monadAdapter = ma.GetMonadAdapter();
            return (Parser<TB>)monadAdapter.Bind(ma, f);
        }

        public static Parser<TB> BindIgnoringLeft<TA, TB>(this Parser<TA> ma, Parser<TB> mb)
        {
            var monadAdapter = ma.GetMonadAdapter();
            return (Parser<TB>)monadAdapter.BindIgnoringLeft(ma, mb);
        }
    }

    internal class ParserMonadAdapter : MonadAdapter
    {
        public override IMonad<TA> Return<TA>(TA a)
        {
            return Parser.Return(a);
        }

        public override IMonad<TB> Bind<TA, TB>(IMonad<TA> ma, Func<TA, IMonad<TB>> f)
        {
            var parserA = (Parser<TA>) ma;
            return parserA.FlatMap(a => (Parser<TB>) f(a));
        }
    }
}
