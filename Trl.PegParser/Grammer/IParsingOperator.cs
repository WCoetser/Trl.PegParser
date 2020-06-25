using System;
using System.Collections.Generic;
using Trl.PegParser.DataStructures;
using Trl.PegParser.Tokenization;

namespace Trl.PegParser.Grammer
{
    public interface IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNonTerminalName : Enum
    {
        /// <summary>
        /// This methed is used to execute the actual parsing of the input tokens and to execute semantic actions.
        /// </summary>
        /// <param name="inputTokens">The input tokens</param>
        /// <param name="startIndex">Start index in the inputTokens to parse from.</param>
        /// <param name="mustConsumeTokens">Indicates whether semantic actions must execute when a token is matched.</param>
        ParseResult<TTokenTypeName, TActionResult> Parse(IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, int startIndex, bool mustConsumeTokens);

        /// <summary>
        /// This needs to be done in order to execute parsing.
        /// It matches the maching rule to the name of the non terminal.
        /// It should be done internally by the parser, therefore this method is marked as "internal"
        /// </summary>
        /// <param name="ruleBodies">Rules making up the parser, grouped by thier non-terminal head symbols.</param>
        void SetNonTerminalParsingRuleBody(
            IDictionary<TNonTerminalName, IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>> ruleBodies);

        /// <summary>
        /// This is used to recursively walk through the expression tree of this operator to see if any non-terminals
        /// have definitions assigned to them. If this is the case, another parser is already using this operator
        /// and an error should be generated.
        /// </summary>
        bool HasNonTerminalParsingRuleBodies { get; }

        /// <summary>
        /// Used for validations - all non-terminals must have corresponding parsing rules.
        /// </summary>
        IEnumerable<TNonTerminalName> GetNonTerminalNames();

        /// <summary>
        /// Memoizer used to avoid exponential parse time for grammers with multiple left-recursive nonterminals
        /// in choice operators.
        /// </summary>
        void SetMemoizer(Memoizer<(TNonTerminalName, int, bool), ParseResult<TTokenTypeName, TActionResult>> memoizer);

        /// <summary>
        /// Gets the parser generator representation of this operator.
        /// This is lazy loaded to avoid repeatedly traversing the PEG tree representation
        /// while passing the spec to the semantic function to indicate what has been parsed.
        /// </summary>
        Lazy<string> ToParserSpec { get; }
    }
}
