using System.Collections.Generic;

namespace ParsersLib
{
    public class Json
    {
    }

    public class JNull : Json
    {
    }

    public class JNumber : Json
    {
        public double Number { get; private set; }

        public JNumber(double n)
        {
            Number = n;
        }
    }

    public class JString : Json
    {
        public string S { get; private set; }

        public JString(string s)
        {
            S = s;
        }
    }

    public class JBool : Json
    {
        public bool Bool { get; private set; }

        public JBool(bool b)
        {
            Bool = b;
        }
    }

    public class JArray : Json
    {
        public IEnumerable<Json> Elements { get; private set; }

        public JArray(IEnumerable<Json> elements)
        {
            Elements = elements;
        }
    }

    public class JObject : Json
    {
        public IDictionary<string, Json> Map { get; private set; }

        public JObject(IDictionary<string, Json> map)
        {
            Map = map;
        }
    }

    public class Fred
    {
        private Json _json = new JObject(new Dictionary<string, Json>
            {
                {"Company name", new JString("Microsoft Corporation")},
                {"Ticker", new JString("MSFT")},
                {"Active", new JBool(true)},
                {"Price", new JNumber(30.66)},
                {
                    "Related companies",
                    new JArray(new[] {new JString("HPQ"), new JString("IBM"), new JString("YHOO"), new JString("DELL"), new JString("GOOG")})
                }
            });
    }
}
