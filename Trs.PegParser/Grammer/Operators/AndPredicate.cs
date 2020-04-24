using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Trs.PegParser.Tokenization;

namespace Trs.PegParser.Grammer.Operators
{
    class AndPredicate<TTokenTypeName, TNoneTerminalName, TActionResult>
        : IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNoneTerminalName : Enum
    {
        public IEnumerable<TNoneTerminalName> GetNonTerminalNames()
        {
            throw new NotImplementedException();
        }

        public ParseResult<TActionResult> Parse([NotNull] IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, int startPosition)
        {
            throw new NotImplementedException();
        }

        IEnumerable<TNoneTerminalName> IParsingOperatorValidation<TNoneTerminalName>.GetNonTerminalNames()
        {
            throw new NotImplementedException();
        }

        ParseResult<TActionResult> IParsingOperatorExecution<TTokenTypeName, TNoneTerminalName, TActionResult>.Parse(IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, int startPosition)
        {
            throw new NotImplementedException();
        }

        void IParsingOperatorExecution<TTokenTypeName, TNoneTerminalName, TActionResult>.SetNonTerminalParsingRuleBody(IDictionary<TNoneTerminalName, IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>> ruleBodies)
        {
            throw new NotImplementedException();
        }

        bool IParsingOperatorExecution<TTokenTypeName, TNoneTerminalName, TActionResult>.HasNonTerminalParsingRuleBodies 
            => throw new NotImplementedException();
    }
}
