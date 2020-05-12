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
            int startIndex, bool mustConsumeTokens)
        {
            if (inputTokens == null)
            {
                throw new ArgumentNullException(nameof(inputTokens));
            }

            if (startIndex >= inputTokens.Count 
                || !(inputTokens[startIndex].TokenName.Equals(_expectedToken)))
            {
                return ParseResult<TTokenTypeName, TActionResult>.Failed(startIndex);
            }
            TActionResult actionResult = default;
            var match = new TokensMatch<TTokenTypeName>(inputTokens, new MatchRange(startIndex, 1));
            if (_matchAction != null && mustConsumeTokens)
            {
                actionResult = _matchAction(match, Enumerable.Empty<TActionResult>());
            }
            return ParseResult<TTokenTypeName, TActionResult>.Succeeded(startIndex + 1, match, actionResult);
        }

        public void SetNonTerminalParsingRuleBody(IDictionary<TNoneTerminalName, IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>> ruleBodies)
        {
            // Nothing to do here - terminals do not contain non-terminals.
        }

        /// <summary>
        /// Terminals do not contain non-terminals, therefore this is always false.
        /// </summary>
        public bool HasNonTerminalParsingRuleBodies => false;

        public override string ToString() => $"\"{_expectedToken}\"";
    }
}
