﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MonadLib;

namespace ParsersLib
{
    public abstract class ParsersBase
    {
        public abstract Either<ParseError, TA> Run<TA>(Parser<TA> p, string input);

        // Primitives
        public abstract Parser<string> String(string s);
        public abstract Parser<string> Regex(Regex r);
        public abstract Parser<string> Slice<TA>(Parser<TA> p);
        public abstract Parser<TA> Succeed<TA>(TA a);
        public abstract Parser<TA> Fail<TA>(string message);
        public abstract Parser<TB> FlatMap<TA, TB>(Parser<TA> p, Func<TA, Parser<TB>> f);
        public abstract Parser<TA> Or<TA>(Parser<TA> p1, Func<Parser<TA>> p2Func);
        public abstract Parser<TA> Label<TA>(string message, Parser<TA> p); 
        public abstract Parser<TA> Scope<TA>(string message, Parser<TA> p); 
        public abstract Parser<TA> Attempt<TA>(Parser<TA> p); 

        // Combinators
        public Parser<char> Char(char c)
        {
            return String(new string(c, 1)).Map(s => s.First());
        }

        public Parser<string> R(string pattern)
        {
            return Regex(new Regex(pattern));
        }

        public Parser<TB> Map<TA, TB>(Parser<TA> p, Func<TA, TB> f)
        {
            return p.FlatMap(a => Succeed(f(a)));
        }

        public Parser<TC> Map2<TA, TB, TC>(Parser<TA> p1, Func<Parser<TB>> p2Func, Func<TA, TB, TC> f)
        {
            return p1.FlatMap(a => Map(p2Func(), b => f(a, b)));
        }

        public Parser<Tuple<TA, TB>> Product<TA, TB>(Parser<TA> p1, Func<Parser<TB>> p2Func)
        {
            return p1.FlatMap(a => Map(p2Func(), b => Tuple.Create(a, b)));
        }

        public Parser<IEnumerable<TA>> Many<TA>(Parser<TA> p)
        {
            return Map2(p, () => Many(p), Cons) | (() => Succeed(Enumerable.Empty<TA>()));
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

        public Parser<TB> SkipL<TA, TB>(Parser<TA> p1, Func<Parser<TB>> p2Func)
        {
            return Map2(Slice(p1), p2Func, (_, b) => b);
        }

        public Parser<TA> SkipR<TA, TB>(Parser<TA> p1, Func<Parser<TB>> p2Func)
        {
            return Map2(p1, () => Slice(p2Func()), (a, _) => a);
        }

        public Parser<Maybe<TA>> Opt<TA>(Parser<TA> p)
        {
            return p.Map(Maybe.Just) | (() => Succeed(Maybe.Nothing<TA>()));
        }

        public Parser<string> Whitespace()
        {
            return R(@"\s*");
        }

        public Parser<string> Digits()
        {
            return R(@"\d+");
        }

        public Parser<TA> Token<TA>(Parser<TA> p)
        {
            return Attempt(p).SkipR(Whitespace);
        }

        public Parser<string> DoubleString()
        {
            return Token(R(@"^[-+]?([0-9]*\.)?[0-9]+([eE][-+]?[0-9]+)?"));
        }

        public Parser<double> Double()
        {
            return Label("double literal", DoubleString().Map(Convert.ToDouble));
        }

        public Parser<int> Int()
        {
            return Token(Digits()).Map(Convert.ToInt32);
        }

        public Parser<string> Eof()
        {
            return Label("unexpected trailing characters", R(@"^\z"));
        }

        public Parser<IEnumerable<TA>> Sep1<TA, TB>(Parser<TA> p1, Parser<TB> p2)
        {
            return Map2(p1, () => Many(SkipL(p2, () => p1)), Cons);
        }

        public Parser<IEnumerable<TA>> Sep<TA, TB>(Parser<TA> p1, Parser<TB> p2)
        {
            return p1.Sep1(p2) | (() => Succeed(Enumerable.Empty<TA>()));
        }

        public Parser<TA> Surround<TA, TB>(Parser<TB> start, Parser<TB> stop, Func<Parser<TA>> pFunc)
        {
            return start.SkipL(
                () => pFunc().SkipR(
                    () => stop));
        }

        public Parser<string> Thru(string s)
        {
            return R(@".*?" + System.Text.RegularExpressions.Regex.Escape(s));
        }

        public Parser<string> Quoted()
        {
            return String("\"").SkipL(() => Thru("\"")).Map(s => s.Substring(0, s.Length - 1));
        }

        public Parser<TB> As<TA, TB>(Parser<TA> p, TB b)
        {
            return Slice(p).Map(_ => b);
        }

        public Parser<TA> Root<TA>(Parser<TA> p)
        {
            return p.SkipR(Eof);
        }

        private static IEnumerable<T> Cons<T>(T x, IEnumerable<T> xs)
        {
            return Enumerable.Repeat(x, 1).Concat(xs);
        }
    }
}
