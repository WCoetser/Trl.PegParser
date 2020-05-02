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
            int startPosition)
        {
            if (startPosition >= inputTokens.Count 
                || !(inputTokens[startPosition].TokenName.Equals(_expectedToken)))
            {
                return ParseResult<TTokenTypeName, TActionResult>.Failed(startPosition);
            }
            TActionResult actionResult = default;
            var match = new TokensMatch<TTokenTypeName>(inputTokens, new MatchRange(startPosition, 1));
            if (_matchAction != null)
            {
                // Terminals cannot have sub-results, therefore pass null
                actionResult = _matchAction(match, null);
            }
            return ParseResult<TTokenTypeName, TActionResult>.Succeeded(startPosition + 1, match, actionResult);
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
