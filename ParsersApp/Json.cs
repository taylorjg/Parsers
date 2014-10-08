using System;
using System.Collections.Generic;
using System.Linq;

namespace ParsersApp
{
    public abstract class Json
    {
    }

    public class JNull : Json
    {
        public override string ToString()
        {
            return "JNull";
        }
    }

    public class JNumber : Json
    {
        public double Number { get; private set; }

        public JNumber(double n)
        {
            Number = n;
        }

        public override string ToString()
        {
            return string.Format("JNumber({0})", Number);
        }
    }

    public class JString : Json
    {
        public string String { get; private set; }

        public JString(string s)
        {
            String = s;
        }

        public override string ToString()
        {
            return string.Format("JString(\"{0}\")", String);
        }
    }

    public class JBool : Json
    {
        public bool Bool { get; private set; }

        public JBool(bool b)
        {
            Bool = b;
        }

        public override string ToString()
        {
            return string.Format("JBool({0})", Bool ? "true" : "false");
        }
    }

    public class JArray : Json
    {
        public IEnumerable<Json> Elements { get; private set; }

        public JArray(IEnumerable<Json> elements)
        {
            Elements = elements;
        }

        public override string ToString()
        {
            var elementSeparator = "," + Environment.NewLine;
            return string.Format(
                "JArray: [{0}{1}{2}]",
                Environment.NewLine,
                string.Join(elementSeparator, Elements.Select(e => string.Format("    {0}", e.ToString()))),
                Environment.NewLine);
        }
    }

    public class JObject : Json
    {
        public IDictionary<string, Json> KeyValues { get; private set; }

        public JObject(IDictionary<string, Json> keyValues)
        {
            KeyValues = keyValues;
        }

        public override string ToString()
        {
            var elementSeparator = "," + Environment.NewLine;
            return string.Format(
                "JObject: {{{0}{1}{2}}}",
                Environment.NewLine,
                string.Join(elementSeparator, KeyValues.Select(kvp => string.Format("    \"{0}\": {1}", kvp.Key, kvp.Value.ToString()))),
                Environment.NewLine);
        }
    }

    //public class Fred
    //{
    //    private Json _json = new JObject(new Dictionary<string, Json>
    //        {
    //            {"Company name", new JString("Microsoft Corporation")},
    //            {"Ticker", new JString("MSFT")},
    //            {"Active", new JBool(true)},
    //            {"Price", new JNumber(30.66)},
    //            {
    //                "Related companies",
    //                new JArray(new[] {new JString("HPQ"), new JString("IBM"), new JString("YHOO"), new JString("DELL"), new JString("GOOG")})
    //            }
    //        });
    //}
}
