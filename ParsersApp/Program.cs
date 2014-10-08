using System;
using System.Text.RegularExpressions;
using MonadLib;
using ParsersLib;

namespace ParsersApp
{
    internal class Program
    {
        private static void Main( /* string[] args */)
        {
            var p = new MyParserImpl();

            PrintResult(p.Run(p.Char('c'), "c"));
            PrintResult(p.Run(p.Char('c'), "d"));
            PrintResult(p.Run(p.String("abc"), "abcdefg"));
            PrintResult(p.Run(p.String("abc"), "defg"));
            PrintResult(p.Run(p.Slice(p.String("abc")), "abcdefg"));
            PrintResult(p.Run(p.Regex(new Regex(@"\dabc\d")), "5abc6"));
            PrintResult(p.Run(p.Double(), "12.4"));
            PrintResult(p.Run(p.Eof(), ""));
            PrintResult(p.Run(p.Eof(), "abc"));
            PrintResult(p.Run(p.Whitespace(), "  "));
            PrintResult(p.Run(p.SkipL(p.Whitespace(), () => p.String("abc")), "  abc"));
            PrintResult(p.Run(p.SkipR(p.String("abc"), p.Whitespace), "abc  "));
            PrintResult(p.Run(p.Surround(p.Char('['), p.Char(']'), p.String("abc")), "[abc]"));
            PrintResult(p.Run(p.Root(p.String("abc")), "abcblah"));
            PrintResult(p.Run(p.Quoted(), "\"abc\""));

            ParseJsonLiteral();
            ParseJsonKeyValue();
            ParseJsonObject();
            ParseJsonArray();
        }

        private static void PrintResult<TA>(Either<ParseError, TA> either)
        {
            either.Match(
                Console.WriteLine,
                a => Console.WriteLine("a: {0}", a));
        }

        private static Parser<Json> Literal(ParsersBase p)
        {
            return p.Scope("literal",
                           p.String("null").As(new JNull() as Json)
                            .Or(() => p.Double().Map(n => new JNumber(n) as Json))
                            .Or(() => p.Quoted().Map(s => new JString(s) as Json))
                            .Or(() => p.String("true").As(new JBool(true) as Json))
                            .Or(() => p.String("false").As(new JBool(false) as Json)));
        }

        private static Parser<Tuple<string, Json>> KeyValue(MyParserImpl p)
        {
            var literal = Literal(p);
            return p.Quoted().Product(() => literal.SkipL(p.String(":")));
        }

        private static void ParseJsonLiteral()
        {
            var p = new MyParserImpl();
            var literal = Literal(p);

            PrintResult(p.Run(literal, "null"));
            PrintResult(p.Run(literal, "12.4"));
            PrintResult(p.Run(literal, "\"fred\""));
            PrintResult(p.Run(literal, "true"));
            PrintResult(p.Run(literal, "false"));
            PrintResult(p.Run(literal, "bogus"));
        }

        private static void ParseJsonKeyValue()
        {
            var p = new MyParserImpl();
            var keyValue = KeyValue(p);

            PrintResult(p.Run(keyValue, "\"employee\":null"));
            PrintResult(p.Run(keyValue, "\"amount\":12.4"));
            PrintResult(p.Run(keyValue, "\"name\":\"Jon\""));
            PrintResult(p.Run(keyValue, "\"enabled\":true"));
            PrintResult(p.Run(keyValue, "\"enabled\":false"));
        }

        private static void ParseJsonObject()
        {
            var p = new MyParserImpl();
        }

        private static void ParseJsonArray()
        {
            var p = new MyParserImpl();
            var literal = Literal(p);
            var array = p.Surround(p.Char('['), p.Char(']'), literal.Sep(p.String(","))).Map(vs => new JArray(vs));
            PrintResult(p.Run(array, "[1,true,null,\"fred\"]"));
        }
    }
}
