using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ParsersLib
{
    public abstract class ParsersBase
    {
        public abstract Result<TA> Run<TA>(Parser<TA> p, string input);

        // Primitives
        public abstract Parser<string> String(string s);
        public abstract Parser<string> Regex(Regex r);
        public abstract Parser<string> Regex(string s);
        public abstract Parser<string> Slice<TA>(Parser<TA> p);
        public abstract Parser<TA> Succeed<TA>(TA a);
        public abstract Parser<TB> FlatMap<TA, TB>(Parser<TA> p, Func<TA, Parser<TB>> f);
        public abstract Parser<TA> Or<TA>(Parser<TA> p1, Func<Parser<TA>> p2Func);
        public abstract Parser<TA> Label<TA>(string messae, Parser<TA> p); 
        public abstract Parser<TA> Scope<TA>(string messae, Parser<TA> p); 
        public abstract Parser<TA> Attempt<TA>(Parser<TA> p); 

        // Combinators
        public Parser<char> Char(char c)
        {
            return Map(String(Convert.ToString(c)), a => a.First());
        }

        public Parser<TB> Map<TA, TB>(Parser<TA> p, Func<TA, TB> f)
        {
            return FlatMap(p, a => Succeed(f(a)));
        }

        public Parser<TC> Map2<TA, TB, TC>(Parser<TA> p1, Func<Parser<TB>> p2, Func<TA, TB, TC> f)
        {
            return FlatMap(p1, a => Map(p2(), b => f(a, b)));
        }

        public Parser<Tuple<TA, TB>> Product<TA, TB>(Parser<TA> p1, Func<Parser<TB>> p2)
        {
            return FlatMap(p1, a => Map(p2(), b => Tuple.Create(a, b)));
        }

        public Parser<IEnumerable<TA>> Many<TA>(Parser<TA> p)
        {
            return Or(
                Map2(p, () => Many(p), Cons),
                () => Succeed(Enumerable.Empty<TA>()));
        }

        public Parser<IEnumerable<TA>> Many1<TA>(Parser<TA> p)
        {
            return Map2(p, () => Many(p), Cons);
        }

        public Parser<IEnumerable<TA>> ListOfN<TA>(int n, Parser<TA> p)
        {
            return n <= 0
                       ? Succeed(Enumerable.Empty<TA>())
                       : Map2(p, () => ListOfN(n - 1, p), Cons);
        }

        private static IEnumerable<T> Cons<T>(T x, IEnumerable<T> xs)
        {
            return Enumerable.Repeat(x, 1).Concat(xs);
        }
    }
}
