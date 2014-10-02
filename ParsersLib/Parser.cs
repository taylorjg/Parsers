using System;

namespace ParsersLib
{
    // Will this class become a monad eventually ?

    public class Parser<TA>
    {
        private readonly Func<Location, Result<TA>> _runFunc;

        public Parser(Func<Location, Result<TA>> runFunc)
        {
            _runFunc = runFunc;
        }

        public Result<TA> Run(Location location)
        {
            return _runFunc(location);
        }
    }
}
