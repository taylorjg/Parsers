﻿using System;
using System.Text.RegularExpressions;
using MonadLib;

namespace ParsersLib
{
    public class MyParser : ParsersBase
    {
        public override Either<ParseError, TA> Run<TA>(Parser<TA> p, string input)
        {
            return p.Run(new Location(input)).Extract();
        }

        public override Parser<string> String(string s)
        {
            return new Parser<string>(
                l => l.Input.Substring(l.Offset).StartsWith(s)
                         ? new Success<string>(s, s.Length) as Result<string>
                         : new Failure<string>(l.ToError("Expected input matching string, '{0}'.", s), false));
        }

        public override Parser<string> Regex(Regex r)
        {
            return new Parser<string>(
                l =>
                    {
                        var match = r.Match(l.Input.Substring(l.Offset));
                        if (match.Success)
                        {
                            var a = match.Value;
                            return new Success<string>(a, a.Length);
                        }
                        return new Failure<string>(l.ToError("Expected input matching regular expression, '{0}'.", r), false);
                    });
        }

        public override Parser<string> Slice<TA>(Parser<TA> p)
        {
            return new Parser<string>(l => p.Run(l).MatchResult(
                success => new Success<string>(l.Input.Substring(l.Offset, success.CharsConsumed), success.CharsConsumed),
                failure => new Failure<string>(failure.ParseError, failure.IsCommitted)));
        }

        public override Parser<TA> Succeed<TA>(TA a)
        {
            return new Parser<TA>(l => new Success<TA>(a, 0));
        }

        public override Parser<TB> FlatMap<TA, TB>(Parser<TA> p, Func<TA, Parser<TB>> f)
        {
            return new Parser<TB>(l => p.Run(l).MatchResult(
                success =>
                    {
                        var a = success.Value;
                        var n = success.CharsConsumed;
                        var p2 = f(a);
                        return p2.Run(l.AdvanceBy(n))
                                 .AddCommit(n > 0)
                                 .AdvanceSuccess(n);
                    },
                failure => new Failure<TB>(failure.ParseError, failure.IsCommitted)));
        }

        public override Parser<TA> Or<TA>(Parser<TA> p1, Func<Parser<TA>> p2Func)
        {
            return new Parser<TA>(l => p1.Run(l).MatchResult(
                success => success,
                failure => failure.IsCommitted ? failure : p2Func().Run(l)));
        }

        public override Parser<TA> Label<TA>(string message, Parser<TA> p)
        {
            return new Parser<TA>(l => p.Run(l).MapError(pe => pe.Label(message)));
        }

        public override Parser<TA> Scope<TA>(string message, Parser<TA> p)
        {
            return new Parser<TA>(l => p.Run(l).MapError(pe => pe.Push(l, message)));
        }

        public override Parser<TA> Attempt<TA>(Parser<TA> p)
        {
            return new Parser<TA>(l => p.Run(l).Uncommit());
        }
    }
}
