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

        public override Parser<string> Regex(string s)
        {
            return new Parser<string>(
                l =>
                    {
                        var r = new Regex(s);
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
            throw new NotImplementedException();
        }

        public override Parser<TA> Succeed<TA>(TA a)
        {
            throw new NotImplementedException();
        }

        public override Parser<TB> FlatMap<TA, TB>(Parser<TA> p, Func<TA, Parser<TB>> f)
        {
            throw new NotImplementedException();
        }

        public override Parser<TA> Or<TA>(Parser<TA> p1, Func<Parser<TA>> p2Func)
        {
            throw new NotImplementedException();
        }

        public override Parser<TA> Label<TA>(string messae, Parser<TA> p)
        {
            throw new NotImplementedException();
        }

        public override Parser<TA> Scope<TA>(string messae, Parser<TA> p)
        {
            throw new NotImplementedException();
        }

        public override Parser<TA> Attempt<TA>(Parser<TA> p)
        {
            throw new NotImplementedException();
        }
    }
}
