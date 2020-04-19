using System;

namespace Trs.PegParser.Grammer
{
    /// <summary>
    /// Represents a parsing rule and defines a non-terminal.
    /// </summary>
    /// <typeparam name="TTokenTypeName">Token types are identified with this. See <see cref="Tokenization.Tokenizer{TTokenTypeName}"/></typeparam>
    /// <typeparam name="TNonTerminalName">Non-terminals are identified by members of this enumeration.</typeparam>
    /// <typeparam name="TActionResult">The result of the semantic actions associated with the parsing expression has this external type.</typeparam>
    public class ParsingRule<TTokenTypeName, TNonTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNonTerminalName: Enum
    {
        /// <summary>
        /// Head used to identify this parsing expression.
        /// </summary>
        public TNonTerminalName RuleIdentifier { get; }

        /// <summary>
        /// Definition of the parsing expression rule.
        /// </summary>
        public IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> ParsingExpression { get; } 

        public ParsingRule(TNonTerminalName ruleIdentifier, IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> parsingExpression)
        {
            if (parsingExpression == null)
            {
                throw new ArgumentNullException(nameof(parsingExpression), "Rule body may not be null.");
            }

            RuleIdentifier = ruleIdentifier;
            ParsingExpression = parsingExpression;            
        }
    }
}
