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
        private TNoneTerminalName _noneTerminalName;

        public NonTerminal(TNoneTerminalName noneTerminalName)
            => (_noneTerminalName) = noneTerminalName;

        public IEnumerable<TNoneTerminalName> GetNonTerminalNames()
            => new[] { _noneTerminalName };

        public ParseResult<TActionResult> Parse([NotNull] IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, int startPosition)
        {
            throw new NotImplementedException();
        }
    }
}
