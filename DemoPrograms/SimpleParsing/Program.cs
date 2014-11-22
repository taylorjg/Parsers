using System;
using MonadLib;
using ParsersLib;

namespace SimpleParsing
{
    internal class Program
    {
        private static void Main()
        {
            SimpleParsing.SimpleParsingDemo();
        }

        public static void PrintResult<TA>(Either<ParseError, TA> result)
        {
            result.Match(Console.WriteLine, a => Console.WriteLine(a));
        }
    }
}
