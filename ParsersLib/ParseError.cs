using System;
using System.Collections.Generic;
using System.Linq;

namespace ParsersLib
{
    public class ParseError
    {
        public IEnumerable<Tuple<Location, string>> Stack { get; private set; }

        public ParseError(IEnumerable<Tuple<Location, string>> stack)
        {
            Stack = stack;
        }

        public ParseError Push(Location location, string message)
        {
            return new ParseError(Enumerable.Repeat(Tuple.Create(location, message), 1).Concat(Stack));
        }

        public ParseError Label(string message)
        {
            return LatestLocation != null
                       ? new ParseError(Enumerable.Repeat(Tuple.Create(LatestLocation, message), 1).Concat(Stack))
                       : new ParseError(Enumerable.Empty<Tuple<Location, string>>());
        }

        private Location LatestLocation {
            get
            {
                return Latest != null ? Latest.Item1 : null;
            }
        }

        private Tuple<Location, string> Latest {
            get { return Stack.LastOrDefault(); }
        }
    }
}
