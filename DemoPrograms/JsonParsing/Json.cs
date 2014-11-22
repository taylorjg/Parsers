using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonParsing
{
    public abstract class Json
    {
        protected string Padding(int level)
        {
            return new string(' ', level * 4);
        }

        public abstract string FormatString(int level);
    }

    public class JNull : Json
    {
        public override string FormatString(int level)
        {
            return string.Format("JNull");
        }

        public override string ToString()
        {
            return FormatString(0);
        }
    }

    public class JNumber : Json
    {
        public double Number { get; private set; }

        public JNumber(double n)
        {
            Number = n;
        }

        public override string FormatString(int level)
        {
            return string.Format("JNumber({0})", Number);
        }

        public override string ToString()
        {
            return FormatString(0);
        }
    }

    public class JString : Json
    {
        public string String { get; private set; }

        public JString(string s)
        {
            String = s;
        }

        public override string FormatString(int level)
        {
            return string.Format("JString(\"{0}\")", String);
        }

        public override string ToString()
        {
            return FormatString(0);
        }
    }

    public class JBool : Json
    {
        public bool Bool { get; private set; }

        public JBool(bool b)
        {
            Bool = b;
        }

        public override string FormatString(int level)
        {
            return string.Format("JBool({0})", Bool ? "true" : "false");
        }

        public override string ToString()
        {
            return FormatString(0);
        }
    }

    public class JArray : Json
    {
        public IEnumerable<Json> Elements { get; private set; }

        public JArray(IEnumerable<Json> elements)
        {
            Elements = elements;
        }

        public override string FormatString(int level)
        {
            var elementSeparator = "," + Environment.NewLine;
            return string.Format(
                "JArray: [{0}{1}{2}{3}]",
                Environment.NewLine,
                string.Join(elementSeparator, Elements.Select(e => string.Format("{0}{1}", Padding(level + 1), e.FormatString(level + 1)))),
                Environment.NewLine,
                Padding(level));
        }

        public override string ToString()
        {
            return FormatString(0);
        }
    }

    public class JObject : Json
    {
        public IDictionary<string, Json> KeyValues { get; private set; }

        public JObject(IDictionary<string, Json> keyValues)
        {
            KeyValues = keyValues;
        }

        public override string FormatString(int level)
        {
            var elementSeparator = "," + Environment.NewLine;
            return string.Format(
                "JObject: {{{0}{1}{2}{3}}}",
                Environment.NewLine,
                string.Join(elementSeparator, KeyValues.Select(kvp => string.Format("{0}\"{1}\": {2}", Padding(level + 1), kvp.Key, kvp.Value.FormatString(level + 1)))),
                Environment.NewLine,
                Padding(level));
        }

        public override string ToString()
        {
            return FormatString(0);
        }
    }
}
