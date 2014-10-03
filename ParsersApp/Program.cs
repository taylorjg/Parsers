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
            var myParser = new MyParser();
            PrintResult(myParser.Run(myParser.Char('c'), "c"));
            PrintResult(myParser.Run(myParser.String("abc"), "abc"));
            PrintResult(myParser.Run(myParser.Regex(new Regex(@"\dabc\d")), "5abc6"));
        }

        private static void PrintResult<TA>(Either<ParseError, TA> either)
        {
            either.Match(
                Console.WriteLine,
                a => Console.WriteLine("a: {0}", a));
        }
    }
}
