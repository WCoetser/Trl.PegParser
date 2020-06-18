using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Trl.PegParser.Grammer.Operators;
using Trl.PegParser.Tokenization;

namespace Trl.PegParser.Grammer.ParserGenerator
{
    /// <summary>
    /// Generates grammer rules from a simple string specification.
    /// </summary>
    public class Generator<TTokenTypeName, TNonTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNonTerminalName : Enum
    {
        private Parser<TokenNames, RuleName, IParserGeneratorResult> _parser;
        private Tokenizer<TokenNames> _tokenizer;
        private readonly PegFacade<TokenNames, RuleName, IParserGeneratorResult> _inputPeg;
        private readonly PegFacade<TTokenTypeName, TNonTerminalName, TActionResult> _outputPeg;

        public Generator(PegFacade<TTokenTypeName, TNonTerminalName, TActionResult> outputPeg)
        {
            _inputPeg = new PegFacade<TokenNames, RuleName, IParserGeneratorResult>();
            _outputPeg = outputPeg;
            BuildTokenizer();
            BuildSemantics();
            BuildParser();
        }

        private void BuildSemantics()
        {
            _inputPeg.DefaultSemanticActions.SetNonTerminalAction(RuleName.Start,
                (_, _subresults) => new RuleCollectionResult<TTokenTypeName, TNonTerminalName, TActionResult>
                {
                    Rules = Enumerable.Empty<ParsingRule<TTokenTypeName, TNonTerminalName, TActionResult>>()
                });
        }

        private void BuildTokenizer()
        {
            _tokenizer = _inputPeg.Tokenizer(new[] {
                _inputPeg.Token(TokenNames.Identifier, new Regex(@"[a-zA-Z_][a-zA-Z\d_]*")),
                _inputPeg.Token(TokenNames.OpenSquare, new Regex(@"\[")),
                _inputPeg.Token(TokenNames.CloseSquare, new Regex(@"\]")),
                _inputPeg.Token(TokenNames.OpenRound, new Regex(@"\(")),
                _inputPeg.Token(TokenNames.CloseRound, new Regex(@"\)")),
                _inputPeg.Token(TokenNames.WhiteSpace, new Regex(@"\s+")),
                _inputPeg.Token(TokenNames.And, new Regex(@"\&")),
                _inputPeg.Token(TokenNames.Not, new Regex(@"\!")),
                _inputPeg.Token(TokenNames.OneOrMore, new Regex(@"\+")),
                _inputPeg.Token(TokenNames.ZeroOrMore, new Regex(@"\*")),
                _inputPeg.Token(TokenNames.Optional, new Regex(@"\?")),
                _inputPeg.Token(TokenNames.Choice, new Regex(@"\|")),
                _inputPeg.Token(TokenNames.Arrow, new Regex(@"\=\>")),
                _inputPeg.Token(TokenNames.SemiColon, new Regex(@";"))
            });
        }

        private void BuildParser()
        {
            var op = _inputPeg.Operators;

            var start = _inputPeg.Rule(RuleName.Start, 
                op.OneOrMore(
                    op.Sequence(op.NonTerminal(RuleName.Rule),
                                op.Optional(op.Terminal(TokenNames.SemiColon)))));

            var rule = _inputPeg.Rule(RuleName.Rule,
               op.Sequence(op.Terminal(TokenNames.Identifier),
                           op.Terminal(TokenNames.Arrow),
                           op.NonTerminal(RuleName.Operator)));

            var @operator = _inputPeg.Rule(RuleName.Operator,
               op.OrderedChoice(op.NonTerminal(RuleName.Choice),
                                op.NonTerminal(RuleName.Sequence),
                                op.NonTerminal(RuleName.Optional),
                                op.NonTerminal(RuleName.ZeroOrMore),
                                op.NonTerminal(RuleName.OneOrMore),
                                op.NonTerminal(RuleName.Terminal),
                                op.NonTerminal(RuleName.NonTerminal),
                                op.NonTerminal(RuleName.Brackets),
                                op.NonTerminal(RuleName.Empty),
                                op.NonTerminal(RuleName.And),
                                op.NonTerminal(RuleName.Not)
                                ));

            var and = _inputPeg.Rule(RuleName.And,
                op.Sequence(op.Terminal(TokenNames.And),
                           op.Terminal(TokenNames.OpenRound),
                           op.NonTerminal(RuleName.Operator),
                           op.Terminal(TokenNames.CloseRound)));

            var not = _inputPeg.Rule(RuleName.Not,
                op.Sequence(op.Terminal(TokenNames.Not),
                           op.Terminal(TokenNames.OpenRound),
                           op.NonTerminal(RuleName.Operator),
                           op.Terminal(TokenNames.CloseRound)));

            var oneOrMore = _inputPeg.Rule(RuleName.OneOrMore,
                op.Sequence(op.NonTerminal(RuleName.Operator),
                           op.Terminal(TokenNames.OneOrMore)));

            var zeroOrMore = _inputPeg.Rule(RuleName.ZeroOrMore,
                op.Sequence(op.NonTerminal(RuleName.Operator),
                           op.Terminal(TokenNames.ZeroOrMore)));

            var terminal = _inputPeg.Rule(RuleName.Terminal,
               op.Sequence(op.Terminal(TokenNames.OpenSquare),
                           op.Terminal(TokenNames.Identifier),
                           op.Terminal(TokenNames.CloseSquare)));

            var emptyString = _inputPeg.Rule(RuleName.Empty,
               op.Sequence(op.Terminal(TokenNames.OpenSquare),
                           op.Terminal(TokenNames.CloseSquare)));

            var nonterminal = _inputPeg.Rule(RuleName.NonTerminal, 
                op.Terminal(TokenNames.Identifier));

            var optional = _inputPeg.Rule(RuleName.Optional,
                op.Sequence(op.NonTerminal(RuleName.Operator),
                           op.Terminal(TokenNames.Optional)));

            var sequence = _inputPeg.Rule(RuleName.Sequence,
                op.Sequence(op.NonTerminal(RuleName.Operator),
                           op.NonTerminal(RuleName.Operator)));

            var brackets = _inputPeg.Rule(RuleName.Brackets,
                op.Sequence(op.Terminal(TokenNames.OpenRound),
                            op.NonTerminal(RuleName.Operator),
                            op.Terminal(TokenNames.CloseRound)));

            var choice = _inputPeg.Rule(RuleName.Choice,
                op.Sequence(op.NonTerminal(RuleName.Operator),
                            op.Terminal(TokenNames.Choice),
                            op.NonTerminal(RuleName.Operator)));

            _parser = _inputPeg.Parser(RuleName.Start, new[]
            {
                start, rule, sequence, @operator, terminal, optional, brackets, choice, nonterminal,
                emptyString, oneOrMore, zeroOrMore, and, not
            });
        }

        public List<ParsingRule<TTokenTypeName, TNonTerminalName, TActionResult>>
            GetParsingRules(string rulesSpecification)
        {
            var tokens = _tokenizer.Tokenize(rulesSpecification);
            if (!tokens.Succeed)
            {
                throw new Exception("Invalid input");
            }
            var tokensNoWhitespace = tokens.MatchedRanges.Where(t => t.TokenName != TokenNames.WhiteSpace).ToList().AsReadOnly();
            var parseResult = _parser.Parse(tokensNoWhitespace);
            if (!parseResult.Succeed)
            {
                throw new Exception("Invalid input");
            }
            var ruleCollection = (RuleCollectionResult<TTokenTypeName, TNonTerminalName, TActionResult>)parseResult.SemanticActionResult;
            return ruleCollection.Rules.ToList();
        }
    }
}
