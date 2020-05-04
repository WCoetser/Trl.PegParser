using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        private SemanticAction<TActionResult, TTokenTypeName> _defaultEmptyStringAction;
        private SemanticAction<TActionResult, TTokenTypeName> _defaultOptionalAction;
        private SemanticAction<TActionResult, TTokenTypeName> _defaultOrderedChoiceAction;
        private SemanticAction<TActionResult, TTokenTypeName> _defaultZeroOrMoreAction;
        private SemanticAction<TActionResult, TTokenTypeName> _defaultOneOrMoreAction;

        private Dictionary<TNonTerminalName, SemanticAction<TActionResult, TTokenTypeName>> _actionByNonTerminal
            = new Dictionary<TNonTerminalName, SemanticAction<TActionResult, TTokenTypeName>>();

        // ==================== Semantic actions

        public void SetDefaultOneOrMoreAction(SemanticAction<TActionResult, TTokenTypeName> semanticAction)
        => _defaultOneOrMoreAction = semanticAction;        

        public void SetDefaultZeroOrMoreAction(SemanticAction<TActionResult, TTokenTypeName> semanticAction)
        => _defaultZeroOrMoreAction = semanticAction;

        public void SetDefaultOptionalAction(SemanticAction<TActionResult, TTokenTypeName> semanticAction)
        => _defaultOptionalAction = semanticAction;

        public void SetDefaultOrderedChoiceAction(SemanticAction<TActionResult, TTokenTypeName> semanticAction)
        => _defaultOrderedChoiceAction = semanticAction;

        public void SetDefaultTerminalAction(TTokenTypeName tokenType, SemanticAction<TActionResult, TTokenTypeName> semanticAction)
            => _actionByTerminalToken[tokenType] = semanticAction;

        public void SetDefaultEmptyStringAction(SemanticAction<TActionResult, TTokenTypeName> semanticAction)
        => _defaultEmptyStringAction = semanticAction;

        public void SetDefaultNonTerminalAction(TNonTerminalName nonTerminalA, SemanticAction<TActionResult, TTokenTypeName> matchAction)
        => _actionByNonTerminal[nonTerminalA] = matchAction;

        public void SetDefaultSequenceAction(SemanticAction<TActionResult, TTokenTypeName> semanticAction)
        => _defaultSequenceAction = semanticAction;

        public ParsingRule<TTokenTypeName, TNonTerminalName, TActionResult> Rule(TNonTerminalName ruleHead,
            IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> ruleBody)
        => new ParsingRule<TTokenTypeName, TNonTerminalName, TActionResult>(ruleHead, ruleBody);

        // ==================== Parsing operators

        public OneOrMore<TTokenTypeName, TNonTerminalName, TActionResult> OneOrMore(
            IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> subExpression, SemanticAction<TActionResult, TTokenTypeName> semanticAction = null)
        {
            var matchAction = semanticAction ?? _defaultOneOrMoreAction;
            return new OneOrMore<TTokenTypeName, TNonTerminalName, TActionResult>(subExpression, matchAction);
        }

        public ZeroOrMore<TTokenTypeName, TNonTerminalName, TActionResult> ZeroOrMore(
            IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> subExpression, SemanticAction<TActionResult, TTokenTypeName> semanticAction = null)
        {
            var matchAction = semanticAction ?? _defaultZeroOrMoreAction;
            return new ZeroOrMore<TTokenTypeName, TNonTerminalName, TActionResult>(subExpression, matchAction);
        }

        public Sequence<TTokenTypeName, TNonTerminalName, TActionResult> Sequence(
            params IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>[] sequenceDefinitions)
        => Sequence(sequenceDefinitions, null);

        public Sequence<TTokenTypeName, TNonTerminalName, TActionResult> Sequence(
            IEnumerable<IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>> sequenceDefinitions,
            SemanticAction<TActionResult, TTokenTypeName> matchAction = null)
        => new Sequence<TTokenTypeName, TNonTerminalName, TActionResult>(sequenceDefinitions, matchAction ?? _defaultSequenceAction);

        public NonTerminal<TTokenTypeName, TNonTerminalName, TActionResult> NonTerminal(TNonTerminalName head, 
            SemanticAction<TActionResult, TTokenTypeName> semanticAction = null)
        {
            var action = semanticAction;
            _ = action == null && _actionByNonTerminal.TryGetValue(head, out action);
            return new NonTerminal<TTokenTypeName, TNonTerminalName, TActionResult>(head, action);
        }

        public EmptyString<TTokenTypeName, TNonTerminalName, TActionResult> EmptyString(
            SemanticAction<TActionResult, TTokenTypeName> semanticAction = null)
        {
            var matchAction = semanticAction ?? _defaultEmptyStringAction;
            return new EmptyString<TTokenTypeName, TNonTerminalName, TActionResult>(matchAction);
        }

        public Terminal<TTokenTypeName, TNonTerminalName, TActionResult> Terminal(
            TTokenTypeName expectedToken, SemanticAction<TActionResult, TTokenTypeName> semanticAction = null)
        {
            var matchAction = semanticAction;
            _ = matchAction == null && _actionByTerminalToken.TryGetValue(expectedToken, out matchAction);
            return new Terminal<TTokenTypeName, TNonTerminalName, TActionResult>(expectedToken, matchAction);
        }

        public Optional<TTokenTypeName, TNonTerminalName, TActionResult> Optional(
            IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> subExpression,
            SemanticAction<TActionResult, TTokenTypeName> semanticAction = null)
        {
            var matchAction = semanticAction ?? _defaultOptionalAction;
            return new Optional<TTokenTypeName, TNonTerminalName, TActionResult>(subExpression, matchAction);
        }

        public OrderedChoice<TTokenTypeName, TNonTerminalName, TActionResult> OrderedChoice(
            IEnumerable<IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>> orderedChoices,
            SemanticAction<TActionResult, TTokenTypeName> semanticAction = null)
        {
            var matchAction = semanticAction ?? _defaultOrderedChoiceAction;
            return new OrderedChoice<TTokenTypeName, TNonTerminalName, TActionResult>(orderedChoices, matchAction);
        }

        public OrderedChoice<TTokenTypeName, TNonTerminalName, TActionResult> OrderedChoice(
            params IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>[] orderedChoices)
        => OrderedChoice(orderedChoices, null);

        // ==================== The rest

        public TokenDefinition<TTokenTypeName> Token(TTokenTypeName tokenName, Regex tokenDefinition)
        => new TokenDefinition<TTokenTypeName>(tokenName, tokenDefinition);

        public Tokenizer<TTokenTypeName> Tokenizer(IEnumerable<TokenDefinition<TTokenTypeName>> prioritizedTokenDefinitions)
        => new Tokenizer<TTokenTypeName>(prioritizedTokenDefinitions);

        public Parser<TTokenTypeName, TNonTerminalName, TActionResult> Parser(TNonTerminalName startSymbol,
            IEnumerable<ParsingRule<TTokenTypeName, TNonTerminalName, TActionResult>> grammerRules)
        => new Parser<TTokenTypeName, TNonTerminalName, TActionResult>(startSymbol, grammerRules);

        /// <summary>
        /// This method exists to avoid re-typing all the generic constraints in relation to the 
        /// reset of the PEG parser in this facade. Therefore it just passes the input to the output.
        /// </summary>
        public SemanticAction<TActionResult, TTokenTypeName> SemanticAction(SemanticAction<TActionResult, TTokenTypeName> semanticAction)
            => semanticAction;
    }
}
