using System;
using System.Collections.Generic;
using System.Linq;
using Trl.PegParser.DataStructures;
using Trl.PegParser.Grammer.Semantics;
using Trl.PegParser.Tokenization;

namespace Trl.PegParser.Grammer.Operators
{
    public class Sequence<TTokenTypeName, TNonTerminalName, TActionResult>
        : IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNonTerminalName : Enum
    {
        /// <summary>
        /// Elements making up the sequence.
        /// </summary>
        private readonly IEnumerable<IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>> _sequenceDefinition;

        /// <summary>
        /// Action to take when sequence is matched.
        /// </summary>
        private readonly SemanticAction<TActionResult, TTokenTypeName> _matchAction;

        public Sequence(IEnumerable<IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>> sequenceDefinitions,
            SemanticAction<TActionResult, TTokenTypeName> matchAction) {
            if (sequenceDefinitions.Count() < 2)
            {
                throw new ArgumentException("Sequence (concatenation) must contain at least 2 elements.", nameof(sequenceDefinitions));
            }
            _sequenceDefinition = sequenceDefinitions;
            _matchAction = matchAction;
        }

        public IEnumerable<TNonTerminalName> GetNonTerminalNames()
        {
            var noneTerminalNames = new HashSet<TNonTerminalName>();
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
                actionResult = _matchAction(match, subActionResults, ToParserSpec.Value);
            }
            return ParseResult<TTokenTypeName, TActionResult>.Succeeded(nextParsePosition, match, actionResult);            
        }

        public void SetMemoizer(Memoizer<(TNonTerminalName, int, bool), ParseResult<TTokenTypeName, TActionResult>> memoizer)
        {
            foreach (var subExpression in _sequenceDefinition)
            {
                subExpression.SetMemoizer(memoizer);
            }
        }

        public void SetNonTerminalParsingRuleBody(IDictionary<TNonTerminalName, IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>> ruleBodies)
        {
            foreach (var sequenceElement in _sequenceDefinition)
            {
                sequenceElement.SetNonTerminalParsingRuleBody(ruleBodies);
            }
        }

        public bool HasNonTerminalParsingRuleBodies
            => _sequenceDefinition.Any(seqElement => seqElement.HasNonTerminalParsingRuleBodies);

        public override string ToString() => ToParserSpec.Value;

        public Lazy<string> ToParserSpec => new Lazy<string>(
            () => string.Join(" ", _sequenceDefinition.Select(s => s.ToString())));
    }
}
