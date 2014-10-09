using System;
using System.Linq;
using System.Text.RegularExpressions;
using MonadLib;
using ParsersLib;

namespace ParsersApp
{
    internal class Program
    {
        private static void Main()
        {
            ParseSimpleThingsThatShouldSucceed();
            ParseSimpleThingsThatShouldFail();

            ParseJsonLiteral();
            ParseJsonArray();
            ParseJsonKeyValueWithSimpleValues();
            ParseJsonKeyValueWithArrayValue();
            ParseJsonKeyValueWithObjectValue();
            ParseJsonObjectWithSimpleValues();
            ParseJsonObjectWithAnArrayProperty();
            ParseJsonObjectWithANestedObject();
        }

        private static void PrintResult<TA>(Either<ParseError, TA> result)
        {
            result.Match(Console.WriteLine, a => Console.WriteLine(a));
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
            return p.Scope(
                "literal",
                p.String("null").As(new JNull() as Json) |
                (() => p.Double().Map(n => new JNumber(n) as Json)) |
                (() => p.Quoted().Map(s => new JString(s) as Json)) |
                (() => p.String("true").As(new JBool(true) as Json)) |
                (() => p.String("false").As(new JBool(false) as Json)));
        }

        private static Parser<Tuple<string, Json>> JsonKeyValue(ParsersBase p)
        {
            return p.Quoted().Product(() => p.SkipL(p.String(":"), () => JsonValue(p)));
        }

        private static Parser<Json> JsonArray(ParsersBase p)
        {
            return p.Surround(
                p.Char('['),
                p.Char(']'),
                () => JsonValue(p).Sep(p.String(",")))
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
            return
                JsonLiteral(p) |
                (() => JsonArray(p)) |
                (() => JsonObject(p));
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

        private static void ParseJsonKeyValueWithSimpleValues()
        {
            var p = new MyParserImpl();
            var jsonKeyValue = JsonKeyValue(p);

            PrintResult(p.Run(jsonKeyValue, "\"employee\":null"));
            PrintResult(p.Run(jsonKeyValue, "\"amount\":12.4"));
            PrintResult(p.Run(jsonKeyValue, "\"name\":\"Jon\""));
            PrintResult(p.Run(jsonKeyValue, "\"enabled\":true"));
            PrintResult(p.Run(jsonKeyValue, "\"enabled\":false"));
        }

        private static void ParseJsonKeyValueWithArrayValue()
        {
            var p = new MyParserImpl();
            var jsonKeyValue = JsonKeyValue(p);

            PrintResult(p.Run(jsonKeyValue, "\"property1\":[1,2,3]"));
        }

        private static void ParseJsonKeyValueWithObjectValue()
        {
            var p = new MyParserImpl();
            var jsonKeyValue = JsonKeyValue(p);

            PrintResult(p.Run(jsonKeyValue, "\"property1\":{\"name\":null,\"age\":12}"));
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
            PrintResult(p.Run(jsonObject, "{\"property1\":1,\"property2\":null,\"property3\":true,\"property4\":12.4}"));
        }

        private static void ParseJsonObjectWithAnArrayProperty()
        {
            var p = new MyParserImpl();
            var jsonObject = JsonObject(p);

            PrintResult(p.Run(jsonObject, "{\"property1\":[1,2,3]}"));
        }

        private static void ParseJsonObjectWithANestedObject()
        {
            var p = new MyParserImpl();
            var jsonObject = JsonObject(p);

            PrintResult(p.Run(jsonObject, "{\"property1\":{\"age\":12}}"));
        }
    }
}
