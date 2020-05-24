﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trs.PegParser.Grammer.Semantics;
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
        
        public ParseResult<TTokenTypeName, TActionResult> Parse(IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, int startIndex, bool mustConsumeTokens)
        {
            TActionResult semanticResult = default;

            // Optional: sub expression matches
            var result = _subExpression.Parse(inputTokens, startIndex, mustConsumeTokens);
            if (result.Succeed)
            {
                if (_matchAction != null && mustConsumeTokens)
                {
                    semanticResult = _matchAction(result.MatchedTokens, new[] { result.SemanticActionResult });
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
                semanticResult = _matchAction(matchedTokens, Enumerable.Empty<TActionResult>());
            }
            return ParseResult<TTokenTypeName, TActionResult>.Succeeded(startIndex, matchedTokens, semanticResult);
        }

        public void SetNonTerminalParsingRuleBody(IDictionary<TNoneTerminalName, 
                IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>> ruleBodies)
        {
            _subExpression.SetNonTerminalParsingRuleBody(ruleBodies);
        }

        public bool HasNonTerminalParsingRuleBodies
            => _subExpression.HasNonTerminalParsingRuleBodies;

        public override string ToString() => $"({_subExpression})?";
    }
}
