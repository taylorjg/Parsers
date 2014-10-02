using System;
using System.Collections.Generic;
using System.Linq;
using MonadLib;

namespace ParsersLib
{
    // This will class become a monad eventually ?
    // ReSharper disable UnusedTypeParameter
    public class Parser<TA>
    // ReSharper restore UnusedTypeParameter
    {
    }

    public abstract class Parsers<TParseError>
    {
        public abstract Either<TParseError, TA> Run<TA>(Parser<TA> p, string input);

        // Primitives
        public abstract Parser<string> String(string s);
        public abstract Parser<string> Slice<TA>(Parser<TA> p);
        public abstract Parser<TA> Or<TA>(Parser<TA> p1, Func<Parser<TA>> p2Func);
        public abstract Parser<TB> FlatMap<TA, TB>(Parser<TA> p, Func<TA, Parser<TB>> f);
        public abstract Parser<IEnumerable<TA>> ListOfN<TA>(int n, Parser<TA> p);
        public abstract Parser<IEnumerable<TA>> Many<TA>(Parser<TA> p);
        public abstract Parser<IEnumerable<TA>> Many1<TA>(Parser<TA> p);

        // Combinators
        public Parser<char> Char(char c)
        {
            return Map(String(Convert.ToString(c)), a => a.First());
        }

        public Parser<TA> Succeed<TA>(TA a)
        {
            return Map(String(string.Empty), _ => a);
        }

        public Parser<TB> Map<TA, TB>(Parser<TA> p, Func<TA, TB> f)
        {
            return FlatMap(p, a => Succeed(f(a)));
        }

        public Parser<TC> Map2<TA, TB, TC>(Parser<TA> p1, Parser<TB> p2, Func<TA, TB, TC> f)
        {
            return FlatMap(p1, a => Map(p2, b => f(a, b)));
        }

        public Parser<Tuple<TA, TB>> Product<TA, TB>(Parser<TA> p1, Func<Parser<TB>> p2)
        {
            return FlatMap(p1, a => Map(p2(), b => Tuple.Create(a, b)));
        }
    }
}
