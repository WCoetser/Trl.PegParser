using System;
using System.Collections.Generic;
using System.Linq;
using Trs.PegParser.Tokenization;

namespace Trs.PegParser.Grammer
{
    public class Parser<TTokenTypeName, TNonTerminalName, TSemanticActionResult>
        where TTokenTypeName: Enum
        where TNonTerminalName: Enum
    {
        private readonly TNonTerminalName _startSymbol;
        private readonly Dictionary<TNonTerminalName, IParsingOperator<TTokenTypeName, TNonTerminalName, TSemanticActionResult>> _grammerRules;

        public Parser(TNonTerminalName startSymbol, 
            IEnumerable<ParsingRule<TTokenTypeName, TNonTerminalName, TSemanticActionResult>> grammerRules) {

            ValidateGrammer(startSymbol, grammerRules);

            _startSymbol = startSymbol;
            _grammerRules = grammerRules.ToDictionary(rule => rule.RuleIdentifier, 
                rule => rule.ParsingExpression);

            AttachRuleBodiesToNonTerminals();
        }

        private void AttachRuleBodiesToNonTerminals()
        {
            foreach (var rule in _grammerRules)
            {
                rule.Value.SetNonTerminalParsingRuleBody(_grammerRules);
            }
        }

        private void ValidateGrammer(TNonTerminalName startSymbol, IEnumerable<ParsingRule<TTokenTypeName, TNonTerminalName, TSemanticActionResult>> grammerRules)
        {            
            // Grammer must be deterministic
            if (grammerRules.GroupBy(rule => rule.RuleIdentifier).Any(group => group.Count() > 1))
            {
                throw new ArgumentException("The same grammer rule non-terminal head symbol are specified more than once.");
            }

            // Start symbol must be present
            if (!grammerRules.Any(r => r.RuleIdentifier.Equals(startSymbol)))
            {
                throw new ArgumentException("Start symbol not found in grammer rule definitions.");
            }

            foreach (var rule in grammerRules)
            {
                if (rule.ParsingExpression.HasNonTerminalParsingRuleBodies)
                {
                    throw new ArgumentException($"Parsing rule for {rule.RuleIdentifier} already used in different parser.");
                }
            }

            // All non-terminals must refer to a known rule
            HashSet<TNonTerminalName> ruleBodyNonTerminals = new HashSet<TNonTerminalName>();
            foreach (var nonterminals in grammerRules.Select(rule => rule.ParsingExpression.GetNonTerminalNames()))
            {
                foreach (var nonterminal in nonterminals)
                {
                    ruleBodyNonTerminals.Add(nonterminal);
                }
            }
            HashSet<TNonTerminalName> headSet = new HashSet<TNonTerminalName>(grammerRules.Select(rule => rule.RuleIdentifier));
            if (!headSet.IsSupersetOf(ruleBodyNonTerminals))
            {
                throw new ArgumentException("Some non-terminals in rule bodies do not have rule definitions.");
            }
        }

        public ParseResult<TTokenTypeName, TSemanticActionResult> Parse(TokenizationResult<TTokenTypeName> inputTokens)
        {
            if (!inputTokens.Succeed)
            {
                return new ParseResult<TTokenTypeName, TSemanticActionResult>
                {
                    NextParsePosition = 0,
                    Succeed = false
                };
            }

            var parseResult = _grammerRules[_startSymbol].Parse(inputTokens.MatchedRanges, 0);
            // Test for extra input at end of input
            if (parseResult.Succeed && parseResult.NextParsePosition != inputTokens.MatchedRanges.Count)
            {
                return new ParseResult<TTokenTypeName, TSemanticActionResult>
                {
                    Succeed = false,
                    NextParsePosition = parseResult.NextParsePosition
                };
            }
            return parseResult;
        }
    }
}
