using System;
using System.Linq;
using ParsersLib;

namespace ParsersApp
{
    public static class CsvParsing
    {
        public static void CsvParsingDemo()
        {
            ParseCsvFixedColumnOrder();
            ParseCsvDateThenTemperature();
            ParseCsvTemperatureThenDate();
        }

        private static Parser<DateTime> DateParser(ParsersBase p)
        {
            return p.Int().SkipR(() => p.String("/")).Bind(
                day => p.Int().SkipR(() => p.String("/")).Bind(
                    month => p.Int().Bind(
                        year => Parser.Return(new DateTime(year, month, day)))));
        }

        private static Parser<int> TemperatureParser(ParsersBase p)
        {
            return p.Token(p.Int());
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
            public int Temperature { get; private set; }

            public Row(DateTime date, int temperature)
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
            Program.PrintResult(result, rows => string.Join(Environment.NewLine, rows.Select(row => row.ToString())));
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
            Program.PrintResult(result, rows => string.Join(Environment.NewLine, rows.Select(row => row.ToString())));
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
            Program.PrintResult(result, rows => string.Join(Environment.NewLine, rows.Select(row => row.ToString())));
        }
    }
}
