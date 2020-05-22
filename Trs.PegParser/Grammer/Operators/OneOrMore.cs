using System;
using System.Collections.Generic;
using Trs.PegParser.Grammer.Semantics;
using Trs.PegParser.Tokenization;

namespace Trs.PegParser.Grammer.Operators
{
    public class OneOrMore<TTokenTypeName, TNoneTerminalName, TActionResult>
        : IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNoneTerminalName : Enum
    {
        private readonly IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult> _subExpression;
        private readonly SemanticAction<TActionResult, TTokenTypeName> _matchAction;

        public OneOrMore(IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult> subExpression,
            SemanticAction<TActionResult, TTokenTypeName> matchAction)
        => (_subExpression, _matchAction) = (subExpression, matchAction);

        public IEnumerable<TNoneTerminalName> GetNonTerminalNames()
        => _subExpression.GetNonTerminalNames();

        public ParseResult<TTokenTypeName, TActionResult> Parse(IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, int startIndex, bool mustConsumeTokens)
        {
            _ = inputTokens ?? throw new ArgumentNullException(nameof(inputTokens));

            ParseResult<TTokenTypeName, TActionResult> lastResult;
            int nextParseIndex = startIndex;
            int totalMatchLength = 0;
            // TODO: Move this to a "yield return" method to stream parse results instead of storing them in memory
            List<TActionResult> subResults = new List<TActionResult>();
            bool hasMatch = false;
            do
            {
                int previousNextParseIndex = nextParseIndex;
                lastResult = _subExpression.Parse(inputTokens, nextParseIndex, mustConsumeTokens);
                if (lastResult.Succeed)
                {
                    totalMatchLength += lastResult.MatchedTokens.MatchedIndices.Length;                    
                    nextParseIndex = lastResult.NextParseStartIndex;
                    subResults.Add(lastResult.SemanticActionResult);
                    hasMatch = true;
                }
                // Prevent non-termination in case empty string is matched
                if (previousNextParseIndex == nextParseIndex)
                {
                    break;
                }
            }
            while (lastResult.Succeed && nextParseIndex < inputTokens.Count);
            if (!hasMatch)
            {
                return ParseResult<TTokenTypeName, TActionResult>.Failed(startIndex);
            }
            var matchedTokens = new TokensMatch<TTokenTypeName>(inputTokens, new MatchRange(startIndex, totalMatchLength));
            TActionResult semanticResult = default;
            if (_matchAction != null && mustConsumeTokens)
            {
                semanticResult = _matchAction(matchedTokens, subResults.AsReadOnly());
            }
            return ParseResult<TTokenTypeName, TActionResult>.Succeeded(nextParseIndex, matchedTokens, semanticResult);
        }

        public void SetNonTerminalParsingRuleBody(IDictionary<TNoneTerminalName, IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>> ruleBodies)
        {
            _subExpression.SetNonTerminalParsingRuleBody(ruleBodies);
        }

        public bool HasNonTerminalParsingRuleBodies
            => _subExpression.HasNonTerminalParsingRuleBodies;

        public override string ToString() => $"OneOrMore({_subExpression})";
    }
}
