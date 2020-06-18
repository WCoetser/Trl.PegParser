using System;
using System.Collections.Generic;
using Trl.PegParser.DataStructures;
using Trl.PegParser.Grammer.Semantics;
using Trl.PegParser.Tokenization;

namespace Trl.PegParser.Grammer.Operators
{
    /// <summary>
    /// Represents calls to other parse rules in the parser.
    /// </summary>
    /// <typeparam name="TTokenTypeName">Enum identifying token types fed into the parser.</typeparam>
    /// <typeparam name="TNonTerminalName">Enum type identifying parser rule heads and non-terminals.</typeparam>
    /// <typeparam name="TActionResult">Result of applying semantic actions when tokens are matched.</typeparam>
    public class NonTerminal<TTokenTypeName, TNonTerminalName, TActionResult>
        : IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNonTerminalName : Enum
    {
        private readonly TNonTerminalName _noneTerminalName;
        private IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> _ruleBody;
        private readonly SemanticAction<TActionResult, TTokenTypeName> _matchAction;
        private Memoizer<(TNonTerminalName, int, bool), ParseResult<TTokenTypeName, TActionResult>> _memoizer;

        /// <summary>
        /// Use this collection to prevent non-termination due to left recursion.
        /// </summary>
        private readonly HashSet<int> _previousStartIndices;

        public NonTerminal(TNonTerminalName noneTerminalName, SemanticAction<TActionResult, TTokenTypeName> matchAction)
            => (_noneTerminalName, _matchAction, _previousStartIndices) = (noneTerminalName, matchAction, new HashSet<int>());

        public IEnumerable<TNonTerminalName> GetNonTerminalNames()
            => new[] { _noneTerminalName };
                
        public void SetNonTerminalParsingRuleBody(
            IDictionary<TNonTerminalName, IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>> ruleBodies)
        {
            if (ruleBodies == null)
            {
                throw new ArgumentNullException(nameof(ruleBodies));
            }

            _ruleBody = ruleBodies[_noneTerminalName];
        }

        public bool HasNonTerminalParsingRuleBodies => _ruleBody != null;

        public void SetMemoizer(Memoizer<(TNonTerminalName, int, bool), ParseResult<TTokenTypeName, TActionResult>> memoizer)
        {
            _memoizer = memoizer;
        }

        public ParseResult<TTokenTypeName, TActionResult> Parse(IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, int startIndex, bool mustConsumeTokens)
        {
            var currentInputs = (_noneTerminalName, startIndex, mustConsumeTokens);
            var knownOutput = _memoizer.GetOutput(currentInputs);
            if (knownOutput != default)
            {
                return knownOutput;
            }

            // Prevent this non-terminal from calling itself on the same start index
            bool hasLeftRecursion = _previousStartIndices.Contains(startIndex);
            if (hasLeftRecursion)
            {
                var fail = ParseResult<TTokenTypeName, TActionResult>.Failed(startIndex);
                _memoizer.Memoize(currentInputs, fail);
                return fail;
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

            _memoizer.Memoize(currentInputs, returnResult);

            return returnResult;
        }

        public override string ToString() => _noneTerminalName.ToString();
    }
}
