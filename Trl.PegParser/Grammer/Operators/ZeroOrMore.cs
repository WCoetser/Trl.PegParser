using System;
using System.Collections.Generic;
using Trl.PegParser.DataStructures;
using Trl.PegParser.Grammer.Semantics;
using Trl.PegParser.Tokenization;

namespace Trl.PegParser.Grammer.Operators
{
    public class ZeroOrMore<TTokenTypeName, TNonTerminalName, TActionResult>
        : IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNonTerminalName : Enum
    {
        private readonly IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> _subExpression;
        private readonly SemanticAction<TActionResult, TTokenTypeName> _matchAction;

        public ZeroOrMore(IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> subExpression,
            SemanticAction<TActionResult, TTokenTypeName> matchAction)
        => (_subExpression, _matchAction) = (subExpression, matchAction);

        public IEnumerable<TNonTerminalName> GetNonTerminalNames()
        => _subExpression.GetNonTerminalNames();

        public ParseResult<TTokenTypeName, TActionResult> Parse(IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, int startIndex, bool mustConsumeTokens)
        {
            ParseResult<TTokenTypeName, TActionResult> lastResult;
            int nextParseIndex = startIndex;
            int totalMatchLength = 0;
            // TODO: Move this to a "yield return" method to stream parse results instead of storing them in memory
            List<TActionResult> subResults = new List<TActionResult>();
            do
            {
                int previousNextParseIndex = nextParseIndex;
                lastResult = _subExpression.Parse(inputTokens, nextParseIndex, mustConsumeTokens);
                if (lastResult.Succeed)
                {
                    totalMatchLength += lastResult.MatchedTokens.MatchedIndices.Length;
                    nextParseIndex = lastResult.NextParseStartIndex;
                    subResults.Add(lastResult.SemanticActionResult);
                }
                // Prevent non-termination in case empty string is matched
                if (previousNextParseIndex == nextParseIndex)
                {
                    break;
                }
            }
            while (lastResult.Succeed && nextParseIndex < inputTokens.Count);
            // This will always succeed, because it also accepts the empty case
            var matchedTokens = new TokensMatch<TTokenTypeName>(inputTokens, new MatchRange(startIndex, totalMatchLength));
            TActionResult semanticResult = default;
            if (_matchAction != null && mustConsumeTokens)
            {
                semanticResult = _matchAction(matchedTokens, subResults.AsReadOnly(), ToParserSpec.Value);
            }
            return ParseResult<TTokenTypeName, TActionResult>.Succeeded(nextParseIndex, matchedTokens, semanticResult);
        }

        public void SetMemoizer(Memoizer<(TNonTerminalName, int, bool), ParseResult<TTokenTypeName, TActionResult>> memoizer)
        {
            _subExpression.SetMemoizer(memoizer);
        }

        public void SetNonTerminalParsingRuleBody(IDictionary<TNonTerminalName, IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>> ruleBodies)
        => _subExpression.SetNonTerminalParsingRuleBody(ruleBodies);

        public bool HasNonTerminalParsingRuleBodies
            => _subExpression.HasNonTerminalParsingRuleBodies;

        public override string ToString() => ToParserSpec.Value;

        public Lazy<string> ToParserSpec => new Lazy<string>(() => $"({_subExpression})*");
    }
}
