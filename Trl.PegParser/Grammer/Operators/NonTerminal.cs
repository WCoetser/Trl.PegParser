using System;
using System.Collections.Generic;
using Trl.PegParser.Grammer.Semantics;
using Trl.PegParser.Tokenization;

namespace Trl.PegParser.Grammer.Operators
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

        /// <summary>
        /// Use this collection to prevent non-termination due to left recursion.
        /// </summary>
        private readonly HashSet<int> _previousStartIndices;

        public NonTerminal(TNoneTerminalName noneTerminalName, SemanticAction<TActionResult, TTokenTypeName> matchAction)
            => (_noneTerminalName, _matchAction, _previousStartIndices) = (noneTerminalName, matchAction, new HashSet<int>());

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
            // Prevent this non-terminal from calling itself on the same start index
            bool hasLeftRecursion = _previousStartIndices.Contains(startIndex);
            if (hasLeftRecursion)
            {
                return ParseResult<TTokenTypeName, TActionResult>.Failed(startIndex);
            }
            _previousStartIndices.Add(startIndex);

            // Actual parsing starts here
            var parseResult = _ruleBody.Parse(inputTokens, startIndex, mustConsumeTokens);
            TActionResult semanticActionResult = default;
            ParseResult<TTokenTypeName, TActionResult> returnResult = null;
            if (!parseResult.Succeed)
            {
                returnResult = ParseResult<TTokenTypeName, TActionResult>.Failed(startIndex);
            }
            else
            {
                if (_matchAction != null && mustConsumeTokens)
                {
                    semanticActionResult = _matchAction(parseResult.MatchedTokens, new[] { parseResult.SemanticActionResult });
                }
                returnResult = ParseResult<TTokenTypeName, TActionResult>.Succeeded(parseResult.NextParseStartIndex, parseResult.MatchedTokens, semanticActionResult);
            }

            // Remove start index to preserve memory ... it should not be needed beyond this point
            _previousStartIndices.Remove(startIndex);

            return returnResult;
        }

        public override string ToString() => _noneTerminalName.ToString();
    }
}
