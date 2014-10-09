using System;
using System.Linq;

namespace ParsersLib
{
    public class Location
    {
        public Location(string input, int offset = 0)
        {
            Input = input;
            Offset = offset;
        }

        public string Input { get; private set; }
        public int Offset { get; private set; }

        public string CurrentInput
        {
            get { return Input.Substring(Offset); }
        }

        public int Line
        {
            get
            {
                // TODO: instead of the substring from 0 -> Offset, should we use 0 -> location of first line break after Offset ?
                return Input.Substring(0, Offset).Count(x => x == '\n');
            }
        }

        public int Column
        {
            get
            {
                // TODO: instead of the substring from 0 -> Offset, should we use 0 -> location of first line break after Offset ?
                var lineStart = Input.Substring(0, Offset).LastIndexOf('\n');
                switch (lineStart)
                {
                    case -1:
                        return Offset + 1;

                    default:
                        return Offset - lineStart;
                }
            }
        }

        public ParseError ToError(string format, params object[] args)
        {
            var message = string.Format(format, args);
            return new ParseError(new[] {Tuple.Create(this, message)});
        }

        public Location AdvanceBy(int n)
        {
            return new Location(Input, Offset + n);
        }
    }
}
