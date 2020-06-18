using System;
using System.Collections.Generic;
using System.Linq;
using Trl.PegParser.DataStructures;
using Trl.PegParser.Grammer.Semantics;
using Trl.PegParser.Tokenization;

namespace Trl.PegParser.Grammer.Operators
{
    public class Terminal<TTokenTypeName, TNonTerminalName, TActionResult>
        : IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNonTerminalName : Enum
    {
        private readonly TTokenTypeName _expectedToken;
        private readonly SemanticAction<TActionResult, TTokenTypeName> _matchAction;

        public Terminal(TTokenTypeName expectedToken, SemanticAction<TActionResult, TTokenTypeName> matchAction)
            => (_expectedToken, _matchAction) = (expectedToken, matchAction);

        public IEnumerable<TNonTerminalName> GetNonTerminalNames()
            => Enumerable.Empty<TNonTerminalName>();
        
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

        public void SetMemoizer(Memoizer<(TNonTerminalName, int, bool), ParseResult<TTokenTypeName, TActionResult>> memoizer)
        {
        }

        public void SetNonTerminalParsingRuleBody(IDictionary<TNonTerminalName, IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>> ruleBodies)
        {
            // Nothing to do here - terminals do not contain non-terminals.
        }

        /// <summary>
        /// Terminals do not contain non-terminals, therefore this is always false.
        /// </summary>
        public bool HasNonTerminalParsingRuleBodies => false;

        public override string ToString() => $"[{_expectedToken}]";
    }
}
