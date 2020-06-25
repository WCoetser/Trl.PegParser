using System;
using System.Collections.Generic;
using System.Linq;
using Trl.PegParser.DataStructures;
using Trl.PegParser.Grammer.Semantics;
using Trl.PegParser.Tokenization;

namespace Trl.PegParser.Grammer.Operators
{
    public class EmptyString<TTokenTypeName, TNonTerminalName, TActionResult>
        : IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNonTerminalName : Enum
    {
        private readonly SemanticAction<TActionResult, TTokenTypeName> _matchAction;

        public EmptyString(SemanticAction<TActionResult, TTokenTypeName> matchAction)
            => _matchAction = matchAction;

        public IEnumerable<TNonTerminalName> GetNonTerminalNames()
        => Enumerable.Empty<TNonTerminalName>();

        public ParseResult<TTokenTypeName, TActionResult> Parse(IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, int startIndex, bool mustConsumeTokens)
        {
            if (inputTokens == null) {
                throw new ArgumentNullException(nameof(inputTokens));
            }

            // Note: It is possible to match the empty string at the end of the input tokens,
            // therefore this is > instead of >=. An example of this happening is when the 
            // input tokens length = 0.
            if (startIndex > inputTokens.Count)
            {
                return ParseResult<TTokenTypeName, TActionResult>.Failed(startIndex);
            }

            TActionResult actionResult = default;
            var match = new TokensMatch<TTokenTypeName>(inputTokens, new MatchRange(startIndex, 0));
            if (_matchAction != null && mustConsumeTokens)
            {
                // Terminals cannot have sub-results, therefore pass null
                actionResult = _matchAction(match, Enumerable.Empty<TActionResult>(), ToParserSpec.Value);
            }
            return ParseResult<TTokenTypeName, TActionResult>.Succeeded(startIndex, match, actionResult);
        }

        public void SetMemoizer(Memoizer<(TNonTerminalName, int, bool), ParseResult<TTokenTypeName, TActionResult>> memoizer)
        {
        }

        public void SetNonTerminalParsingRuleBody(IDictionary<TNonTerminalName, IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>> ruleBodies)
        {
            // Nothing to do here - empty string has no non-terminals
        }

        public bool HasNonTerminalParsingRuleBodies => false;

        public override string ToString() => ToParserSpec.Value;

        public Lazy<string> ToParserSpec => new Lazy<string>(() => "[]");

    }
}
