using System;
using System.Collections.Generic;
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

            var rowParser1 = dateParser.Product(() => p.SkipL(p.Token(p.String(",")), () => temperatureParser)).Map(tuple => new Row(tuple.Item1, tuple.Item2));
            var rowParser2 = temperatureParser.Product(() => p.SkipL(p.Token(p.String(",")), () => dateParser)).Map(tuple => new Row(tuple.Item2, tuple.Item1));

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
