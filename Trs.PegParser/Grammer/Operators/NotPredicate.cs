using System;
using System.Collections.Generic;
using Trs.PegParser.Tokenization;

namespace Trs.PegParser.Grammer.Operators
{
    public class NotPredicate<TTokenTypeName, TNoneTerminalName, TActionResult>
        : IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNoneTerminalName : Enum
    {
        private readonly IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult> _subExpression;

        public NotPredicate(IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult> subExpression)
        {
            _subExpression = subExpression;
        }

        public IEnumerable<TNoneTerminalName> GetNonTerminalNames()
        => _subExpression.GetNonTerminalNames();

        public ParseResult<TTokenTypeName, TActionResult> Parse(IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, int startIndex, bool mustConsumeTokens)
        {
            var parseResult = _subExpression.Parse(inputTokens, startIndex, false);
            if (parseResult.Succeed)
            {
                return ParseResult<TTokenTypeName, TActionResult>.Failed(startIndex);
            }
            // Note: Predicates do not trigger semantic actions or consume tokens
            return ParseResult<TTokenTypeName, TActionResult>.Succeeded(startIndex, 
                new TokensMatch<TTokenTypeName>(inputTokens, new MatchRange(startIndex, 0)), default);
        }

        public void SetNonTerminalParsingRuleBody(IDictionary<TNoneTerminalName, IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>> ruleBodies)
        {
            _subExpression.SetNonTerminalParsingRuleBody(ruleBodies);
        }

        public bool HasNonTerminalParsingRuleBodies => _subExpression.HasNonTerminalParsingRuleBodies;
    }
}
