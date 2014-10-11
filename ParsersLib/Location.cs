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
                return Input.Substring(0, Offset).Count(x => x == '\n') + 1;
            }
        }

        public int Column
        {
            get
            {
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
