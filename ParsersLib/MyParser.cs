using System;
using System.Text.RegularExpressions;

namespace ParsersLib
{
    public class MyParser : ParsersBase
    {
        public override Result<TA> Run<TA>(Parser<TA> p, string input)
        {
            return p.Run(new Location(input));
        }

        public override Parser<string> String(string s)
        {
            return new Parser<string>(
                l => l.Input.StartsWith(s)
                         ? new Success<string>(s, s.Length) as Result<string>
                         : new Failure<string>(l.ToError("Expected input matching string, '{0}'.", s)));
        }

        public override Parser<string> Regex(Regex r)
        {
            return new Parser<string>(
                l =>
                    {
                        var match = r.Match(l.Input);
                        if (match.Success)
                        {
                            var a = match.Value;
                            return new Success<string>(a, a.Length);
                        }
                        return new Failure<string>(l.ToError("Expected input matching regular expression, '{0}'.", r));
                    });
        }

        public override Parser<string> Slice<TA>(Parser<TA> p)
        {
            return new Parser<string>(l => p.Run(l).Match(
                success => new Success<string>(l.Input.Substring(0, success.CharsConsumed), success.CharsConsumed),
                failure => new Failure<string>(failure.ParseError)));
        }

        public override Parser<TA> Succeed<TA>(TA a)
        {
            return new Parser<TA>(l => new Success<TA>(a, 0));
        }

        public override Parser<TB> FlatMap<TA, TB>(Parser<TA> p, Func<TA, Parser<TB>> f)
        {
            throw new NotImplementedException();
        }

        public override Parser<TA> Or<TA>(Parser<TA> p1, Func<Parser<TA>> p2Func)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
}
