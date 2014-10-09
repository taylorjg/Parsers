using System;
using System.Collections.Generic;
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

            ParseContextSensitive();

            ParseJsonLiteral();
            ParseJsonArray();
            ParseJsonKeyValueWithSimpleValues();
            ParseJsonKeyValueWithArrayValue();
            ParseJsonKeyValueWithObjectValue();
            ParseJsonObjectWithSimpleValues();
            ParseJsonObjectWithAnArrayProperty();
            ParseJsonObjectWithANestedObject();
            ParseJsonRoot();

            ParseCsvFixedColumnOrder();
            ParseCsvDateThenTemperature();
            ParseCsvTemperatureThenDate();
        }

        private static void PrintResult<TA>(Either<ParseError, TA> result)
        {
            result.Match(Console.WriteLine, a => Console.WriteLine(a));
        }

        private static void PrintResult<TA>(Either<ParseError, TA> result, Func<TA, string> f)
        {
            result.Match(Console.WriteLine, a => Console.WriteLine(f(a)));
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

        private static void ParseContextSensitive()
        {
            var p = new MyParserImpl();
            var parser = p.Double().Bind(n => p.ListOfN(Convert.ToInt32(n), p.Char('a')));
            Func<IEnumerable<char>, string> f = xs => string.Join("", xs);
            PrintResult(p.Run(parser, "0"), f);
            PrintResult(p.Run(parser, "1a"), f);
            PrintResult(p.Run(parser, "2aa"), f);
            PrintResult(p.Run(parser, "3aaa"), f);
            PrintResult(p.Run(parser, "7aaaaaaa"), f);
        }

        private static Parser<Json> JsonLiteral(ParsersBase p)
        {
            return p.Scope(
                "literal",
                p.Token(p.String("null")).As(new JNull() as Json) |
                (() => p.Double().Map(n => new JNumber(n) as Json)) |
                (() => p.Token(p.Quoted()).Map(s => new JString(s) as Json)) |
                (() => p.Token(p.String("true")).As(new JBool(true) as Json)) |
                (() => p.Token(p.String("false")).As(new JBool(false) as Json)));
        }

        private static Parser<Tuple<string, Json>> JsonKeyValue(ParsersBase p)
        {
            return p.Quoted().Product(() => p.SkipL(p.Token(p.String(":")), () => JsonValue(p)));
        }

        private static Parser<Json> JsonArray(ParsersBase p)
        {
            return p.Surround(
                p.Token(p.String("[")),
                p.Token(p.String("]")),
                () => JsonValue(p).Sep(p.Token(p.String(","))))
                    .Map(vs => new JArray(vs) as Json)
                    .Scope("array");
        }

        private static Parser<Json> JsonObject(ParsersBase p)
        {
            return p.Surround(
                p.Token(p.String("{")),
                p.Token(p.String("}")),
                () => JsonKeyValue(p).Sep(p.Token(p.String(",")))
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

        private static Parser<Json> JsonRoot(ParsersBase p)
        {
            return p.Whitespace().SkipL(() => JsonObject(p) | (() => JsonArray(p)));
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
            PrintResult(p.Run(jsonKeyValue, "\"amount2\": 12.4"));
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

        private static void ParseJsonRoot()
        {
            var p = new MyParserImpl();
            var jsonRoot = JsonRoot(p);

            PrintResult(p.Run(jsonRoot, @"
{
    ""property1"": {
        ""age"": 12
    },
    ""property2"": {
        ""name"": ""Henry""
    }
}
                "));

            PrintResult(p.Run(jsonRoot, @"
{
    ""property1"": {
        ""age"": 14
    },
    ""property2"": {
        ""name"": ""Sally""
    },
    ""property3"": {
        ""numbers"": [
            1,
            2,
            3
        ]
    }
}
                "));
        }

        private static Parser<DateTime> DateParser(ParsersBase p)
        {
            return p.Double().SkipR(() => p.String("/")).Bind(
                day => p.Double().SkipR(() => p.String("/")).Bind(
                    month => p.Double().Bind(
                        year => Parser.Return(new DateTime(
                                                  Convert.ToInt32(year),
                                                  Convert.ToInt32(month),
                                                  Convert.ToInt32(day))))));
        }

        private static Parser<double> TemperatureParser(ParsersBase p)
        {
            return p.Token(p.Double());
        }

        private static Parser<Parser<Row>> HeaderParser(ParsersBase p)
        {
            var dateParser = DateParser(p);
            var temperatureParser = TemperatureParser(p);

            var rowParser1 = dateParser.Product(() => p.SkipL(p.Token(p.String(",")), () => temperatureParser)).Map(tuple => new Row(tuple.Item1, tuple.Item2));
            var rowParser2 = temperatureParser.Product(() => p.SkipL(p.Token(p.String(",")), () => dateParser)).Map(tuple => new Row(tuple.Item2, tuple.Item1));

            var hash = p.Token(p.String("#"));
            var date = p.Token(p.String("Date"));
            var comma = p.Token(p.String(","));
            var temperature = p.Token(p.String("Temperature"));

            return p.Whitespace().SkipL(
                () => p.Attempt(hash.SkipL(
                    () => date.SkipL(
                        () => comma.SkipL(
                            () => temperature)).Map(_ => rowParser1)))
                       ) |
                   (() => p.Thru("\n").Map(_ => rowParser2));
        }

        public class Row
        {
            public DateTime Date { get; private set; }
            public double Temperature { get; private set; }

            public Row(DateTime date, double temperature)
            {
                Date = date;
                Temperature = temperature;
            }

            public override string ToString()
            {
                return string.Format("Date: {0}, Temp: {1}", Date.ToString("d"), Temperature);
            }
        }

        private static void ParseCsvFixedColumnOrder()
        {
            var p = new MyParserImpl();
            var dateParser = DateParser(p);
            var temperatureParser = TemperatureParser(p);
            var rowParser = dateParser.Product(() => p.SkipL(p.Token(p.String(",")), () => temperatureParser)).Map(tuple => new Row(tuple.Item1, tuple.Item2));
            var rowsParser = p.Whitespace().SkipL(() => rowParser.Sep(p.Whitespace()));

            const string input = @"
1/1/2010, 25
2/1/2010, 28
3/1/2010, 42
4/1/2010, 53
";

            var result = p.Run(rowsParser, input);
            PrintResult(result, rows => string.Join(Environment.NewLine, rows.Select(row => row.ToString())));
        }

        private static void ParseCsvDateThenTemperature()
        {
            var p = new MyParserImpl();
            var headerParser = HeaderParser(p);

            const string input = @"
# Date, Temperature
1/1/2010, 25
2/1/2010, 28
3/1/2010, 42
4/1/2010, 53
";

            var rowsParser = p.Whitespace().SkipL(() => headerParser.Bind(rowParser => rowParser.Sep(p.Whitespace())));
            var result = p.Run(rowsParser, input);
            PrintResult(result, rows => string.Join(Environment.NewLine, rows.Select(row => row.ToString())));
        }

        private static void ParseCsvTemperatureThenDate()
        {
            var p = new MyParserImpl();
            var headerParser = HeaderParser(p);

            const string input = @"
# Temperature, Date
25, 1/1/2010
28, 2/1/2010
42, 3/1/2010
53, 4/1/2010
";

            var rowsParser = p.Whitespace().SkipL(() => headerParser.Bind(rowParser => rowParser.Sep(p.Whitespace())));
            var result = p.Run(rowsParser, input);
            PrintResult(result, rows => string.Join(Environment.NewLine, rows.Select(row => row.ToString())));
        }
    }
}
