using System;
using System.Linq;
using System.Text.RegularExpressions;
using MonadLib;
using ParsersLib;

namespace ParsersApp
{
    internal class Program
    {
        private static void Main( /* string[] args */)
        {
            ParseSimpleThingsThatShouldSucceed();
            ParseSimpleThingsThatShouldFail();

            ParseJsonLiteral();
            ParseJsonKeyValue();
            ParseJsonArray();
            ParseJsonObjectWithSimpleValues();
            ParseJsonObjectWithAnObjectValue();
        }

        private static void PrintResult<TA>(Either<ParseError, TA> either)
        {
            either.Match(
                Console.WriteLine,
                a => Console.WriteLine("a: {0}", a));
        }

        private static void ParseSimpleThingsThatShouldSucceed()
        {
            var p = new MyParserImpl();
            PrintResult(p.Run(p.Char('c'), "c"));
            PrintResult(p.Run(p.String("abc"), "abcdefg"));
            PrintResult(p.Run(p.Slice(p.String("abc")), "abcdefg"));
            PrintResult(p.Run(p.Regex(new Regex(@"\dabc\d")), "5abc6"));
            PrintResult(p.Run(p.Double(), "12.4"));
            PrintResult(p.Run(p.Eof(), ""));
            PrintResult(p.Run(p.Whitespace(), "  "));
            PrintResult(p.Run(p.SkipL(p.Whitespace(), () => p.String("abc")), "  abc"));
            PrintResult(p.Run(p.SkipR(p.String("abc"), p.Whitespace), "abc  "));
            PrintResult(p.Run(p.Surround(p.Char('['), p.Char(']'), () => p.String("abc")), "[abc]"));
            PrintResult(p.Run(p.Root(p.String("abc")), "abc"));
            PrintResult(p.Run(p.Quoted(), "\"abc\""));
        }

        private static void ParseSimpleThingsThatShouldFail()
        {
            var p = new MyParserImpl();
            PrintResult(p.Run(p.Char('c'), "d"));
            PrintResult(p.Run(p.String("abc"), "defg"));
            PrintResult(p.Run(p.Eof(), "abc"));
            PrintResult(p.Run(p.Root(p.String("abc")), "abcblah"));
        }

        private static Parser<Json> JsonLiteral(ParsersBase p)
        {
            return p.Scope("literal",
                           p.String("null").As(new JNull() as Json)
                            .Or(() => p.Double().Map(n => new JNumber(n) as Json))
                            .Or(() => p.Quoted().Map(s => new JString(s) as Json))
                            .Or(() => p.String("true").As(new JBool(true) as Json))
                            .Or(() => p.String("false").As(new JBool(false) as Json)));
        }

        private static Parser<Tuple<string, Json>> JsonKeyValue(ParsersBase p)
        {
            return p.Quoted().Product(() => JsonValue(p).SkipL(p.String(":")));
        }

        private static Parser<Json> JsonArray(ParsersBase p)
        {
            return p.Surround(
                p.Char('['),
                p.Char(']'),
                () => JsonLiteral(p).Sep(p.String(",")))
                    .Map(vs => new JArray(vs) as Json)
                    .Scope("array");
        }

        private static Parser<Json> JsonObject(ParsersBase p)
        {
            return p.Surround(
                p.Char('{'),
                p.Char('}'),
                () => JsonKeyValue(p).Sep(p.String(","))
                                     .Map(kvs => new JObject(kvs.ToDictionary(kv => kv.Item1, kv => kv.Item2)) as Json))
                    .Scope("object");
        }

        private static Parser<Json> JsonValue(ParsersBase p)
        {
            return JsonLiteral(p).Or(() => JsonArray(p)).Or(() => JsonObject(p));
        }

        private static void ParseJsonLiteral()
        {
            var p = new MyParserImpl();
            var jsonLiteral = JsonLiteral(p);

            PrintResult(p.Run(jsonLiteral, "null"));
            PrintResult(p.Run(jsonLiteral, "12.4"));
            PrintResult(p.Run(jsonLiteral, "\"fred\""));
            PrintResult(p.Run(jsonLiteral, "true"));
            PrintResult(p.Run(jsonLiteral, "false"));
        }

        private static void ParseJsonKeyValue()
        {
            var p = new MyParserImpl();
            var jsonKeyValue = JsonKeyValue(p);

            PrintResult(p.Run(jsonKeyValue, "\"employee\":null"));
            PrintResult(p.Run(jsonKeyValue, "\"amount\":12.4"));
            PrintResult(p.Run(jsonKeyValue, "\"name\":\"Jon\""));
            PrintResult(p.Run(jsonKeyValue, "\"enabled\":true"));
            PrintResult(p.Run(jsonKeyValue, "\"enabled\":false"));
        }

        private static void ParseJsonArray()
        {
            var p = new MyParserImpl();
            var jsonArray = JsonArray(p);

            PrintResult(p.Run(jsonArray, "[1,true,null,\"fred\"]"));
        }

        private static void ParseJsonObjectWithSimpleValues()
        {
            var p = new MyParserImpl();
            var jsonObject = JsonObject(p);

            PrintResult(p.Run(jsonObject, "{\"property1\":1,\"property2\":null,\"property3\":12.4,\"property4\":true}"));
        }

        private static void ParseJsonObjectWithAnObjectValue()
        {
            var p = new MyParserImpl();
            var jsonObject = JsonObject(p);

            PrintResult(p.Run(jsonObject, "{\"property1\":{\"age\":12}}"));
        }
    }
}
