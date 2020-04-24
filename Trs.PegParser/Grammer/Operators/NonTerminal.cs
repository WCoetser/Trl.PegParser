using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Trs.PegParser.Tokenization;

namespace Trs.PegParser.Grammer.Operators
{
    public class NonTerminal<TTokenTypeName, TNoneTerminalName, TActionResult>
        : IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNoneTerminalName : Enum
    {
        private readonly TNoneTerminalName _noneTerminalName;
        private IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult> _ruleBody;

        public NonTerminal(TNoneTerminalName noneTerminalName)
            => _noneTerminalName = noneTerminalName;

        public IEnumerable<TNoneTerminalName> GetNonTerminalNames()
            => new[] { _noneTerminalName };
                
        void IParsingOperatorExecution<TTokenTypeName, TNoneTerminalName, TActionResult>
            .SetNonTerminalParsingRuleBody(
                IDictionary<TNoneTerminalName, IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>> ruleBodies)
        {
            _ruleBody = ruleBodies[_noneTerminalName];
        }

        bool IParsingOperatorExecution<TTokenTypeName, TNoneTerminalName, TActionResult>.HasNonTerminalParsingRuleBodies
            => _ruleBody != null;

        public ParseResult<TActionResult> Parse([NotNull] IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, int startPosition)
        {
            throw new NotImplementedException();
        }
    }
}
