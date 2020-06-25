using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Trl.PegParser.Grammer.Semantics;
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
            var actions = _inputPeg.DefaultSemanticActions;

            actions.SetDefaultGenericPassthroughAction<GenericAstResult>();

            actions.OrderedChoiceAction = (_, subResults, matchedPeg) => subResults.First();

            actions.SetNonTerminalAction(RuleName.Operator, (_, subResults, matchedPeg) => subResults.First());

            _inputPeg.DefaultSemanticActions.SetNonTerminalAction(RuleName.Start, (_, subResults, matchedPeg) =>
            {
                var oneOrMore = ((GenericAstResult)subResults.First()).SubResults.Cast<RuleAstResult<TTokenTypeName, TNonTerminalName, TActionResult>>();
                var ruleCollectionResult = new RuleCollectionResult<TTokenTypeName, TNonTerminalName, TActionResult>();
                ruleCollectionResult.Rules = oneOrMore.Select(ruleAstResult => _outputPeg.Rule(ruleAstResult.RuleName, ruleAstResult.Operator));
                return ruleCollectionResult;
            });

            _inputPeg.DefaultSemanticActions.SetNonTerminalAction(RuleName.Rule, (_, subResults, matchedPeg) =>
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
                (_, subresults, matchedPeg) =>
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
                (_, subresults, matchedPeg) =>
                {
                    return new OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult>
                    {
                        Operator = _outputPeg.Operators.EmptyString()
                    };
                });

            _inputPeg.DefaultSemanticActions.SetNonTerminalAction(RuleName.Brackets, (_, subResults, matchedPeg) =>
            {
                return ((GenericAstResult)(subResults.First())).SubResults[1] as OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult>;
            });

            _inputPeg.DefaultSemanticActions.SetNonTerminalAction(RuleName.And, (_, subResults, matchedPeg) =>
            {
                var op = ((GenericAstResult)subResults.First()).SubResults[2] as OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult>;
                return new OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult>
                {
                    Operator = _outputPeg.Operators.AndPredicate(op.Operator)
                };
            });

            _inputPeg.DefaultSemanticActions.SetNonTerminalAction(RuleName.Not, (_, subResults, matchedPeg) =>
            {
                var op = ((GenericAstResult)subResults.First()).SubResults[2] as OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult>;
                return new OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult>
                {
                    Operator = _outputPeg.Operators.NotPredicate(op.Operator)
                };
            });

            _inputPeg.DefaultSemanticActions.SetNonTerminalAction(RuleName.OneOrMore, (_, subResults, matchedPeg) =>
            {
                var firstSubExpression = (GenericAstResult)subResults.First();
                var subExpression = firstSubExpression.SubResults[0] as OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult>;
                return new OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult>
                {
                    Operator = _outputPeg.Operators.OneOrMore(subExpression.Operator)
                };
            });

            _inputPeg.DefaultSemanticActions.SetNonTerminalAction(RuleName.ZeroOrMore, (_, subResults, matchedPeg) =>
            {
                var firstSubExpression = (GenericAstResult)subResults.First();
                var subExpression = firstSubExpression.SubResults[0] as OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult>;
                return new OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult>
                {
                    Operator = _outputPeg.Operators.ZeroOrMore(subExpression.Operator)
                };
            });

            _inputPeg.DefaultSemanticActions.SetNonTerminalAction(RuleName.Optional, (_, subResults, matchedPeg) =>
            {
                var firstSubExpression = (GenericAstResult)subResults.First();
                var subExpression = firstSubExpression.SubResults[0] as OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult>;
                return new OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult>
                {
                    Operator = _outputPeg.Operators.Optional(subExpression.Operator)
                };
            });

            _inputPeg.DefaultSemanticActions.SetNonTerminalAction(RuleName.Terminal,
            (_, subresults, matchedPeg) =>
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

            _inputPeg.DefaultSemanticActions.SetNonTerminalAction(RuleName.Sequence,
                (_, subResults, matchedPeg) =>
                {
                    var subSubResults = ((GenericAstResult)subResults.First()).SubResults;
                    var head = subSubResults[0] as OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult>;
                    var tail = subSubResults[1] as OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult>;
                    return new OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult>
                    {
                        Operator = _outputPeg.Operators.Sequence(head.Operator, tail.Operator)
                    };
                });

            _inputPeg.DefaultSemanticActions.SetNonTerminalAction(RuleName.Choice,
                (_, subResults, matchedPeg) =>
                {
                    var subSubResults = ((GenericAstResult)subResults.First()).SubResults;
                    var head = subSubResults[0] as OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult>;
                    var tail = subSubResults[2] as OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult>;
                    return new OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult>
                    {
                        Operator = _outputPeg.Operators.OrderedChoice(head.Operator, tail.Operator)
                    };
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
                op.OneOrMore(op.NonTerminal(RuleName.Rule)));

            var rule = _inputPeg.Rule(RuleName.Rule,
               op.Sequence(op.Terminal(TokenNames.Identifier),
                           op.Terminal(TokenNames.Arrow),
                           op.NonTerminal(RuleName.Operator),
                           op.Optional(op.Terminal(TokenNames.SemiColon))));

            var @operator = _inputPeg.Rule(RuleName.Operator,
               // NB: Order matters here
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
