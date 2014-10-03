using System;
using System.Linq;
using MonadLib;

namespace ParsersLib
{
    public class Result<TA>
    {
        public Result<TB> Match<TB>(Func<Success<TA>, Result<TB>> successFunc, Func<Failure<TA>, Result<TB>> failureFunc)
        {
            if (this is Success<TA>)
            {
                var success = this as Success<TA>;
                return successFunc(success);
            }

            if (this is Failure<TA>)
            {
                var failure = this as Failure<TA>;
                return failureFunc(failure);
            }

            throw new Exception("'this' is neither Success nor Failure!");
        }

        public TB Match<TB>(Func<Success<TA>, TB> successFunc, Func<Failure<TA>, TB> failureFunc)
        {
            if (this is Success<TA>)
            {
                var success = this as Success<TA>;
                return successFunc(success);
            }

            if (this is Failure<TA>)
            {
                var failure = this as Failure<TA>;
                return failureFunc(failure);
            }

            throw new Exception("'this' is neither Success nor Failure!");
        }

        public Result<TA> MapError(Func<ParseError, ParseError> f)
        {
            return Match(
                success => success,
                failure => new Failure<TA>(f(failure.ParseError)));
        }

        public Result<TA> Uncommit()
        {
            return Match(
                success => success,
                failure => new Failure<TA>(failure.ParseError, false));
        }

        public Result<TA> AddCommit(bool isCommitted)
        {
            return Match(
                success => success,
                failure => new Failure<TA>(failure.ParseError, failure.IsCommitted || isCommitted));
        }

        public Result<TA> AdvanceSuccess(int n)
        {
            return Match(
                success => new Success<TA>(success.Value, success.CharsConsumed + n), 
                failure => failure);
        }

        public Either<ParseError, TA> Extract()
        {
            return Match(
                success => Either<ParseError>.Right(success.Value),
                failure => Either<ParseError>.Left<TA>(failure.ParseError));
        }
    }

    public class Success<TA> : Result<TA>
    {
        public TA Value { get; private set; }
        public int CharsConsumed { get; private set; }

        public Success(TA value, int charsConsumed)
        {
            Value = value;
            CharsConsumed = charsConsumed;
        }

        public override string ToString()
        {
            return string.Format("Success - Value: {0}, CharsConsumed: {1}", Value, CharsConsumed);
        }
    }

    public class Failure<TA> : Result<TA>
    {
        public ParseError ParseError { get; private set; }
        public bool IsCommitted { get; private set; }

        public Failure(ParseError parseError, bool isCommitted = true)
        {
            ParseError = parseError;
            IsCommitted = isCommitted;
        }

        public override string ToString()
        {
            var tuple = ParseError.Stack.First();
            if (tuple == null) return "Failure";
            var location = tuple.Item1;
            var message = tuple.Item2;
            var line = location.Line;
            var column = location.Column;
            return string.Format("Failure - message: {0}, line: {1}, column: {2}", message, line, column);
        }
    }
}
