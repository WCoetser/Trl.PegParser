using System;
using System.Linq;
using System.Text.RegularExpressions;
using Trl.PegParser.Grammer;
using Trl.PegParser.Tokenization;

namespace Trl.PegParser.SampleApp
{
    public enum Tokens { Plus, Number, Whitespace }

    public enum RuleNames { Add }

    class Program
    {
        static void Main(string[] args)
        {
            var peg = new PegFacade<Tokens, RuleNames, double?>();

            const string input = "1 + 2 + 3";

            // Define tokenizer
            Tokenizer<Tokens> tokenizer = GetTokenizer(peg);

            // Define semantic callback functions
            DefineSemanticActions(peg);

            // Define PEG rules
            Parser<Tokens, RuleNames, double?> parser = DefineParser(peg);

            // Tokenize input
            var tokenResult = tokenizer.Tokenize(input);
            if (!tokenResult.Succeed)
            {
                Console.WriteLine("Invalid input");
                return;
            }

            // Remove whitespace
            var inputNoWhitespace = tokenResult.MatchedRanges
                .Where(t => t.TokenName != Tokens.Whitespace)
                .ToList().AsReadOnly();

            // Parse
            var output = parser.Parse(inputNoWhitespace);
            if (!output.Succeed)
            {
                Console.WriteLine("Invalid input");
            }
            else
            {
                Console.WriteLine("Parse succeeded");
                Console.WriteLine($"Sum = {output.SemanticActionResult}");
            }
        }

        private static Parser<Tokens, RuleNames, double?> DefineParser(PegFacade<Tokens, RuleNames, double?> peg)
        {
            var grammer = @"Add => (Add [Plus] Add) | [Number]";
            var rules = peg.ParserGenerator.GetParsingRules(grammer);
            var parser = peg.Parser(RuleNames.Add, rules); // RuleNames.Add is the start symbol
            return parser;
        }

        private static void DefineSemanticActions(PegFacade<Tokens, RuleNames, double?> peg)
        {
            var semanticActions = peg.DefaultSemanticActions;
            semanticActions.OrderedChoiceAction = (matchedTokens, subresults) => subresults.First();
            semanticActions.SetTerminalAction(Tokens.Number,
                (matchedTokens, subresults) => double.Parse(matchedTokens.GetMatchedString()));            
            semanticActions.SetNonTerminalAction(RuleNames.Add,
                (matchedTokens, subresults) => subresults.First());
            semanticActions.SequenceAction = (matchedTokens, subresults) =>
            {
                // "+" will return a sub result of null because if does not have an
                // action associated with the Plus token via a SetTerminalAction call
                return subresults.Where(s => s.HasValue).Sum(s => s.Value);
            };
        }

        private static Tokenizer<Tokens> GetTokenizer(PegFacade<Tokens, RuleNames, double?> peg)
        {
            return peg.Tokenizer(new[] {
                peg.Token(Tokens.Plus, new Regex(@"\+")),
                peg.Token(Tokens.Number, new Regex(@"[-+]?[0-9]+\.?[0-9]*")),
                peg.Token(Tokens.Whitespace, new Regex(@"\s+"))
            });
        }
    }
}
