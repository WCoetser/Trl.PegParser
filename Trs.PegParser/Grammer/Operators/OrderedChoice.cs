using System;
using System.Collections.Generic;
using System.Linq;
using Trs.PegParser.Tokenization;

namespace Trs.PegParser.Grammer.Operators
{
    public class OrderedChoice<TTokenTypeName, TNoneTerminalName, TActionResult>
        : IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNoneTerminalName : Enum
    {
        private readonly IEnumerable<IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>> _choiceSubExpressions;
        private readonly SemanticAction<TActionResult, TTokenTypeName> _matchAction;

        public OrderedChoice(IEnumerable<IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>> choiceSubExpressions,
            SemanticAction<TActionResult, TTokenTypeName> matchAction)
        {
            if (choiceSubExpressions.Count() < 2)
            {
                throw new ArgumentException("Must have at least 2 sub-expressions for Ordered Choice.");
            }

            _choiceSubExpressions = choiceSubExpressions;
            _matchAction = matchAction;
        }

        public IEnumerable<TNoneTerminalName> GetNonTerminalNames()
        => _choiceSubExpressions.SelectMany(cse => cse.GetNonTerminalNames());

        public ParseResult<TTokenTypeName, TActionResult> Parse(IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, int startIndex, bool mustConsumeTokens)
        {
            ParseResult<TTokenTypeName, TActionResult> lastResult = null;
            foreach (var subExpression in _choiceSubExpressions)
            {
                lastResult = subExpression.Parse(inputTokens, startIndex, mustConsumeTokens);
                if (lastResult.Succeed)
                {
                    break;
                }
            }
            // lastResult cannot be null at this point - there will always be at least 2 choices to try
            if (!lastResult.Succeed)
            {
                return ParseResult<TTokenTypeName, TActionResult>.Failed(startIndex);
            }
            TActionResult actionResult = default;
            if (_matchAction != null && mustConsumeTokens)
            {
                actionResult = _matchAction(lastResult.MatchedTokens, new[] { lastResult.SemanticActionResult });
            }
            return ParseResult<TTokenTypeName, TActionResult>.Succeeded(lastResult.NextParseStartIndex, lastResult.MatchedTokens, actionResult);
        }

        public void SetNonTerminalParsingRuleBody(IDictionary<TNoneTerminalName, IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>> ruleBodies)
        {
            foreach (var subExpression in _choiceSubExpressions)
            {
                subExpression.SetNonTerminalParsingRuleBody(ruleBodies);
            }
        }

        public bool HasNonTerminalParsingRuleBodies
            => _choiceSubExpressions.Any(e => e.HasNonTerminalParsingRuleBodies);
    }
}
