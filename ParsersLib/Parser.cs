using System;
using MonadLib;

namespace ParsersLib
{
    // This will class become a monad eventually ?

    public class Parser<TA>
    {
        private readonly Func<string, Either<ParseError, TA>> _runFunc;

        public Parser(Func<string, Either<ParseError, TA>> runFunc)
        {
            _runFunc = runFunc;
        }

        public Either<ParseError, TA> Run(string input)
        {
            return _runFunc(input);
        }
    }
}
