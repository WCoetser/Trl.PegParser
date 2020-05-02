using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Trs.PegParser.Tokenization;

namespace Trs.PegParser.Grammer.Operators
{
    public class Optional<TTokenTypeName, TNoneTerminalName, TActionResult>
        : IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNoneTerminalName : Enum
    {
        private readonly IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult> _subExpression;
        private readonly SemanticAction<TActionResult, TTokenTypeName> _matchAction;

        public Optional(IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult> subExpression,
            SemanticAction<TActionResult, TTokenTypeName> matchAction)
        => (_subExpression, _matchAction) = (subExpression, matchAction);


        public IEnumerable<TNoneTerminalName> GetNonTerminalNames()
        => _subExpression.GetNonTerminalNames();
        
        public ParseResult<TTokenTypeName, TActionResult> Parse([NotNull] IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, int startPosition)
        {
            // Optional: sub expression matches
            var result = _subExpression.Parse(inputTokens, startPosition);
            if (result.Succeed)
            {
                return ParseResult<TTokenTypeName, TActionResult>.Succeeded(result.NextParsePosition, result.MatchedTokens, 
                    _matchAction(result.MatchedTokens, new[] { result.SemanticActionResult }));
            };
            // Optional: empty match
            if (startPosition > inputTokens.Count)
            {
                return ParseResult<TTokenTypeName, TActionResult>.Failed(startPosition);
            }
            var matchedTokens = new TokensMatch<TTokenTypeName>(inputTokens, new MatchRange(startPosition, 0));
            return ParseResult<TTokenTypeName, TActionResult>.Succeeded(startPosition, matchedTokens, _matchAction(matchedTokens, null));
        }

        void IParsingOperatorExecution<TTokenTypeName, TNoneTerminalName, TActionResult>
            .SetNonTerminalParsingRuleBody(IDictionary<TNoneTerminalName, 
                IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>> ruleBodies)
        {
            _subExpression.SetNonTerminalParsingRuleBody(ruleBodies);
        }

        bool IParsingOperatorExecution<TTokenTypeName, TNoneTerminalName, TActionResult>.HasNonTerminalParsingRuleBodies
            => _subExpression.HasNonTerminalParsingRuleBodies;
    }
}
