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
        /// <summary>
        /// This methed is used to execute the actual parsing of the input tokens and to execute semantic actions.
        /// </summary>
        /// <param name="inputTokens">The input tokens</param>
        /// <param name="startPosition">Start index in the inputTokens to parse from.</param>
        ParseResult<TTokenTypeName, TActionResult> Parse([NotNull] IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, int startPosition);

        /// <summary>
        /// This needs to be done in order to execute parsing.
        /// It matches the maching rule to the name of the non terminal.
        /// It should be done internally by the parser, therefore this method is marked as "internal"
        /// </summary>
        /// <param name="ruleBodies">Rules making up the parser, grouped by thier non-terminal head symbols.</param>
        void SetNonTerminalParsingRuleBody(
            IDictionary<TNoneTerminalName, IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>> ruleBodies);
                
        /// <summary>
        /// This is used to recursively walk through the expression tree of this operator to see if any non-terminals
        /// have definitions assigned to them. If this is the case, another parser is already using this operator
        /// and an error should be generated.
        /// </summary>
        bool HasNonTerminalParsingRuleBodies { get; } // TODO: Move this to validations interface
    }
}
