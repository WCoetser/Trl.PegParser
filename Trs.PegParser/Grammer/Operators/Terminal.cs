using System;
using System.Collections.Generic;
using System.Linq;
using Trs.PegParser.Tokenization;

namespace Trs.PegParser.Grammer.Operators
{
    public class Terminal<TTokenTypeName, TNoneTerminalName, TActionResult>
        : IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNoneTerminalName : Enum
    {
        private readonly TTokenTypeName _expectedToken;
        private readonly SemanticAction<TActionResult, TTokenTypeName> _matchAction;

        public Terminal(TTokenTypeName expectedToken, SemanticAction<TActionResult, TTokenTypeName> matchAction)
            => (_expectedToken, _matchAction) = (expectedToken, matchAction);

        public IEnumerable<TNoneTerminalName> GetNonTerminalNames()
            => Enumerable.Empty<TNoneTerminalName>();
        
        public ParseResult<TTokenTypeName, TActionResult> Parse(IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens,
            int startIndex)
        {
            if (startIndex >= inputTokens.Count 
                || !(inputTokens[startIndex].TokenName.Equals(_expectedToken)))
            {
                return ParseResult<TTokenTypeName, TActionResult>.Failed(startIndex);
            }
            TActionResult actionResult = default;
            var match = new TokensMatch<TTokenTypeName>(inputTokens, new MatchRange(startIndex, 1));
            if (_matchAction != null)
            {
                actionResult = _matchAction(match, Enumerable.Empty<TActionResult>());
            }
            return ParseResult<TTokenTypeName, TActionResult>.Succeeded(startIndex + 1, match, actionResult);
        }

        void IParsingOperatorExecution<TTokenTypeName, TNoneTerminalName, TActionResult>
            .SetNonTerminalParsingRuleBody(IDictionary<TNoneTerminalName, IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>> ruleBodies)
        {
            // Nothing to do here - terminals do not contain non-terminals.
        }

        /// <summary>
        /// Terminals do not contain non-terminals, therefore this is always false.
        /// </summary>
        bool IParsingOperatorExecution<TTokenTypeName, TNoneTerminalName, TActionResult>.HasNonTerminalParsingRuleBodies
            => false;
    }
}
