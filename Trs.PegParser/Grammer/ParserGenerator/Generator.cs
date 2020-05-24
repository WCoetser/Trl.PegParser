using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Trs.PegParser.Grammer.Operators;
using Trs.PegParser.Tokenization;

namespace Trs.PegParser.Grammer.ParserGenerator
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
            _inputPeg.DefaultSemanticActions.SetDefaultGenericPassthroughAction<GenericAstResult>();

            _inputPeg.DefaultSemanticActions.SetNonTerminalAction(RuleName.Terminal,
            (_, subresults) =>
            {
                var result = (GenericAstResult)subresults.First();
                var terminalName = ((GenericAstResult)result.SubResults[1]).MatchedTokens.GetMatchedString();

                if (!Enum.TryParse(typeof(TTokenTypeName), terminalName, out object nameObj))
                {
                    throw new Exception($"Invalid nonterminal name given: {terminalName} is undeclared in {nameof(TTokenTypeName)}");
                }
                TTokenTypeName name = (TTokenTypeName)nameObj;
                return new OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult>
                {
                    Operator = _outputPeg.Operators.Terminal(name)
                };
            });

            _inputPeg.DefaultSemanticActions.SetNonTerminalAction(RuleName.Start, (_, subResults) =>
            {
                var oneOrMore = ((GenericAstResult)subResults.First()).SubResults.Cast<RuleAstResult<TTokenTypeName, TNonTerminalName, TActionResult>>();
                var ruleCollectionResult = new RuleCollectionResult<TTokenTypeName, TNonTerminalName, TActionResult>();
                ruleCollectionResult.Rules = oneOrMore.Select(ruleAstResult => _outputPeg.Rule(ruleAstResult.RuleName, ruleAstResult.Operator));
                return ruleCollectionResult;
            });

            _inputPeg.DefaultSemanticActions.SetNonTerminalAction(RuleName.Rule, (_, subResults) =>
            {
                var resultsArray = ((GenericAstResult)subResults.First()).SubResults;
                var resultNonTermnial = (GenericAstResult)resultsArray[0];
                var nonTerminalName = resultNonTermnial.MatchedTokens.GetMatchedString();
                if (!Enum.TryParse(typeof(TNonTerminalName), nonTerminalName, out object nameObj))
                {
                    throw new Exception($"Invalid nonterminal name given: {nonTerminalName} is undeclared in {nameof(TNonTerminalName)}");
                }
                TNonTerminalName name = (TNonTerminalName)nameObj;
                return new RuleAstResult<TTokenTypeName, TNonTerminalName, TActionResult>
                {

                    RuleName = name,
                    Operator = ((OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult>)resultsArray[2]).Operator
                };
            });

            _inputPeg.DefaultSemanticActions.SetNonTerminalAction(RuleName.NonTerminal,
                (_, subresults) =>
                {
                    var result = (GenericAstResult)subresults.First();
                    var nonTerminalName = result.MatchedTokens.GetMatchedString();

                    if (!Enum.TryParse(typeof(TNonTerminalName), nonTerminalName, out object nameObj))
                    {
                        throw new Exception($"Invalid nonterminal name given: {nonTerminalName} is undeclared in {nameof(TNonTerminalName)}");
                    }
                    TNonTerminalName name = (TNonTerminalName)nameObj;
                    return new OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult>
                    {
                        Operator = _outputPeg.Operators.NonTerminal(name)
                    };
                });

            _inputPeg.DefaultSemanticActions.SetNonTerminalAction(RuleName.Empty,
                (_, subresults) =>
                {
                    return new OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult>
                    {
                        Operator = _outputPeg.Operators.EmptyString()
                    };
                });

            _inputPeg.DefaultSemanticActions.SetNonTerminalAction(RuleName.Operator, (_, subResults) =>
            {
                var subResult = ((GenericAstResult)subResults.First()).SubResults[0] as OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult>;

                if (subResult != null
                    && (subResult.Operator is Terminal<TTokenTypeName, TNonTerminalName, TActionResult>
                        || subResult.Operator is NonTerminal<TTokenTypeName, TNonTerminalName, TActionResult>
                        || subResult.Operator is EmptyString<TTokenTypeName, TNonTerminalName, TActionResult>))
                {
                    return subResult;
                }

                throw new NotImplementedException();
            });
        }

        private void BuildTokenizer()
        {
            _tokenizer = _inputPeg.Tokenizer(new[] {
                _inputPeg.Token(TokenNames.Identifier, new Regex(@"[a-zA-Z_][a-zA-Z0-9_]*")),
                _inputPeg.Token(TokenNames.OpenSquare, new Regex(@"\[")),
                _inputPeg.Token(TokenNames.CloseSquare, new Regex(@"\]")),
                _inputPeg.Token(TokenNames.OpenRound, new Regex(@"\[")),
                _inputPeg.Token(TokenNames.CloseRound, new Regex(@"\]")),
                _inputPeg.Token(TokenNames.WhiteSpace, new Regex(@"\s+")),
                _inputPeg.Token(TokenNames.And, new Regex(@"\&")),
                _inputPeg.Token(TokenNames.Not, new Regex(@"\!")),
                _inputPeg.Token(TokenNames.OneOrMore, new Regex(@"\+")),
                _inputPeg.Token(TokenNames.ZeroOrMore, new Regex(@"\*")),
                _inputPeg.Token(TokenNames.Optional, new Regex(@"\?")),
                _inputPeg.Token(TokenNames.Choice, new Regex(@"OR")),
                _inputPeg.Token(TokenNames.Arrow, new Regex(@"\=\>")),
                _inputPeg.Token(TokenNames.SemiColon, new Regex(@";"))
            });
        }

        private void BuildParser()
        {
            var ruleCollection = BuildRuleCollection();
            var rule = BuildRule();
            var @operator = BuildOperator();
            var and = BuildAndPredicate();
            var empty = BuildEmpty();
            var nonTerminal = BuildNonTerminal();
            var not = BuildNotPredicate();
            var brackets = BuildBrackets();
            var terminal = BuildTerminal();
            var leftRecursionCluster = BuildLeftRecursionCluster();

            _parser = _inputPeg.Parser(RuleName.Start, new[]
            {
                ruleCollection, rule, @operator, and, empty, nonTerminal, not, brackets,
                terminal, leftRecursionCluster
            });
        }

        private ParsingRule<TokenNames, RuleName, IParserGeneratorResult> BuildRuleCollection()
        {
            var op = _inputPeg.Operators;

            // Start, a parser is a collection of one or more rules
            return _inputPeg.Rule(RuleName.Start, op.OneOrMore(op.NonTerminal(RuleName.Rule)));
        }

        private ParsingRule<TokenNames, RuleName, IParserGeneratorResult> BuildRule()
        {
            var op = _inputPeg.Operators;

            return _inputPeg.Rule(RuleName.Rule,
                op.Sequence(op.Terminal(TokenNames.Identifier),
                            op.Terminal(TokenNames.Arrow),
                            op.NonTerminal(RuleName.Operator),
                            op.Optional(op.Terminal(TokenNames.SemiColon))));
        }

        private ParsingRule<TokenNames, RuleName, IParserGeneratorResult> BuildLeftRecursionCluster()
        {
            var op = _inputPeg.Operators;
            // OneOrMore, Optional, ZeroOrMore, Choice, Sequence 
            // ... specified together to avoid inefficient parsing
            return _inputPeg.Rule(RuleName.LeftRecursionCluster,
                op.Sequence(op.NonTerminal(RuleName.Operator),
                            op.OrderedChoice(
                                op.Terminal(TokenNames.Optional),
                                op.Terminal(TokenNames.ZeroOrMore),
                                op.Terminal(TokenNames.OneOrMore),
                                op.Sequence(
                                    op.Terminal(TokenNames.Choice),
                                    op.NonTerminal(RuleName.Operator)),
                                op.NonTerminal(RuleName.Operator))));
        }

        private ParsingRule<TokenNames, RuleName, IParserGeneratorResult> BuildTerminal()
        {
            var op = _inputPeg.Operators;

            return _inputPeg.Rule(RuleName.Terminal,
                op.Sequence(op.Terminal(TokenNames.OpenSquare),
                            op.Terminal(TokenNames.Identifier),
                            op.Terminal(TokenNames.CloseSquare)));
        }

        private ParsingRule<TokenNames, RuleName, IParserGeneratorResult> BuildBrackets()
        {
            var op = _inputPeg.Operators;
            return _inputPeg.Rule(RuleName.Brackets,
                op.Sequence(op.Terminal(TokenNames.OpenRound),
                            op.NonTerminal(RuleName.Operator),
                            op.Terminal(TokenNames.CloseRound)));
        }

        private ParsingRule<TokenNames, RuleName, IParserGeneratorResult> BuildNotPredicate()
        {
            var op = _inputPeg.Operators;
            // Not Predicate
            return _inputPeg.Rule(RuleName.Not,
                op.Sequence(op.Terminal(TokenNames.Not),
                            op.NonTerminal(RuleName.NonTerminal),
                            op.NonTerminal(RuleName.NonTerminal)));
        }

        private ParsingRule<TokenNames, RuleName, IParserGeneratorResult> BuildNonTerminal()
        {
            var op = _inputPeg.Operators;

            return _inputPeg.Rule(RuleName.NonTerminal, op.Terminal(TokenNames.Identifier));
        }

        private ParsingRule<TokenNames, RuleName, IParserGeneratorResult> BuildEmpty()
        {
            var op = _inputPeg.Operators;
            return _inputPeg.Rule(RuleName.Empty,
                op.Sequence(op.Terminal(TokenNames.OpenSquare), op.Terminal(TokenNames.CloseSquare)));
        }

        private ParsingRule<TokenNames, RuleName, IParserGeneratorResult> BuildAndPredicate()
        {
            var op = _inputPeg.Operators;
            // And Predicate
            return _inputPeg.Rule(RuleName.And,
                op.Sequence(op.Terminal(TokenNames.And),
                            op.NonTerminal(RuleName.NonTerminal),
                            op.NonTerminal(RuleName.NonTerminal)));
        }

        private ParsingRule<TokenNames, RuleName, IParserGeneratorResult> BuildOperator()
        {
            var op = _inputPeg.Operators;

            // Operators
            return _inputPeg.Rule(RuleName.Operator,
                op.OrderedChoice(op.NonTerminal(RuleName.And),
                                 op.NonTerminal(RuleName.Empty),
                                 op.NonTerminal(RuleName.NonTerminal),
                                 op.NonTerminal(RuleName.Not),
                                 op.NonTerminal(RuleName.Brackets),
                                 op.NonTerminal(RuleName.Terminal),
                                 op.NonTerminal(RuleName.LeftRecursionCluster)));
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
