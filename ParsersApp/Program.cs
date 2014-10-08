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
            var p = new MyParser();

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

            {
                var jsonLiteral = p.Scope("literal",
                    p.String("null").As(new JNull() as Json)
                    .Or(() => p.String("true").As(new JBool(true) as Json))
                    .Or(() => p.String("false").As(new JBool(false) as Json)));
                PrintResult(p.Run(jsonLiteral, "null"));
                PrintResult(p.Run(jsonLiteral, "true"));
                PrintResult(p.Run(jsonLiteral, "false"));
                PrintResult(p.Run(jsonLiteral, "bogus"));
            }

            {
                //var result = p.Run(p.Surround(p.Char('{'), p.Char('}'), ???), "{}");
            }
        }

        private static void PrintResult<TA>(Either<ParseError, TA> either)
        {
            either.Match(
                Console.WriteLine,
                a => Console.WriteLine("a: {0}", a));
        }
    }
}
