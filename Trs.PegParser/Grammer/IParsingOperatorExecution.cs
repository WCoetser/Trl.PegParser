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
    public interface IParsingOperatorExecution<TTokenTypeName, TNoneTerminalName, TActionResult>        
        where TTokenTypeName : Enum
        where TNoneTerminalName : Enum
    {
        ParseResult<TActionResult> Parse([NotNull] IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, int startPosition);

        /// <summary>
        /// This needs to be done in order to execute parsing.
        /// It matches the maching rule to the name of the non terminal.
        /// It should be done internally by the parser, therefore this method is marked as "internal"
        /// </summary>
        internal void SetNonTerminalParsingRuleBody(
            IDictionary<TNoneTerminalName, IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>> ruleBodies);

        internal bool HasNonTerminalParsingRuleBodies { get;  }
    }
}
