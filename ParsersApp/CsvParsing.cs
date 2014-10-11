using System;
using System.Collections.Generic;
using System.Linq;
using ParsersLib;

// ReSharper disable ImplicitlyCapturedClosure

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

        private static Parser<TA> AddOptionalCommaTokenSuffix<TA>(ParsersBase p, Parser<TA> thingParser)
        {
            return thingParser.SkipR(() => p.Opt(p.Token(p.String(","))));
        }

        private static Parser<DateTime> DateParser(ParsersBase p)
        {
            var dateParser = p.Int().SkipR(() => p.String("/")).Bind(
                day => p.Int().SkipR(() => p.String("/")).Bind(
                    month => p.Int().Bind(
                        year => Parser.Return(new DateTime(year, month, day)))));
            return AddOptionalCommaTokenSuffix(p, dateParser);
        }

        private static Parser<int> TemperatureParser(ParsersBase p)
        {
            var temperatureParser = p.Token(p.Int());
            return AddOptionalCommaTokenSuffix(p, temperatureParser);
        }

        private static Parser<IEnumerable<string>> ColumnTitlesParser(ParsersBase p)
        {
            var beginLineCommentParser = p.Token(p.String("#"));
            var commaParser = p.Token(p.String(","));
            var columnTitleParser = p.Token(p.Quoted() | (() => p.R(@"(\w+)")));

            return p.Whitespace()
                    .SkipL(
                        () => beginLineCommentParser.SkipL(
                            () => columnTitleParser.Sep(commaParser)));
        }

        private static Parser<Parser<Row>> HeaderParser(ParsersBase p)
        {
            var dateParser = DateParser(p);
            var temperatureParser = TemperatureParser(p);

            var rowParser1 = p.Map2(dateParser, () => temperatureParser, Row.MakeRowFunc);
            var rowParser2 = p.Map2(temperatureParser, () => dateParser, Flip(Row.MakeRowFunc));

            var columnTitlesParser = ColumnTitlesParser(p);
            return columnTitlesParser.Map(cols =>
                {
                    var colsList1 = cols.ToList();
                    if (colsList1.SequenceEqual(new[] { "Date", "Temperature" })) return rowParser1;
                    if (colsList1.SequenceEqual(new[] { "Temperature", "Date" })) return rowParser2;
                    return p.Fail<Row>("");
                });
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

            public static Row MakeRow(DateTime date, int temperature)
            {
                return new Row(date, temperature);
            }

            public static Func<DateTime, int, Row> MakeRowFunc
            {
                get { return MakeRow; }
            }
        }

        private static Func<TB, TA, TC> Flip<TA, TB, TC>(Func<TA, TB, TC> f)
        {
            return (a, b) => f(b, a);
        }

        private static void ParseCsvFixedColumnOrder()
        {
            var p = new MyParserImpl();
            var dateParser = DateParser(p);
            var temperatureParser = TemperatureParser(p);
            var rowParser = p.Map2(dateParser, () => temperatureParser, Row.MakeRowFunc);
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
            ParseCsvDynamically(@"
# Date, Temperature
1/1/2010, 25
2/1/2010, 28
3/1/2010, 42
4/1/2010, 53
");
        }

        private static void ParseCsvTemperatureThenDate()
        {
            ParseCsvDynamically(@"
# Temperature, Date
25, 1/1/2010
28, 2/1/2010
42, 3/1/2010
53, 4/1/2010
");
        }

        private static void ParseCsvDynamically(string input)
        {
            var p = new MyParserImpl();
            var rowsParser = HeaderParser(p).Bind(rowParser => rowParser.Sep(p.Whitespace()));
            var result = p.Run(rowsParser, input);
            Program.PrintResult(result, rows => string.Join(Environment.NewLine, rows.Select(row => row.ToString())));
        }
    }
}
