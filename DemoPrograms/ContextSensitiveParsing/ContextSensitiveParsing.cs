using System;
using System.Collections.Generic;
using ParsersLib;

namespace ContextSensitiveParsing
{
    public static class ContextSensitiveParsing
    {
        public static void ContextSensitiveParsingDemo()
        {
            ParseContextSensitive();
        }

        private static void ParseContextSensitive()
        {
            var p = new MyParserImpl();
            var parser = p.Double().Bind(n => p.ListOfN(Convert.ToInt32(n), p.Char('a')));
            Func<IEnumerable<char>, string> f = xs => string.Join("", xs);
            Program.PrintResult(p.Run(parser, "0"), f);
            Program.PrintResult(p.Run(parser, "1a"), f);
            Program.PrintResult(p.Run(parser, "2aa"), f);
            Program.PrintResult(p.Run(parser, "3aaa"), f);
            Program.PrintResult(p.Run(parser, "7aaaaaaa"), f);
        }
    }
}
