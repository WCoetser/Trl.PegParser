using System;
using System.Collections.Generic;
using System.Linq;
using Trl.PegParser.DataStructures;
using Trl.PegParser.Grammer.Semantics;
using Trl.PegParser.Tokenization;

namespace Trl.PegParser.Grammer.Operators
{
    public class Optional<TTokenTypeName, TNonTerminalName, TActionResult>
        : IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNonTerminalName : Enum
    {
        private readonly IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> _subExpression;
        private readonly SemanticAction<TActionResult, TTokenTypeName> _matchAction;

        public Optional(IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> subExpression,
            SemanticAction<TActionResult, TTokenTypeName> matchAction)
        => (_subExpression, _matchAction) = (subExpression, matchAction);


        public IEnumerable<TNonTerminalName> GetNonTerminalNames()
        => _subExpression.GetNonTerminalNames();
        
        public ParseResult<TTokenTypeName, TActionResult> Parse(IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, int startIndex, bool mustConsumeTokens)
        {
            TActionResult semanticResult = default;

            // Optional: sub expression matches
            var result = _subExpression.Parse(inputTokens, startIndex, mustConsumeTokens);
            if (result.Succeed)
            {
                if (_matchAction != null && mustConsumeTokens)
                {
                    semanticResult = _matchAction(result.MatchedTokens, new[] { result.SemanticActionResult }, ToParserSpec.Value);
                }
                return ParseResult<TTokenTypeName, TActionResult>.Succeeded(result.NextParseStartIndex, result.MatchedTokens, semanticResult);
            };

            // Optional: empty match
            if (startIndex > inputTokens.Count)
            {
                return ParseResult<TTokenTypeName, TActionResult>.Failed(startIndex);
            }
            var matchedTokens = new TokensMatch<TTokenTypeName>(inputTokens, new MatchRange(startIndex, 0));
            if (_matchAction != null && mustConsumeTokens)
            {
                semanticResult = _matchAction(matchedTokens, Enumerable.Empty<TActionResult>(), ToParserSpec.Value);
            }
            return ParseResult<TTokenTypeName, TActionResult>.Succeeded(startIndex, matchedTokens, semanticResult);
        }

        public void SetMemoizer(Memoizer<(TNonTerminalName, int, bool), ParseResult<TTokenTypeName, TActionResult>> memoizer)
        {
            _subExpression.SetMemoizer(memoizer);
        }

        public void SetNonTerminalParsingRuleBody(IDictionary<TNonTerminalName, 
                IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>> ruleBodies)
        {
            _subExpression.SetNonTerminalParsingRuleBody(ruleBodies);
        }

        public bool HasNonTerminalParsingRuleBodies
            => _subExpression.HasNonTerminalParsingRuleBodies;

        public override string ToString() => ToParserSpec.Value;

        public Lazy<string> ToParserSpec => new Lazy<string>(() => $"({_subExpression})?");
    }
}
