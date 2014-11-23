using System.Text.RegularExpressions;
using ParsersLib;

namespace SimpleParsing
{
    public static class SimpleParsing
    {
        public static void SimpleParsingDemo()
        {
            ParseSimpleThingsThatShouldSucceed();
            ParseSimpleThingsThatShouldFail();
        }
        private static void ParseSimpleThingsThatShouldSucceed()
        {
            var p = new MyParserImpl();
            Program.PrintResult(p.Run(p.Char('c'), "c"));
            Program.PrintResult(p.Run(p.String("abc"), "abcdefg"));
            Program.PrintResult(p.Run(p.Slice(p.String("abc")), "abcdefg"));
            Program.PrintResult(p.Run(p.Regex(new Regex(@"\dabc\d")), "5abc6"));
            Program.PrintResult(p.Run(p.Double(), "12.4"));
            Program.PrintResult(p.Run(p.Eof(), ""));
            Program.PrintResult(p.Run(p.Whitespace(), "  "));
            Program.PrintResult(p.Run(p.SkipL(p.Whitespace(), () => p.String("abc")), "  abc"));
            Program.PrintResult(p.Run(p.SkipR(p.String("abc"), p.Whitespace), "abc  "));
            Program.PrintResult(p.Run(p.Surround(p.Char('['), p.Char(']'), () => p.String("abc")), "[abc]"));
            Program.PrintResult(p.Run(p.Root(p.String("abc")), "abc"));
            Program.PrintResult(p.Run(p.Quoted(), "\"abc\""));
            Program.PrintResult(p.Run(p.String("mouse ").Bind(s => p.NotFollowedBy(p.String("cat")).BindIgnoringLeft(p.Succeed(s))), "mouse dog"));
            Program.PrintResult(p.Run(p.String("mouse ").Bind(s => p.NotFollowedBy(p.String("cat")).BindIgnoringLeft(p.Succeed(s))), "mouse cat"));
            Program.PrintResult(p.Run(p.NoneOf("abc"), "x"));
            Program.PrintResult(p.Run(p.NoneOf("abc"), "b"));

            var intAndOptionalCommaParser = p.Int().SkipR(() => p.OptionMaybe(p.Token(p.String(","))));
            Program.PrintResult(p.Run(intAndOptionalCommaParser, "10, 11, 12"));
            Program.PrintResult(p.Run(intAndOptionalCommaParser, "10"));
        }

        private static void ParseSimpleThingsThatShouldFail()
        {
            var p = new MyParserImpl();
            Program.PrintResult(p.Run(p.Char('c'), "d"));
            Program.PrintResult(p.Run(p.String("abc"), "defg"));
            Program.PrintResult(p.Run(p.Eof(), "abc"));
            Program.PrintResult(p.Run(p.Root(p.String("abc")), "abcblah"));
        }
    }
}
