using System;
using System.Text.RegularExpressions;
using ParsersLib;

namespace ParsersApp
{
    internal class Program
    {
        private static void Main(/* string[] args */)
        {
            var myParser = new MyParser();

            {
                var result = myParser.Run(myParser.Char('c'), "c");
                Console.WriteLine("result: {0}", result);
            }

            {
                var result = myParser.Run(myParser.String("abc"), "abc");
                Console.WriteLine("result: {0}", result);
            }

            {
                var result = myParser.Run(myParser.Regex(new Regex(@"\dabc\d")), "5abc6");
                Console.WriteLine("result: {0}", result);
            }
        }
    }
}
