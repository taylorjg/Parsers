using System;
using System.Text.RegularExpressions;
using MonadLib;

namespace ParsersLib
{
    using EitherParseError = Either<ParseError>;

    public class MyParser : ParsersBase
    {
        public override Either<ParseError, TA> Run<TA>(Parser<TA> p, string input)
        {
            return p.Run(input);
        }

        public override Parser<string> String(string s)
        {
            return new Parser<string>(
                input => input.StartsWith(s)
                             ? EitherParseError.Right(s)
                             : EitherParseError.Left<string>(new Location(input).ToError("Expected: {0}", s)));
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
