using System;
using System.Collections.Generic;
using System.Linq;
using Trl.PegParser.DataStructures;
using Trl.PegParser.Grammer.Operators;
using Trl.PegParser.Tokenization;

namespace Trl.PegParser.Grammer
{
    public class Parser<TTokenTypeName, TNonTerminalName, TSemanticActionResult>
        where TTokenTypeName: Enum
        where TNonTerminalName: Enum
    {
        private readonly Dictionary<TNonTerminalName, IParsingOperator<TTokenTypeName, TNonTerminalName, TSemanticActionResult>> _grammerRules;
        private readonly NonTerminal<TTokenTypeName, TNonTerminalName, TSemanticActionResult> _startSymbol;
        private readonly Memoizer<(TNonTerminalName, int, bool), ParseResult<TTokenTypeName, TSemanticActionResult>> _memoizer;

        public Parser(NonTerminal<TTokenTypeName, TNonTerminalName, TSemanticActionResult> startSymbol, 
            IEnumerable<ParsingRule<TTokenTypeName, TNonTerminalName, TSemanticActionResult>> grammerRules) {

            if (grammerRules == null)
            {
                throw new ArgumentNullException(nameof(grammerRules));
            }

            _startSymbol = startSymbol;
            
            ValidateGrammer(_startSymbol.GetNonTerminalNames().Single(), grammerRules);

            _grammerRules = grammerRules.ToDictionary(rule => rule.RuleIdentifier, 
                rule => rule.ParsingExpression);

            var equalityComparer = EqualityComparer<ValueTuple<TNonTerminalName, int, bool>>.Default;
            _memoizer = new Memoizer<(TNonTerminalName, int, bool), ParseResult<TTokenTypeName, TSemanticActionResult>>(equalityComparer);
            InitializeMemoizer();

            AttachRuleBodiesToNonTerminals();
        }

        private void InitializeMemoizer()
        {
            _startSymbol.SetMemoizer(_memoizer);
            foreach (var rule in _grammerRules)
            {
                rule.Value.SetMemoizer(_memoizer);
            }
        }

        private void AttachRuleBodiesToNonTerminals()
        {
            foreach (var rule in _grammerRules)
            {
                rule.Value.SetNonTerminalParsingRuleBody(_grammerRules);
            }
            _startSymbol.SetNonTerminalParsingRuleBody(_grammerRules);
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

        public ParseResult<TTokenTypeName, TSemanticActionResult> Parse(IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens)
        {
            _ = inputTokens ?? throw new ArgumentNullException(nameof(inputTokens));

            _memoizer.ClearAll();

            var parseResult = _startSymbol.Parse(inputTokens, 0, true);

            // Test for extra input at end of input
            if (parseResult.Succeed && parseResult.NextParseStartIndex != inputTokens.Count)
            {
                return ParseResult<TTokenTypeName, TSemanticActionResult>.Failed(parseResult.NextParseStartIndex);
            }

            return parseResult;
        }
    }
}
