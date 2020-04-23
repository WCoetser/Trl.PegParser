using System;
using System.Collections.Generic;
using Trs.PegParser.Grammer;
using Trs.PegParser.Grammer.Operators;
using Trs.PegParser.Tokenization;

namespace Trs.PegParser
{
    /// <summary>
    /// Facade for creating PEG parser.
    /// 
    /// NB: The purpose of this class is to avoid typing all the generic constraints repeatedly over and over.
    /// 
    /// </summary>
    /// <typeparam name="TTokenTypeName">Enum to name tokens.</typeparam>
    /// <typeparam name="TNonTerminalName">Enum used to name non-terminals and parsing rule heads.</typeparam>
    /// <typeparam name="TActionResult">Result of evaluation a match, ex. numbers for a calculator or AST node type for a compiler.</typeparam>
    public class PegFacade<TTokenTypeName, TNonTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNonTerminalName : Enum
    {
        private Dictionary<TTokenTypeName, SemanticAction<TActionResult, TTokenTypeName>> _actionByTerminalToken
            = new Dictionary<TTokenTypeName, SemanticAction<TActionResult, TTokenTypeName>>();
        private SemanticAction<TActionResult, TTokenTypeName> _defaultSequenceAction;

        public void SetTerminalDefaultAction(TTokenTypeName tokenType, SemanticAction<TActionResult, TTokenTypeName> semanticAction)
            => _actionByTerminalToken[tokenType] = semanticAction;

        public void SetDefaultSequenceAction(SemanticAction<TActionResult, TTokenTypeName> semanticAction)
            => _defaultSequenceAction = semanticAction;

        public Tokenizer<TTokenTypeName> Tokenizer(IEnumerable<TokenDefinition<TTokenTypeName>> prioritizedTokenDefinitions)
        => new Tokenizer<TTokenTypeName>(prioritizedTokenDefinitions);

        public Parser<TTokenTypeName, TNonTerminalName, TActionResult> Parser(TNonTerminalName startSymbol,
            IEnumerable<ParsingRule<TTokenTypeName, TNonTerminalName, TActionResult>> grammerRules)
        => new Parser<TTokenTypeName, TNonTerminalName, TActionResult>(startSymbol, grammerRules);

        public IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> Sequence(
            params IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>[] sequenceDefinitions)
        => Sequence(sequenceDefinitions, null);

        public IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> Sequence(
            IEnumerable<IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>> sequenceDefinitions,
            SemanticAction<TActionResult, TTokenTypeName> matchAction = null)
        => new Sequence<TTokenTypeName, TNonTerminalName, TActionResult>(sequenceDefinitions, matchAction ?? _defaultSequenceAction);


        /// <summary>
        /// This method exists to avoid re-typing all the generic constraints in relation to the 
        /// reset of the PEG parser in this facade. Therefore it just passes the input to the output.
        /// </summary>
        public SemanticAction<TActionResult, TTokenTypeName> SemanticAction(SemanticAction<TActionResult, TTokenTypeName> semanticAction)
            => semanticAction;

        public ParsingRule<TTokenTypeName, TNonTerminalName, TActionResult> Rule(TNonTerminalName ruleHead,
            IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> ruleBody)
        => new ParsingRule<TTokenTypeName, TNonTerminalName, TActionResult>(ruleHead, ruleBody);

        public Terminal<TTokenTypeName, TNonTerminalName, TActionResult> Terminal(
            TTokenTypeName expectedToken, SemanticAction<TActionResult, TTokenTypeName> matchAction = null)
        {
            var semanticAction = matchAction;
            if (semanticAction == null && _actionByTerminalToken.TryGetValue(expectedToken, 
                out SemanticAction<TActionResult, TTokenTypeName> action))
            {
                semanticAction = action;
            }
            return new Terminal<TTokenTypeName, TNonTerminalName, TActionResult>(expectedToken,  semanticAction);
        }
    }
}
