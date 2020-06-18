using System;
using System.Collections.Generic;
using System.Linq;
using Trl.PegParser.DataStructures;
using Trl.PegParser.Grammer.Semantics;
using Trl.PegParser.Tokenization;

namespace Trl.PegParser.Grammer.Operators
{
    public class OrderedChoice<TTokenTypeName, TNonTerminalName, TActionResult>
        : IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNonTerminalName : Enum
    {
        private readonly IEnumerable<IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>> _choiceSubExpressions;
        private readonly SemanticAction<TActionResult, TTokenTypeName> _matchAction;

        public OrderedChoice(IEnumerable<IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>> choiceSubExpressions,
            SemanticAction<TActionResult, TTokenTypeName> matchAction)
        {
            if (choiceSubExpressions.Count() < 2)
            {
                throw new ArgumentException("Must have at least 2 sub-expressions for Ordered Choice.");
            }

            _choiceSubExpressions = choiceSubExpressions;
            _matchAction = matchAction;
        }

        public IEnumerable<TNonTerminalName> GetNonTerminalNames()
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

        public void SetMemoizer(Memoizer<(TNonTerminalName, int, bool), ParseResult<TTokenTypeName, TActionResult>> memoizer)
        {
            foreach (var subExpression in _choiceSubExpressions)
            {
                subExpression.SetMemoizer(memoizer);
            }
        }

        public void SetNonTerminalParsingRuleBody(IDictionary<TNonTerminalName, IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>> ruleBodies)
        {
            foreach (var subExpression in _choiceSubExpressions)
            {
                subExpression.SetNonTerminalParsingRuleBody(ruleBodies);
            }
        }

        public bool HasNonTerminalParsingRuleBodies
            => _choiceSubExpressions.Any(e => e.HasNonTerminalParsingRuleBodies);

        public override string ToString() => $"({ string.Join(" | ", _choiceSubExpressions.Select(s => s.ToString())) })";
    }
}
