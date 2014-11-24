using System.Collections.Generic;
using System.Linq;
using ParsersLib;

namespace ReaderAllAboutMonadsExample
{
    public class TemplateParser
    {
        private readonly ParsersBase _p;

        public TemplateParser(ParsersBase p)
        {
            _p = p;
        }

        public Parser<IEnumerable<NamedTemplate>> TemplateFile()
        {
            return _p.Many(NamedTemplate()).Bind(
                nts => _p.Eof().Bind(
                    _ => Parser.Return(nts)));
        }

        public Parser<NamedTemplate> NamedTemplate()
        {
            return Name().Bind(
                n => (Template(string.Empty) | (() => _p.Fail<Template>("template"))).Bind(
                    t => End().BindIgnoringLeft(
                        _p.Whitespace().BindIgnoringLeft(
                            Parser.Return(new NamedTemplate(n, t))))));
        }

        public Parser<string> Name()
        {
            return _p.Surround(
                _p.String("["),
                _p.String("]"),
                () => _p.Many1(_p.NoneOf("]")).Map(cs => new string(cs.ToArray()))) |
                   (() => _p.Fail<string>("label"));
        }

        public Parser<string> End()
        {
            return _p.String("[END]") | (() => _p.Fail<string>("[END]"));
        }

        public Parser<Template> Template(string except)
        {
            return _p.Many1(SimpleTemplate(except)).Map(ts => ts.ToList()).Bind(
                ts => ts.Count == 1
                          ? Parser.Return(ts.First())
                          : Parser.Return(new CompoundTemplate(ts) as Template));
        }

        public Parser<Template> SimpleTemplate(string except)
        {
            return Text(except) | 
                   (() => _p.Attempt(Variable())) |
                   (() => _p.Attempt(Quote())) |
                   Include;
        }

        public Parser<char> Dollar()
        {
            return _p.Attempt(_p.Char('$').Bind(
                c => _p.NotFollowedBy(_p.OneOf("{<\"")).BindIgnoringLeft(
                    Parser.Return(c)))); // <?> ""
        }

        // http://hackage.haskell.org/package/parsec-3.0.0/docs/Text-ParserCombinators-Parsec-Prim.html
        // http://hackage.haskell.org/package/parsec-3.0.0/docs/src/Text-ParserCombinators-Parsec-Prim.html#pzero
        // pzero :: GenParser tok st a
        // pzero = parserZero
        //
        // http://hackage.haskell.org/package/parsec-3.0.0/docs/src/Text-Parsec-Prim.html#parserZero
        // parserZero :: (Monad m) => ParsecT s u m a
        // parserZero = ParsecT $ \s -> return $ Empty $ return $ Error (unknownError s)
        //
        // parserZero always fails without consuming any input. parserZero is defined equal
        // to the mzero member of the MonadPlus class and to the Control.Applicative.empty
        // member of the Control.Applicative.Applicative class.
        //
        // http://hackage.haskell.org/package/parsec-3.0.0/docs/Text-Parsec-Prim.html
        // http://hackage.haskell.org/package/parsec-3.0.0/docs/src/Text-Parsec-Prim.html#Consumed
        // data Consumed a  = Consumed a | Empty !a

        public Parser<char> LeftBracket()
        {
            // TODO: pzero instead of Fail ?
            return _p.Attempt((_p.Attempt(End()) | (() => _p.String("["))).Bind(
                s => s == "[END]" ? _p.Fail<char>("") : Parser.Return('['))); // <?> ""
        }

        public Parser<char> TextChar(string except)
        {
            return _p.NoneOf("$[" + except) | Dollar | LeftBracket;
        }

        public Parser<Template> Text(string except)
        {
            return _p.Many1(TextChar(except)).Map(cs => new string(cs.ToArray())).Bind(
                str => Parser.Return(new TextTemplate(str) as Template)) | (() => _p.Fail<Template>("text"));
        }

        public Parser<Template> Variable()
        {
            return _p.Surround(_p.String("${"), _p.String("}"), () => Template("}")).Bind(
                t => Parser.Return(new VariableTemplate(t) as Template)) |
                   (() => _p.Fail<Template>("variable pattern"));
        }

        public Parser<Template> Quote()
        {
            return _p.Surround(_p.String("$\""), _p.String("\""), () => Template("\"")).Bind(
                t => Parser.Return(new QuoteTemplate(t) as Template)) |
                   (() => _p.Fail<Template>("quoted include pattern"));
        }

        public Parser<Template> Include()
        {
            return _p.Surround(_p.String("$<"), _p.String(">"), IncludeBody) |
                   (() => _p.Fail<Template>("include pattern"));
        }

        public Parser<Template> IncludeBody()
        {
            return Template("|>").Bind(
                t => _p.Option(Enumerable.Empty<Definition>(), Definitions()).Map(
                    ds => ds.ToList()).Bind(
                        ds => Parser.Return(new IncludeTemplate(t, ds) as Template)));
        }

        public Parser<IEnumerable<Definition>> Definitions()
        {
            return _p.Char('|').BindIgnoringLeft(_p.SepBy(Definition(), _p.Char(',')));
        }

        public Parser<Definition> Definition()
        {
            return Template("=,>").Bind(
                t1 => _p.Char('=').BindIgnoringLeft(Template(",>")).Bind(
                    t2 => Parser.Return(new Definition(t1, t2)))) | (() => _p.Fail<Definition>("variable definition"));
        }
    }
}
