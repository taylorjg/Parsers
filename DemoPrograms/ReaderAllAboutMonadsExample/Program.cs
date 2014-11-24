using System;
using System.Collections.Generic;
using System.Linq;
using MonadLib;
using ParsersLib;

namespace ReaderAllAboutMonadsExample
{
    public class Program
    {
        private static Maybe<string> LookupVar(string name, Environment env)
        {
            string v;
            return env.Variables.TryGetValue(name, out v) ? Maybe.Just(v) : Maybe.Nothing<string>();
        }

        private static Maybe<Template> LookupTemplate(string name, Environment env)
        {
            Template t;
            return env.Templates.TryGetValue(name, out t) ? Maybe.Just(t) : Maybe.Nothing<Template>();
        }

        private static Environment AddDefs(IEnumerable<KeyValuePair<string, string>> defs, Environment env)
        {
            var newDefs = defs.Concat(env.Variables).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return new Environment(env.Templates, newDefs);
        }

        private static Reader<Environment, KeyValuePair<string, string>> ResolveDef(Definition definition)
        {
            return Resolve(definition.TemplateT).Bind(
                name => Resolve(definition.TemplateD).Bind(
                    value => Reader<Environment>.Return(new KeyValuePair<string, string>(name, value))));
        }

        private static Reader<Environment, string> Resolve(Template template)
        {
            Func<string, Func<Environment, Maybe<string>>> partiallyApplyLookupVar = name => env => LookupVar(name, env);
            Func<string, Func<Environment, Maybe<Template>>> partiallyApplyLookupTemplate = name => env => LookupTemplate(name, env);

            if (template is TextTemplate)
            {
                return Reader<Environment>.Return((template as TextTemplate).String);
            }

            if (template is VariableTemplate)
            {
                var t = (template as VariableTemplate).Template;
                return Resolve(t).Bind(
                    varName => Reader.Asks(partiallyApplyLookupVar(varName)).Bind(
                        varValue => Reader<Environment>.Return(Maybe.MapOrDefault(string.Empty, x => x, varValue))));
            }

            if (template is QuoteTemplate)
            {
                var t = (template as QuoteTemplate).Template;
                return Resolve(t).Bind(
                    tmplName => Reader.Asks(partiallyApplyLookupTemplate(tmplName)).Bind(
                        body => Reader<Environment>.Return(Maybe.MapOrDefault(string.Empty, x => x.ToString(), body))));
            }

            if (template is IncludeTemplate)
            {
                var t = (template as IncludeTemplate).Template;
                var ds = (template as IncludeTemplate).Definitions;

                return Resolve(t).Bind(
                    tmplName => Reader.Asks(partiallyApplyLookupTemplate(tmplName)).Bind(
                        body => body.Match(
                            t2 => Reader.MapM(ResolveDef, ds).Bind(defs => Reader.Local(r => AddDefs(defs, r), Resolve(t2))),
                            () => Reader<Environment>.Return(string.Empty))));
            }

            if (template is CompoundTemplate)
            {
                var ts = (template as CompoundTemplate).Templates;
                return Reader.MapM(Resolve, ts).LiftM(string.Concat);
            }

            throw new InvalidOperationException("Unknown Template type");
        }

        // parseFromFile :: Parser a -> String -> IO (Either ParseError a)
        private static Either<ParseError, IEnumerable<NamedTemplate>> ParseFromFile(Parser<IEnumerable<NamedTemplate>> p, string filePath)
        {
            // http://hackage.haskell.org/package/parsec-3.1.2/docs/Text-Parsec-ByteString.html

            // parseFromFile p filePath runs a strict bytestring parser p on the input read from
            // filePath using readFile. Returns either a ParseError (Left) or a value of type a (Right).

            return null;
        }

        // parse :: Stream s Identity t => Parsec s () a -> SourceName -> s -> Either ParseError a
        private static Either<ParseError, TA> Parse<TA>(Parser<TA> p, string filePath, string s)
        {
            // http://hackage.haskell.org/package/parsec-3.0.0/docs/Text-Parsec-Prim.html

            // parse p filePath input runs a parser p over Identity without user state. The filePath
            // is only used in error messages and may be the empty string. Returns either a
            // ParseError (Left) or a value of type a (Right).

            return null;
        }

        // Read the command line arguments, parse the template file, the user template, and any
        // variable definitions.  Then construct the environment and print the resolved user template.
        private static void Main(string[] args)
        {
            // TODO: change Environment to use IImmutableList internally ?

            var tmplFile = args[0];
            var pattern = args[1];
            var defs = args.Skip(2);

            var templateParser = new TemplateParser(new MyParserImpl());
            var nts = ParseFromFile(templateParser.TemplateFile(), tmplFile);
            nts.Match(err => Console.Error.WriteLine(err), _ => { });

            var tmpl = Parse(templateParser.Template(string.Empty), "pattern", pattern);
            tmpl.Match(err => Console.Error.WriteLine(err), _ => { });

            var ds = defs.Select(d =>
                {
                    var pos = d.IndexOf('=');
                    return Tuple.Create(d.Substring(0, pos), d.Substring(pos + 1));
                });
            var ds2 = ds.ToDictionary(pair => pair.Item1, pair => pair.Item2);
            var ntl = nts.MapEither(_ => Enumerable.Empty<NamedTemplate>(), x => x);
            var env = new Environment(ntl.Select(nt => nt.StripName()).ToDictionary(pair => pair.Item1, pair => pair.Item2), ds2);
            var t = tmpl.MapEither(_ => new TextTemplate(string.Empty), x => x);
            var result = Resolve(t).RunReader(env);
            Console.WriteLine(result);
        }
    }
}
