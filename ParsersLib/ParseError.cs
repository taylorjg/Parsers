using System;
using System.Collections.Generic;
using System.Linq;
using MonadLib;

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
            return new ParseError(ConsStack(location, message));
        }

        public ParseError Label(string message)
        {
            return new ParseError(LatestLocation.LiftM(location => Tuple.Create(location, message))
                                                .ToEnumerable());
        }

        private Maybe<Location> LatestLocation
        {
            get { return Latest.LiftM(l => l.Item1); }
        }

        private Maybe<Tuple<Location, string>> Latest
        {
            get { return Maybe.ListToMaybe(Stack.Reverse()); }
        }

        private IEnumerable<Tuple<Location, string>> ConsStack(Location location, string message)
        {
            return Enumerable.Repeat(Tuple.Create(location, message), 1)
                             .Concat(Stack);
        }
    }
}
