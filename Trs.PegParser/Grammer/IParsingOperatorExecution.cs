using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Trs.PegParser.Tokenization;

namespace Trs.PegParser.Grammer
{
    /// <summary>
    /// Interface specifying PEG operators.
    /// </summary>
    /// <typeparam name="TTokenTypeName">The enum used to identify token types.</typeparam>
    /// <typeparam name="TActionResult">The result type for the semantic actions that are executed when a match occurs.</typeparam>
    public interface IParsingOperatorExecution<TTokenTypeName, TActionResult>
        where TTokenTypeName : Enum
    {
        ParseResult<TActionResult> Parse([NotNull] IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, int startPosition);
    }        
}
