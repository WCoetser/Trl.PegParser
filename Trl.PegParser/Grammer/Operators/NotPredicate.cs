using System;
using System.Collections.Generic;
using Trl.PegParser.DataStructures;
using Trl.PegParser.Tokenization;

namespace Trl.PegParser.Grammer.Operators
{
    public class NotPredicate<TTokenTypeName, TNonTerminalName, TActionResult>
        : IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNonTerminalName : Enum
    {
        private readonly IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> _subExpression;

        public NotPredicate(IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> subExpression)
        {
            _subExpression = subExpression;
        }

        public IEnumerable<TNonTerminalName> GetNonTerminalNames()
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

        public void SetMemoizer(Memoizer<(TNonTerminalName, int, bool), ParseResult<TTokenTypeName, TActionResult>> memoizer)
        {
            _subExpression.SetMemoizer(memoizer);
        }

        public void SetNonTerminalParsingRuleBody(IDictionary<TNonTerminalName, IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>> ruleBodies)
        {
            _subExpression.SetNonTerminalParsingRuleBody(ruleBodies);
        }

        public bool HasNonTerminalParsingRuleBodies => _subExpression.HasNonTerminalParsingRuleBodies;

        public override string ToString() => ToParserSpec.Value;

        public Lazy<string> ToParserSpec => new Lazy<string>(() => $"!({_subExpression})");
    }
}
