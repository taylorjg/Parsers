namespace ParsersLib
{
    // ReSharper disable UnusedTypeParameter
    public class Result<TA>
    {
    }
    // ReSharper restore UnusedTypeParameter

    public class Success<TA> : Result<TA>
    {
        public TA Value { get; private set; }
        public int CharsConsumed { get; private set; }

        public Success(TA value, int charsConsumed)
        {
            Value = value;
            CharsConsumed = charsConsumed;
        }
    }

    public class Failure<TA> : Result<TA>
    {
        public ParseError ParseError { get; private set; }

        public Failure(ParseError parseError)
        {
            ParseError = parseError;
        }
    }
}
