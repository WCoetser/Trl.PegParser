using System;
using System.Collections.Generic;
using Trs.PegParser.Tokenization;

namespace Trs.PegParser.Grammer.Operators
{
    /// <summary>
    /// Represents calls to other parse rules in the parser.
    /// </summary>
    /// <typeparam name="TTokenTypeName">Enum identifying token types fed into the parser.</typeparam>
    /// <typeparam name="TNoneTerminalName">Enum type identifying parser rule heads and non-terminals.</typeparam>
    /// <typeparam name="TActionResult">Result of applying semantic actions when tokens are matched.</typeparam>
    public class NonTerminal<TTokenTypeName, TNoneTerminalName, TActionResult>
        : IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNoneTerminalName : Enum
    {
        private readonly TNoneTerminalName _noneTerminalName;
        private IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult> _ruleBody;
        private readonly SemanticAction<TActionResult, TTokenTypeName> _matchAction;

        public NonTerminal(TNoneTerminalName noneTerminalName, SemanticAction<TActionResult, TTokenTypeName> matchAction)
            => (_noneTerminalName, _matchAction) = (noneTerminalName, matchAction);

        public IEnumerable<TNoneTerminalName> GetNonTerminalNames()
            => new[] { _noneTerminalName };
                
        public void SetNonTerminalParsingRuleBody(
            IDictionary<TNoneTerminalName, IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>> ruleBodies)
        {
            if (ruleBodies == null)
            {
                throw new ArgumentNullException(nameof(ruleBodies));
            }

            _ruleBody = ruleBodies[_noneTerminalName];
        }

        public bool HasNonTerminalParsingRuleBodies => _ruleBody != null;

        public ParseResult<TTokenTypeName, TActionResult> Parse(IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, int startIndex, bool mustConsumeTokens)
        {
            var parseResult = _ruleBody.Parse(inputTokens, startIndex, mustConsumeTokens);
            TActionResult semanticActionResult = default;
            if (parseResult.Succeed)
            {
                if (_matchAction != null && mustConsumeTokens)
                {
                    semanticActionResult = _matchAction(parseResult.MatchedTokens, new[] { parseResult.SemanticActionResult });
                }
                return ParseResult<TTokenTypeName, TActionResult>.Succeeded(parseResult.NextParseStartIndex, parseResult.MatchedTokens, semanticActionResult);
            }
            return ParseResult<TTokenTypeName, TActionResult>.Failed(parseResult.NextParseStartIndex);
        }
    }
}
