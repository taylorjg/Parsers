using System.Collections.Generic;
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
            return null;
        }

        public Parser<string> End()
        {
            return _p.String("[END]") | (() => _p.Fail<string>("[END]"));
        }

        public Parser<Template> Template(string except)
        {
            return null;
        }

        public Parser<Template> SimpleTemplate(string except)
        {
            return null;
        }

        public Parser<char> Dollar()
        {
            return null;
        }

        public Parser<char> LeftBracket()
        {
            return null;
        }

        public Parser<char> TextChar(string except)
        {
            return null;
        }

        public Parser<Template> Text(string except)
        {
            return null;
        }

        public Parser<Template> Variable()
        {
            return null;
        }

        public Parser<Template> Quote()
        {
            return null;
        }

        public Parser<Template> Include()
        {
            return null;
        }

        public Parser<Template> IncludeBody()
        {
            return null;
        }

        public Parser<IEnumerable<Definition>> Definitions()
        {
            return null;
        }

        public Parser<Definition> Definition()
        {
            return null;
        }
    }
}
