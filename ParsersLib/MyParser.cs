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
                location => location.Input.StartsWith(s)
                                ? new Success<string>(s, s.Length) as Result<string>
                                : new Failure<string>(location.ToError("Expected: {0}", s)));
        }

        public override Parser<string> Regex(Regex r)
        {
            throw new NotImplementedException();
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
