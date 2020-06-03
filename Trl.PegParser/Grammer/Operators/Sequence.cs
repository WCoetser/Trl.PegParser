using System;
using System.Collections.Generic;
using System.Linq;
using Trl.PegParser.Grammer.Semantics;
using Trl.PegParser.Tokenization;

namespace Trl.PegParser.Grammer.Operators
{
    public class Sequence<TTokenTypeName, TNoneTerminalName, TActionResult>
        : IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNoneTerminalName : Enum
    {
        /// <summary>
        /// Elements making up the sequence.
        /// </summary>
        private readonly IEnumerable<IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>> _sequenceDefinition;

        /// <summary>
        /// Action to take when sequence is matched.
        /// </summary>
        private readonly SemanticAction<TActionResult, TTokenTypeName> _matchAction;

        public Sequence(IEnumerable<IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>> sequenceDefinitions,
            SemanticAction<TActionResult, TTokenTypeName> matchAction) {
            if (sequenceDefinitions.Count() < 2)
            {
                throw new ArgumentException("Sequence (concatenation) must contain at least 2 elements.", nameof(sequenceDefinitions));
            }
            _sequenceDefinition = sequenceDefinitions;
            _matchAction = matchAction;
        }

        public IEnumerable<TNoneTerminalName> GetNonTerminalNames()
        {
            var noneTerminalNames = new HashSet<TNoneTerminalName>();
            foreach (var seqElement in _sequenceDefinition)
            {
                noneTerminalNames.UnionWith(seqElement.GetNonTerminalNames());
            }
            return noneTerminalNames;
        }        

        public ParseResult<TTokenTypeName, TActionResult> Parse(IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, int startIndex, bool mustConsumeTokens)
        {
            int nextParsePosition = startIndex;
            int totalMatchLength = 0;
            var subActionResults = new List<TActionResult>();
            foreach (var sequenceElement in _sequenceDefinition)
            {
                var currentResult = sequenceElement.Parse(inputTokens, nextParsePosition, mustConsumeTokens);
                // TODO: Adaptive parsing - skip failed sub results
                if (!currentResult.Succeed)
                {
                    return currentResult;
                }
                subActionResults.Add(currentResult.SemanticActionResult);
                nextParsePosition = currentResult.NextParseStartIndex;
                totalMatchLength += currentResult.MatchedTokens.MatchedIndices.Length;
            }
            TActionResult actionResult = default;
            var match = new TokensMatch<TTokenTypeName>(inputTokens, new MatchRange(startIndex, totalMatchLength));
            if (_matchAction != null && mustConsumeTokens)
            {
                actionResult = _matchAction(match, subActionResults);
            }
            return ParseResult<TTokenTypeName, TActionResult>.Succeeded(nextParsePosition, match, actionResult);            
        }

        public void SetNonTerminalParsingRuleBody(IDictionary<TNoneTerminalName, IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>> ruleBodies)
        {
            foreach (var sequenceElement in _sequenceDefinition)
            {
                sequenceElement.SetNonTerminalParsingRuleBody(ruleBodies);
            }
        }

        public bool HasNonTerminalParsingRuleBodies
            => _sequenceDefinition.Any(seqElement => seqElement.HasNonTerminalParsingRuleBodies);

        public override string ToString() => string.Join(" ", _sequenceDefinition.Select(s => s.ToString()));
    }
}
