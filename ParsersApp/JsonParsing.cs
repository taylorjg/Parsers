using System;
using System.Linq;
using ParsersLib;

namespace ParsersApp
{
    public static class JsonParsing
    {
        public static void JsonParsingDemo()
        {
            ParseJsonLiteral();
            ParseJsonArray();
            ParseJsonKeyValueWithSimpleValues();
            ParseJsonKeyValueWithArrayValue();
            ParseJsonKeyValueWithObjectValue();
            ParseJsonObjectWithSimpleValues();
            ParseJsonObjectWithAnArrayProperty();
            ParseJsonObjectWithANestedObject();
            ParseJsonRoot();
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

            Program.PrintResult(p.Run(jsonLiteral, "null"));
            Program.PrintResult(p.Run(jsonLiteral, "12.4"));
            Program.PrintResult(p.Run(jsonLiteral, "\"fred\""));
            Program.PrintResult(p.Run(jsonLiteral, "true"));
            Program.PrintResult(p.Run(jsonLiteral, "false"));
        }

        private static void ParseJsonKeyValueWithSimpleValues()
        {
            var p = new MyParserImpl();
            var jsonKeyValue = JsonKeyValue(p);

            Program.PrintResult(p.Run(jsonKeyValue, "\"employee\":null"));
            Program.PrintResult(p.Run(jsonKeyValue, "\"amount\":12.4"));
            Program.PrintResult(p.Run(jsonKeyValue, "\"name\":\"Jon\""));
            Program.PrintResult(p.Run(jsonKeyValue, "\"enabled\":true"));
            Program.PrintResult(p.Run(jsonKeyValue, "\"enabled\":false"));
            Program.PrintResult(p.Run(jsonKeyValue, "\"amount2\": 12.4"));
        }

        private static void ParseJsonKeyValueWithArrayValue()
        {
            var p = new MyParserImpl();
            var jsonKeyValue = JsonKeyValue(p);

            Program.PrintResult(p.Run(jsonKeyValue, "\"property1\":[1,2,3]"));
        }

        private static void ParseJsonKeyValueWithObjectValue()
        {
            var p = new MyParserImpl();
            var jsonKeyValue = JsonKeyValue(p);

            Program.PrintResult(p.Run(jsonKeyValue, "\"property1\":{\"name\":null,\"age\":12}"));
        }

        private static void ParseJsonArray()
        {
            var p = new MyParserImpl();
            var jsonArray = JsonArray(p);

            Program.PrintResult(p.Run(jsonArray, "[1,true,null,\"fred\"]"));
        }

        private static void ParseJsonObjectWithSimpleValues()
        {
            var p = new MyParserImpl();
            var jsonObject = JsonObject(p);

            Program.PrintResult(p.Run(jsonObject, "{\"property1\":1,\"property2\":null,\"property3\":12.4,\"property4\":true}"));
            Program.PrintResult(p.Run(jsonObject, "{\"property1\":1,\"property2\":null,\"property3\":true,\"property4\":12.4}"));
        }

        private static void ParseJsonObjectWithAnArrayProperty()
        {
            var p = new MyParserImpl();
            var jsonObject = JsonObject(p);

            Program.PrintResult(p.Run(jsonObject, "{\"property1\":[1,2,3]}"));
        }

        private static void ParseJsonObjectWithANestedObject()
        {
            var p = new MyParserImpl();
            var jsonObject = JsonObject(p);

            Program.PrintResult(p.Run(jsonObject, "{\"property1\":{\"age\":12}}"));
        }

        private static void ParseJsonRoot()
        {
            var p = new MyParserImpl();
            var jsonRoot = JsonRoot(p);

            Program.PrintResult(p.Run(jsonRoot, @"
{
    ""property1"": {
        ""age"": 12
    },
    ""property2"": {
        ""name"": ""Henry""
    }
}
                "));

            Program.PrintResult(p.Run(jsonRoot, @"
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
    }
}
