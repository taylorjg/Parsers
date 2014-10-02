using System;
using System.Collections.Generic;

namespace ParsersLib
{
    public class ParseError
    {
        public IEnumerable<Tuple<Location, string>> Stack { get; private set; }

        public ParseError(IEnumerable<Tuple<Location, string>> stack)
        {
            Stack = stack;
        }
    }
}
