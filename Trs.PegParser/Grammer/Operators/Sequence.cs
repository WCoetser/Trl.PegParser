using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Trs.PegParser.Tokenization;

namespace Trs.PegParser.Grammer.Operators
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

        public ParseResult<TTokenTypeName, TActionResult> Parse([NotNull] IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, int startPosition)
        {
            int nextParsePosition = startPosition;
            int totalMatchLength = 0;
            var subActionResults = new List<TActionResult>();
            foreach (var sequenceElement in _sequenceDefinition)
            {
                var currentResult = sequenceElement.Parse(inputTokens, nextParsePosition);
                // TODO: Adaptive parsing - skip failed sub results
                if (!currentResult.Succeed)
                {
                    return currentResult;
                }
                subActionResults.Add(currentResult.SemanticActionResult);
                nextParsePosition = currentResult.NextParsePosition.Value;
                totalMatchLength += currentResult.MatchedTokens.MatchedIndices.Length;
            }
            TActionResult actionResult = default;
            var match = new TokensMatch<TTokenTypeName>(inputTokens, new MatchRange(startPosition, totalMatchLength));
            if (_matchAction != null)
            {
                actionResult = _matchAction(match, subActionResults);
            }
            return new ParseResult<TTokenTypeName, TActionResult>
            {
                Succeed = true,
                NextParsePosition = nextParsePosition,
                SemanticActionResult = actionResult,
                MatchedTokens = match
            };
        }

        void IParsingOperatorExecution<TTokenTypeName, TNoneTerminalName, TActionResult>
            .SetNonTerminalParsingRuleBody(IDictionary<TNoneTerminalName, IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>> ruleBodies)
        {
            foreach (var sequenceElement in _sequenceDefinition)
            {
                sequenceElement.SetNonTerminalParsingRuleBody(ruleBodies);
            }
        }

        bool IParsingOperatorExecution<TTokenTypeName, TNoneTerminalName, TActionResult>.HasNonTerminalParsingRuleBodies
            => _sequenceDefinition.Any(seqElement => seqElement.HasNonTerminalParsingRuleBodies);
    }
}
