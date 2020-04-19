using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Trs.PegParser.Tokenization;

namespace Trs.PegParser.Grammer.Operators
{
    class Sequence<TTokenTypeName, TNoneTerminalName, TActionResult>
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
    }
}
