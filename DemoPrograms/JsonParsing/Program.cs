using System;
using MonadLib;
using ParsersLib;

namespace JsonParsing
{
    internal class Program
    {
        private static void Main()
        {
            JsonParsing.JsonParsingDemo();
        }

        public static void PrintResult<TA>(Either<ParseError, TA> result)
        {
            result.Match(Console.WriteLine, a => Console.WriteLine(a));
        }

        public static void PrintResult<TA>(Either<ParseError, TA> result, Func<TA, string> f)
        {
            result.Match(Console.WriteLine, a => Console.WriteLine(f(a)));
        }
    }
}
